using System;
using System.Collections.Generic;
using System.Linq;

namespace PanopticonAuditHistorySearch.Model
{
    public class SyncScope
    {
        public const int DefaultDays = 30;
        public const int PreflightRequiredDays = 90;
        public const int ConfirmationRequiredDays = 365;

        public IList<EntityScope> Entities { get; set; }
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }

        public static SyncScope Default()
        {
            var now = DateTime.UtcNow;
            return new SyncScope
            {
                Entities = new List<EntityScope>(),
                FromUtc = now.Date.AddDays(-DefaultDays),
                ToUtc = now
            };
        }

        public int SpanDays
        {
            get { return (int)Math.Ceiling((ToUtc - FromUtc).TotalDays); }
        }

        public bool RequiresPreflight { get { return SpanDays > PreflightRequiredDays; } }
        public bool RequiresConfirmation { get { return SpanDays > ConfirmationRequiredDays; } }

        public string Validate()
        {
            if (Entities == null || Entities.Count == 0)
                return "Select at least one table to search.";
            if (FromUtc == default(DateTime) || ToUtc == default(DateTime))
                return "A start and end date are both required.";
            if (ToUtc <= FromUtc)
                return "The end date must be after the start date.";
            if (SpanDays > 36500)
                return "The date range cannot exceed 100 years.";
            return null;
        }

        public int EffectiveSpanDays
        {
            get { return (int)Math.Ceiling((EffectiveToUtc - EffectiveFromUtc).TotalDays); }
        }

        public DateTime EffectiveFromUtc { get { return MonthStart(FromUtc); } }
        public DateTime EffectiveToUtc { get { return MonthStart(ToUtc).AddMonths(1); } }

        private static DateTime MonthStart(DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public IEnumerable<DateRange> MonthlyWindows()
        {
            var cursor = EffectiveFromUtc;
            var end = EffectiveToUtc;
            while (cursor < end)
            {
                yield return new DateRange { FromUtc = cursor, ToUtc = cursor.AddMonths(1) };
                cursor = cursor.AddMonths(1);
            }
        }

        public IEnumerable<SyncUnit> Units()
        {
            return from e in Entities
                   from w in MonthlyWindows()
                   select new SyncUnit { Entity = e, Window = w };
        }
    }

    public class EntityScope
    {
        public string LogicalName { get; set; }
        public string DisplayName { get; set; }
        public int ObjectTypeCode { get; set; }
        public override string ToString() { return DisplayName + " (" + LogicalName + ")"; }
    }

    public class DateRange
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
    }

    public class SyncUnit
    {
        public EntityScope Entity { get; set; }
        public DateRange Window { get; set; }
    }
}
