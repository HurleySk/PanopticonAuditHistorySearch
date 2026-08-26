using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Sqlite;
using PanopticonAuditHistorySearch.Model;

namespace PanopticonAuditHistorySearch.Services
{
    public class AuditSearch
    {
        private readonly AuditCache _cache;

        public AuditSearch(AuditCache cache)
        {
            _cache = cache;
        }

        public SearchResult Run(SearchCriteria criteria)
        {
            var where = new WhereClause(criteria);

            var total = Count(where);
            var capped = Math.Min(total, AuditCache.MaxMaterializedResults);

            Exec("DROP TABLE IF EXISTS temp.result_set");
            Exec("CREATE TABLE temp.result_set(position INTEGER PRIMARY KEY, auditid BLOB NOT NULL)");

            using (var cmd = _cache.Connection.CreateCommand())
            {
                cmd.CommandText =
                    "INSERT INTO temp.result_set(auditid) SELECT a.auditid FROM audit a " +
                    where.Sql + " ORDER BY a.created_on DESC, a.auditid LIMIT " + AuditCache.MaxMaterializedResults;
                where.Bind(cmd);
                cmd.ExecuteNonQuery();
            }

            return new SearchResult
            {
                TotalMatched = total,
                Available = capped,
                Truncated = total > capped
            };
        }

        public IList<AuditRow> Page(int startIndex, int count)
        {
            var rows = new List<AuditRow>(count);
            using (var cmd = _cache.Connection.CreateCommand())
            {
                cmd.CommandText = @"
SELECT a.auditid, a.created_on, a.otc, a.object_id, a.user_id, a.calling_user_id,
       a.action, a.operation, a.transaction_id, a.attribute_mask,
       e.logical_name, e.display_name, p.name, o.name
FROM temp.result_set r
JOIN audit a ON a.auditid = r.auditid
LEFT JOIN entity e ON e.otc = a.otc
LEFT JOIN principal p ON p.id = a.user_id
LEFT JOIN object_name o ON o.otc = a.otc AND o.object_id = a.object_id
WHERE r.position > $from AND r.position <= $to
ORDER BY r.position";
                cmd.Parameters.AddWithValue("$from", startIndex);
                cmd.Parameters.AddWithValue("$to", startIndex + count);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) rows.Add(ReadRow(reader));
                }
            }

            AttachChangedFields(rows);
            return rows;
        }

        public IList<AuditRow> All(int limit)
        {
            var rows = new List<AuditRow>();
            var batch = 5000;
            for (var start = 0; start < limit; start += batch)
            {
                var page = Page(start, Math.Min(batch, limit - start));
                if (page.Count == 0) break;
                rows.AddRange(page);
            }
            return rows;
        }

        public IList<Guid> UnresolvedUserIds(int limit)
        {
            return ReadGuids(@"
SELECT DISTINCT a.user_id FROM temp.result_set r
JOIN audit a ON a.auditid = r.auditid
LEFT JOIN principal p ON p.id = a.user_id
WHERE a.user_id IS NOT NULL AND p.id IS NULL
LIMIT " + limit);
        }

        public IDictionary<int, IList<Guid>> UnresolvedObjectIds(int startIndex, int count)
        {
            var result = new Dictionary<int, IList<Guid>>();
            using (var cmd = _cache.Connection.CreateCommand())
            {
                cmd.CommandText = @"
SELECT DISTINCT a.otc, a.object_id FROM temp.result_set r
JOIN audit a ON a.auditid = r.auditid
LEFT JOIN object_name o ON o.otc = a.otc AND o.object_id = a.object_id
WHERE r.position > $from AND r.position <= $to
  AND a.object_id IS NOT NULL AND o.object_id IS NULL";
                cmd.Parameters.AddWithValue("$from", startIndex);
                cmd.Parameters.AddWithValue("$to", startIndex + count);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var otc = reader.GetInt32(0);
                        IList<Guid> list;
                        if (!result.TryGetValue(otc, out list))
                        {
                            list = new List<Guid>();
                            result[otc] = list;
                        }
                        list.Add(ReadGuid(reader, 1).Value);
                    }
                }
            }
            return result;
        }

        public IList<Guid> AuditIdsWithoutDetails(int startIndex, int count)
        {
            return ReadGuids(string.Format(@"
SELECT r.auditid FROM temp.result_set r
LEFT JOIN audit_detail d ON d.auditid = r.auditid
WHERE r.position > {0} AND r.position <= {1} AND d.auditid IS NULL
ORDER BY r.position", startIndex, startIndex + count));
        }

        public IList<FacetValue> UserFacet(SearchCriteria criteria)
        {
            var where = new WhereClause(criteria);
            var facets = new List<FacetValue>();
            using (var cmd = _cache.Connection.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT a.user_id, COALESCE(p.name, '(unresolved)'), COUNT(*) FROM audit a " +
                    "LEFT JOIN principal p ON p.id = a.user_id " + where.Sql +
                    " GROUP BY a.user_id ORDER BY COUNT(*) DESC LIMIT 200";
                where.Bind(cmd);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = ReadGuid(reader, 0);
                        if (!id.HasValue) continue;
                        facets.Add(new FacetValue
                        {
                            Key = id.Value,
                            Label = reader.GetString(1),
                            Count = reader.GetInt64(2)
                        });
                    }
                }
            }
            return facets;
        }

        private long Count(WhereClause where)
        {
            using (var cmd = _cache.Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM audit a " + where.Sql;
                where.Bind(cmd);
                return Convert.ToInt64(cmd.ExecuteScalar());
            }
        }

        private void AttachChangedFields(IList<AuditRow> rows)
        {
            if (rows.Count == 0) return;

            var byOtc = rows.GroupBy(r => r.ObjectTypeCode);
            foreach (var group in byOtc)
            {
                var names = ColumnNames(group.Key);
                foreach (var row in group)
                {
                    var fields = new List<string>();
                    foreach (var number in AuditCache.ParseMask(row.AttributeMask))
                    {
                        string name;
                        fields.Add(names.TryGetValue(number, out name) ? name : "#" + number);
                    }
                    row.ChangedFields = fields;
                }
            }
        }

        private Dictionary<int, string> ColumnNames(int objectTypeCode)
        {
            var map = new Dictionary<int, string>();
            using (var cmd = _cache.Connection.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT column_number, COALESCE(display_name, logical_name) FROM attribute WHERE otc = $o";
                cmd.Parameters.AddWithValue("$o", objectTypeCode);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) map[reader.GetInt32(0)] = reader.GetString(1);
                }
            }
            return map;
        }

        private static AuditRow ReadRow(SqliteDataReader reader)
        {
            return new AuditRow
            {
                AuditId = ReadGuid(reader, 0).Value,
                CreatedOn = AuditCache.FromUnix(reader.GetInt64(1)),
                ObjectTypeCode = reader.GetInt32(2),
                ObjectId = ReadGuid(reader, 3) ?? Guid.Empty,
                UserId = ReadGuid(reader, 4) ?? Guid.Empty,
                CallingUserId = ReadGuid(reader, 5) ?? Guid.Empty,
                Action = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                Operation = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                TransactionId = ReadGuid(reader, 8) ?? Guid.Empty,
                AttributeMask = reader.IsDBNull(9) ? null : reader.GetString(9),
                EntityLogicalName = reader.IsDBNull(10) ? null : reader.GetString(10),
                EntityDisplayName = reader.IsDBNull(11) ? null : reader.GetString(11),
                UserName = reader.IsDBNull(12) ? null : reader.GetString(12),
                ObjectName = reader.IsDBNull(13) ? null : reader.GetString(13)
            };
        }

        private static Guid? ReadGuid(SqliteDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal)) return null;
            var bytes = (byte[])reader.GetValue(ordinal);
            return bytes.Length == 16 ? new Guid(bytes) : (Guid?)null;
        }

        private IList<Guid> ReadGuids(string sql)
        {
            var list = new List<Guid>();
            using (var cmd = _cache.Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = ReadGuid(reader, 0);
                        if (id.HasValue) list.Add(id.Value);
                    }
                }
            }
            return list;
        }

        private void Exec(string sql)
        {
            using (var cmd = _cache.Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        private class WhereClause
        {
            private readonly List<KeyValuePair<string, object>> _parameters =
                new List<KeyValuePair<string, object>>();

            public string Sql { get; private set; }

            public WhereClause(SearchCriteria criteria)
            {
                var clauses = new List<string>();

                AddInts(clauses, "a.otc", criteria.ObjectTypeCodes, "otc");
                AddInts(clauses, "a.action", criteria.Actions, "act");
                AddInts(clauses, "a.operation", criteria.Operations, "op");

                if (criteria.UserIds != null && criteria.UserIds.Count > 0)
                {
                    var names = new List<string>();
                    for (var i = 0; i < criteria.UserIds.Count; i++)
                    {
                        var name = "$usr" + i;
                        names.Add(name);
                        _parameters.Add(new KeyValuePair<string, object>(name, criteria.UserIds[i].ToByteArray()));
                    }
                    clauses.Add("a.user_id IN (" + string.Join(",", names) + ")");
                }

                if (criteria.ChangedFields != null && criteria.ChangedFields.Count > 0)
                {
                    var perEntity = new List<string>();
                    var index = 0;
                    foreach (var group in criteria.ChangedFields.GroupBy(f => f.ObjectTypeCode))
                    {
                        var otcName = "$fotc" + index;
                        _parameters.Add(new KeyValuePair<string, object>(otcName, group.Key));

                        var columnNames = new List<string>();
                        foreach (var selector in group)
                        {
                            var name = "$fcol" + index + "_" + columnNames.Count;
                            columnNames.Add(name);
                            _parameters.Add(new KeyValuePair<string, object>(name, selector.ColumnNumber));
                        }

                        perEntity.Add("(a.otc = " + otcName + " AND f.column_number IN ("
                                      + string.Join(",", columnNames) + "))");
                        index++;
                    }

                    clauses.Add("EXISTS (SELECT 1 FROM audit_field f WHERE f.auditid = a.auditid AND ("
                                + string.Join(" OR ", perEntity) + "))");
                }

                if (criteria.FromUtc.HasValue)
                {
                    clauses.Add("a.created_on >= $from");
                    _parameters.Add(new KeyValuePair<string, object>("$from", AuditCache.ToUnix(criteria.FromUtc.Value)));
                }

                if (criteria.ToUtc.HasValue)
                {
                    clauses.Add("a.created_on <= $to");
                    _parameters.Add(new KeyValuePair<string, object>("$to", AuditCache.ToUnix(criteria.ToUtc.Value)));
                }

                if (criteria.ObjectId.HasValue)
                {
                    clauses.Add("a.object_id = $oid");
                    _parameters.Add(new KeyValuePair<string, object>("$oid", criteria.ObjectId.Value.ToByteArray()));
                }

                if (!string.IsNullOrWhiteSpace(criteria.ObjectNameContains))
                {
                    clauses.Add("EXISTS (SELECT 1 FROM object_name n WHERE n.otc = a.otc " +
                                "AND n.object_id = a.object_id AND n.name LIKE $name)");
                    _parameters.Add(new KeyValuePair<string, object>("$name", "%" + criteria.ObjectNameContains.Trim() + "%"));
                }

                Sql = clauses.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", clauses);
            }

            private void AddInts(List<string> clauses, string column, IList<int> values, string prefix)
            {
                if (values == null || values.Count == 0) return;
                var names = new List<string>();
                for (var i = 0; i < values.Count; i++)
                {
                    var name = "$" + prefix + i;
                    names.Add(name);
                    _parameters.Add(new KeyValuePair<string, object>(name, values[i]));
                }
                clauses.Add(column + " IN (" + string.Join(",", names) + ")");
            }

            public void Bind(SqliteCommand command)
            {
                foreach (var parameter in _parameters)
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }
        }
    }

    public class SearchResult
    {
        public long TotalMatched { get; set; }
        public long Available { get; set; }
        public bool Truncated { get; set; }

        public string Describe()
        {
            if (TotalMatched == 0) return "No matching audit rows in the cache.";
            if (!Truncated) return string.Format("{0:N0} matching rows.", TotalMatched);
            return string.Format("{0:N0} matching rows; showing the {1:N0} most recent. Narrow the filters to see the rest.",
                TotalMatched, Available);
        }
    }

    public class FacetValue
    {
        public Guid Key { get; set; }
        public string Label { get; set; }
        public long Count { get; set; }
        public override string ToString() { return Label + "  (" + Count.ToString("N0") + ")"; }
    }
}
