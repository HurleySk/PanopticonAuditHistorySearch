using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PanopticonAuditHistorySearch.Model;

namespace PanopticonAuditHistorySearch.Services
{
    public class SyncEngine
    {
        private readonly AuditCache _cache;
        private readonly AuditQueryService _query;
        private readonly MetadataCatalog _catalog;

        public SyncEngine(AuditCache cache, AuditQueryService query, MetadataCatalog catalog)
        {
            _cache = cache;
            _query = query;
            _catalog = catalog;
        }

        public SyncOutcome Run(SyncScope scope, bool forceRefresh, IProgress<SyncProgress> progress,
            CancellationToken token)
        {
            var outcome = new SyncOutcome();
            var watch = Stopwatch.StartNew();

            _cache.SaveEntities(scope.Entities);

            foreach (var entity in scope.Entities)
            {
                token.ThrowIfCancellationRequested();
                if (forceRefresh) _cache.ForgetWindows(entity.ObjectTypeCode);
                _cache.SaveColumns(entity.ObjectTypeCode, _catalog.Columns(entity).Values);
            }

            var units = scope.Units().ToList();
            var completed = 0;

            foreach (var unit in units)
            {
                token.ThrowIfCancellationRequested();
                completed++;

                if (_cache.IsWindowComplete(unit.Entity.ObjectTypeCode, unit.Window))
                {
                    outcome.WindowsSkipped++;
                    Report(progress, completed, units.Count, unit, outcome, "already cached");
                    continue;
                }

                var loaded = 0;
                foreach (var page in _query.Pages(unit.Entity, unit.Window, token))
                {
                    loaded += _cache.SaveAuditRows(page);
                    outcome.RowsLoaded += page.Count;
                    Report(progress, completed, units.Count, unit, outcome,
                        string.Format("{0:N0} rows", loaded));
                }

                if (unit.Window.ToUtc <= DateTime.UtcNow)
                    _cache.MarkWindowComplete(unit.Entity.ObjectTypeCode, unit.Window, loaded);
                outcome.WindowsLoaded++;
            }

            outcome.Elapsed = watch.Elapsed;
            return outcome;
        }

        private static void Report(IProgress<SyncProgress> progress, int completed, int total,
            SyncUnit unit, SyncOutcome outcome, string note)
        {
            if (progress == null) return;
            progress.Report(new SyncProgress
            {
                UnitsCompleted = completed,
                UnitsTotal = total,
                RowsLoaded = outcome.RowsLoaded,
                Message = string.Format("{0}  {1:MMM yyyy}  -  {2}",
                    unit.Entity.DisplayName, unit.Window.FromUtc, note)
            });
        }
    }

    public class SyncProgress
    {
        public int UnitsCompleted { get; set; }
        public int UnitsTotal { get; set; }
        public long RowsLoaded { get; set; }
        public string Message { get; set; }

        public int Percent
        {
            get { return UnitsTotal == 0 ? 0 : (int)Math.Min(100, 100.0 * UnitsCompleted / UnitsTotal); }
        }
    }

    public class SyncOutcome
    {
        public long RowsLoaded { get; set; }
        public int WindowsLoaded { get; set; }
        public int WindowsSkipped { get; set; }
        public TimeSpan Elapsed { get; set; }

        public string Describe()
        {
            var parts = new List<string>
            {
                string.Format("{0:N0} rows in {1}", RowsLoaded, SyncEstimate.FormatDuration(Elapsed))
            };
            if (WindowsSkipped > 0)
                parts.Add(string.Format("{0} window(s) already cached and skipped", WindowsSkipped));
            return string.Join("; ", parts) + ".";
        }
    }
}
