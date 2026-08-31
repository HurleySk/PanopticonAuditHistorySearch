using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using PanopticonAuditHistorySearch.Model;

namespace PanopticonAuditHistorySearch.Services
{
    public class AuditCache : IDisposable
    {
        public const int MaxMaterializedResults = 250000;
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly SqliteConnection _connection;
        private readonly object _gate = new object();
        private bool _disposed;

        public object Gate { get { return _gate; } }

        public string DatabasePath { get; private set; }

        public AuditCache(string databasePath)
        {
            DatabasePath = databasePath;
            CacheLocator.EnsureRoot();
            _connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = databasePath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Private,
                Pooling = false
            }.ToString());
            _connection.Open();
            Exec("PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA temp_store=MEMORY;");
            EnsureSchema();
        }

        private void EnsureSchema()
        {
            Exec(@"
CREATE TABLE IF NOT EXISTS entity(
  otc INTEGER PRIMARY KEY, logical_name TEXT NOT NULL, display_name TEXT);
CREATE TABLE IF NOT EXISTS attribute(
  otc INTEGER NOT NULL, column_number INTEGER NOT NULL,
  logical_name TEXT NOT NULL, display_name TEXT,
  PRIMARY KEY(otc, column_number));
CREATE TABLE IF NOT EXISTS principal(id BLOB PRIMARY KEY, name TEXT);
CREATE TABLE IF NOT EXISTS object_name(
  otc INTEGER NOT NULL, object_id BLOB NOT NULL, name TEXT,
  PRIMARY KEY(otc, object_id));
CREATE TABLE IF NOT EXISTS audit(
  auditid BLOB PRIMARY KEY, created_on INTEGER NOT NULL, otc INTEGER NOT NULL,
  object_id BLOB, user_id BLOB, calling_user_id BLOB,
  action INTEGER, operation INTEGER, transaction_id BLOB, attribute_mask TEXT);
CREATE TABLE IF NOT EXISTS audit_field(auditid BLOB NOT NULL, column_number INTEGER NOT NULL);
CREATE TABLE IF NOT EXISTS audit_detail(
  auditid BLOB PRIMARY KEY, fetched_on INTEGER NOT NULL, payload TEXT);
CREATE TABLE IF NOT EXISTS sync_window(
  otc INTEGER NOT NULL, from_utc INTEGER NOT NULL, to_utc INTEGER NOT NULL,
  completed_on INTEGER NOT NULL, rows_loaded INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY(otc, from_utc, to_utc));

CREATE INDEX IF NOT EXISTS ix_audit_otc_date ON audit(otc, created_on DESC);
CREATE INDEX IF NOT EXISTS ix_audit_user_date ON audit(user_id, created_on DESC);
CREATE INDEX IF NOT EXISTS ix_audit_object ON audit(object_id);
CREATE INDEX IF NOT EXISTS ix_audit_txn ON audit(transaction_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_field ON audit_field(column_number, auditid);
CREATE INDEX IF NOT EXISTS ix_field_audit ON audit_field(auditid);");
        }

        public static long ToUnix(DateTime utc)
        {
            return (long)(utc.ToUniversalTime() - Epoch).TotalMilliseconds;
        }

        public static DateTime FromUnix(long ms)
        {
            return Epoch.AddMilliseconds(ms);
        }

        public void SaveEntities(IEnumerable<EntityScope> entities)
        {
            lock (Gate)
            {
                using (var tx = _connection.BeginTransaction())
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText =
                        "INSERT INTO entity(otc, logical_name, display_name) VALUES($o,$l,$d) " +
                        "ON CONFLICT(otc) DO UPDATE SET logical_name=$l, display_name=$d";
                    var o = cmd.Parameters.Add("$o", SqliteType.Integer);
                    var l = cmd.Parameters.Add("$l", SqliteType.Text);
                    var d = cmd.Parameters.Add("$d", SqliteType.Text);
                    foreach (var e in entities)
                    {
                        o.Value = e.ObjectTypeCode;
                        l.Value = e.LogicalName;
                        d.Value = (object)e.DisplayName ?? DBNull.Value;
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
        }

        public void SaveColumns(int objectTypeCode, IEnumerable<ColumnInfo> columns)
        {
            lock (Gate)
            {
                using (var tx = _connection.BeginTransaction())
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText =
                        "INSERT INTO attribute(otc, column_number, logical_name, display_name) VALUES($o,$c,$l,$d) " +
                        "ON CONFLICT(otc, column_number) DO UPDATE SET logical_name=$l, display_name=$d";
                    var o = cmd.Parameters.Add("$o", SqliteType.Integer);
                    var c = cmd.Parameters.Add("$c", SqliteType.Integer);
                    var l = cmd.Parameters.Add("$l", SqliteType.Text);
                    var d = cmd.Parameters.Add("$d", SqliteType.Text);
                    foreach (var column in columns)
                    {
                        o.Value = objectTypeCode;
                        c.Value = column.ColumnNumber;
                        l.Value = column.LogicalName;
                        d.Value = (object)column.DisplayName ?? DBNull.Value;
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
        }

        public int SaveAuditRows(IList<AuditRow> rows)
        {
            lock (Gate)
            {
                if (rows.Count == 0) return 0;

                using (var tx = _connection.BeginTransaction())
                {
                    using (var cmd = _connection.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText =
                            "INSERT OR REPLACE INTO audit(auditid, created_on, otc, object_id, user_id, " +
                            "calling_user_id, action, operation, transaction_id, attribute_mask) " +
                            "VALUES($a,$c,$o,$oi,$u,$cu,$ac,$op,$t,$m)";
                        var a = cmd.Parameters.Add("$a", SqliteType.Blob);
                        var c = cmd.Parameters.Add("$c", SqliteType.Integer);
                        var o = cmd.Parameters.Add("$o", SqliteType.Integer);
                        var oi = cmd.Parameters.Add("$oi", SqliteType.Blob);
                        var u = cmd.Parameters.Add("$u", SqliteType.Blob);
                        var cu = cmd.Parameters.Add("$cu", SqliteType.Blob);
                        var ac = cmd.Parameters.Add("$ac", SqliteType.Integer);
                        var op = cmd.Parameters.Add("$op", SqliteType.Integer);
                        var t = cmd.Parameters.Add("$t", SqliteType.Blob);
                        var m = cmd.Parameters.Add("$m", SqliteType.Text);

                        foreach (var row in rows)
                        {
                            a.Value = row.AuditId.ToByteArray();
                            c.Value = ToUnix(row.CreatedOn);
                            o.Value = row.ObjectTypeCode;
                            oi.Value = Blob(row.ObjectId);
                            u.Value = Blob(row.UserId);
                            cu.Value = Blob(row.CallingUserId);
                            ac.Value = row.Action;
                            op.Value = row.Operation;
                            t.Value = Blob(row.TransactionId);
                            m.Value = (object)row.AttributeMask ?? DBNull.Value;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    using (var del = _connection.CreateCommand())
                    using (var ins = _connection.CreateCommand())
                    {
                        del.Transaction = tx;
                        del.CommandText = "DELETE FROM audit_field WHERE auditid = $a";
                        var da = del.Parameters.Add("$a", SqliteType.Blob);

                        ins.Transaction = tx;
                        ins.CommandText = "INSERT OR IGNORE INTO audit_field(auditid, column_number) VALUES($a,$c)";
                        var ia = ins.Parameters.Add("$a", SqliteType.Blob);
                        var ic = ins.Parameters.Add("$c", SqliteType.Integer);

                        foreach (var row in rows)
                        {
                            var bytes = row.AuditId.ToByteArray();
                            da.Value = bytes;
                            del.ExecuteNonQuery();

                            foreach (var number in ParseMask(row.AttributeMask))
                            {
                                ia.Value = bytes;
                                ic.Value = number;
                                ins.ExecuteNonQuery();
                            }
                        }
                    }

                    tx.Commit();
                }

                return rows.Count;
            }
        }

        public static IEnumerable<int> ParseMask(string mask)
        {
            if (string.IsNullOrWhiteSpace(mask)) yield break;
            foreach (var part in mask.Split(','))
            {
                int number;
                if (int.TryParse(part.Trim(), out number) && number > 0)
                    yield return number;
            }
        }

        public void SavePrincipals(IDictionary<Guid, string> names)
        {
            lock (Gate)
            {
                if (names.Count == 0) return;
                using (var tx = _connection.BeginTransaction())
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = "INSERT OR REPLACE INTO principal(id, name) VALUES($i,$n)";
                    var i = cmd.Parameters.Add("$i", SqliteType.Blob);
                    var n = cmd.Parameters.Add("$n", SqliteType.Text);
                    foreach (var pair in names)
                    {
                        i.Value = pair.Key.ToByteArray();
                        n.Value = (object)pair.Value ?? DBNull.Value;
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
        }

        public void SaveObjectNames(int objectTypeCode, IDictionary<Guid, string> names)
        {
            lock (Gate)
            {
                if (names.Count == 0) return;
                using (var tx = _connection.BeginTransaction())
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = "INSERT OR REPLACE INTO object_name(otc, object_id, name) VALUES($o,$i,$n)";
                    var o = cmd.Parameters.Add("$o", SqliteType.Integer);
                    var i = cmd.Parameters.Add("$i", SqliteType.Blob);
                    var n = cmd.Parameters.Add("$n", SqliteType.Text);
                    foreach (var pair in names)
                    {
                        o.Value = objectTypeCode;
                        i.Value = pair.Key.ToByteArray();
                        n.Value = (object)pair.Value ?? DBNull.Value;
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
        }

        public void MarkWindowComplete(int objectTypeCode, DateRange window, int rowsLoaded)
        {
            lock (Gate)
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT OR REPLACE INTO sync_window(otc, from_utc, to_utc, completed_on, rows_loaded) " +
                        "VALUES($o,$f,$t,$c,$r)";
                    cmd.Parameters.AddWithValue("$o", objectTypeCode);
                    cmd.Parameters.AddWithValue("$f", ToUnix(window.FromUtc));
                    cmd.Parameters.AddWithValue("$t", ToUnix(window.ToUtc));
                    cmd.Parameters.AddWithValue("$c", ToUnix(DateTime.UtcNow));
                    cmd.Parameters.AddWithValue("$r", rowsLoaded);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool IsWindowComplete(int objectTypeCode, DateRange window)
        {
            lock (Gate)
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT 1 FROM sync_window WHERE otc=$o AND from_utc=$f AND to_utc=$t";
                    cmd.Parameters.AddWithValue("$o", objectTypeCode);
                    cmd.Parameters.AddWithValue("$f", ToUnix(window.FromUtc));
                    cmd.Parameters.AddWithValue("$t", ToUnix(window.ToUtc));
                    return cmd.ExecuteScalar() != null;
                }
            }
        }

        public void ForgetWindows(int objectTypeCode)
        {
            lock (Gate)
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM sync_window WHERE otc=$o";
                    cmd.Parameters.AddWithValue("$o", objectTypeCode);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public AuditDetailPayload GetDetail(Guid auditId)
        {
            lock (Gate)
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT payload FROM audit_detail WHERE auditid=$a";
                    cmd.Parameters.AddWithValue("$a", auditId.ToByteArray());
                    var value = cmd.ExecuteScalar() as string;
                    return value == null ? null : DetailSerializer.Deserialize(value);
                }
            }
        }

        public void SaveDetail(AuditDetailPayload payload)
        {
            lock (Gate)
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT OR REPLACE INTO audit_detail(auditid, fetched_on, payload) VALUES($a,$f,$p)";
                    cmd.Parameters.AddWithValue("$a", payload.AuditId.ToByteArray());
                    cmd.Parameters.AddWithValue("$f", ToUnix(DateTime.UtcNow));
                    cmd.Parameters.AddWithValue("$p", DetailSerializer.Serialize(payload));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public CacheStats Stats()
        {
            lock (Gate)
            {
                var stats = new CacheStats { SizeBytes = CacheLocator.SizeOnDisk(DatabasePath) };
                stats.AuditRows = Scalar("SELECT COUNT(*) FROM audit");
                stats.DetailRows = Scalar("SELECT COUNT(*) FROM audit_detail");
                stats.Entities = Scalar("SELECT COUNT(DISTINCT otc) FROM audit");
                var min = ScalarOrNull("SELECT MIN(created_on) FROM audit");
                var max = ScalarOrNull("SELECT MAX(created_on) FROM audit");
                if (min.HasValue) stats.OldestUtc = FromUnix(min.Value);
                if (max.HasValue) stats.NewestUtc = FromUnix(max.Value);
                return stats;
            }
        }

        public void Dispose()
        {
            lock (Gate)
            {
                if (_disposed) return;
                _disposed = true;
                try { Exec("PRAGMA optimize; PRAGMA wal_checkpoint(TRUNCATE);"); }
                catch (SqliteException) { }
                _connection.Close();
                SqliteConnection.ClearPool(_connection);
                _connection.Dispose();
            }
        }

        internal SqliteConnection Connection { get { return _connection; } }

        private static object Blob(Guid value)
        {
            return value == Guid.Empty ? (object)DBNull.Value : value.ToByteArray();
        }

        private void Exec(string sql)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        private long Scalar(string sql)
        {
            var value = ScalarOrNull(sql);
            return value ?? 0;
        }

        private long? ScalarOrNull(string sql)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = sql;
                var result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value) return null;
                return Convert.ToInt64(result);
            }
        }
    }

    public class CacheStats
    {
        public long AuditRows { get; set; }
        public long DetailRows { get; set; }
        public long Entities { get; set; }
        public long SizeBytes { get; set; }
        public DateTime? OldestUtc { get; set; }
        public DateTime? NewestUtc { get; set; }

        public string Describe()
        {
            if (AuditRows == 0) return "Cache empty.";
            return string.Format(
                "{0:N0} rows across {1} table(s)  |  {2:N0} with values cached  |  {3} on disk  |  {4:d} to {5:d}",
                AuditRows, Entities, DetailRows, SyncEstimate.FormatBytes(SizeBytes),
                OldestUtc ?? DateTime.MinValue, NewestUtc ?? DateTime.MinValue);
        }
    }
}
