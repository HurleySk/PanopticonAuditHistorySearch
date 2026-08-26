using System;
using System.Collections.Generic;

namespace PanopticonAuditHistorySearch.Model
{
    public class SyncEstimate
    {
        public const int BytesPerRow = 400;

        public IList<EntityEstimate> Entities { get; set; }
        public bool AnySampled { get; set; }

        public long TotalRows
        {
            get
            {
                long total = 0;
                if (Entities != null)
                    foreach (var e in Entities) total += e.Rows;
                return total;
            }
        }

        public long EstimatedBytes { get { return TotalRows * BytesPerRow; } }

        public TimeSpan EstimatedDuration
        {
            get
            {
                var pages = Math.Ceiling(TotalRows / 5000.0);
                return TimeSpan.FromSeconds(pages * 2.5);
            }
        }

        public string Summary()
        {
            var qualifier = AnySampled ? "approx. " : "";
            return string.Format("{0}{1:N0} audit rows, {2}, about {3}",
                qualifier, TotalRows, FormatBytes(EstimatedBytes), FormatDuration(EstimatedDuration));
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes >= 1073741824L) return (bytes / 1073741824.0).ToString("N2") + " GB";
            if (bytes >= 1048576L) return (bytes / 1048576.0).ToString("N1") + " MB";
            if (bytes >= 1024L) return (bytes / 1024.0).ToString("N0") + " KB";
            return bytes + " B";
        }

        public static string FormatDuration(TimeSpan span)
        {
            if (span.TotalHours >= 1) return string.Format("{0:N1} hours", span.TotalHours);
            if (span.TotalMinutes >= 1) return string.Format("{0:N0} min", span.TotalMinutes);
            return string.Format("{0:N0} sec", span.TotalSeconds);
        }
    }

    public class EntityEstimate
    {
        public EntityScope Entity { get; set; }
        public long Rows { get; set; }
        public bool Sampled { get; set; }
        public string Note { get; set; }
    }
}
