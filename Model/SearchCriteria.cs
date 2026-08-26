using System;
using System.Collections.Generic;

namespace PanopticonAuditHistorySearch.Model
{
    public class SearchCriteria
    {
        public IList<int> ObjectTypeCodes { get; set; }
        public IList<Guid> UserIds { get; set; }
        public IList<int> Actions { get; set; }
        public IList<int> Operations { get; set; }
        public IList<FieldSelector> ChangedFields { get; set; }
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public Guid? ObjectId { get; set; }
        public string ObjectNameContains { get; set; }

        public bool IsEmpty
        {
            get
            {
                return Count(ObjectTypeCodes) == 0 && Count(UserIds) == 0 && Count(Actions) == 0
                    && Count(Operations) == 0 && Count(ChangedFields) == 0
                    && !FromUtc.HasValue && !ToUtc.HasValue && !ObjectId.HasValue
                    && string.IsNullOrWhiteSpace(ObjectNameContains);
            }
        }

        private static int Count<T>(IList<T> list) { return list == null ? 0 : list.Count; }
    }

    public class FieldSelector
    {
        public int ObjectTypeCode { get; set; }
        public int ColumnNumber { get; set; }
    }

    public class FieldFilterOption
    {
        public string LogicalName { get; set; }
        public string DisplayName { get; set; }
        public IList<FieldSelector> Selectors { get; set; }
        public override string ToString() { return DisplayName + "  (" + LogicalName + ")"; }
    }

    public class SearchPage
    {
        public IList<AuditRow> Rows { get; set; }
        public long TotalCount { get; set; }
    }
}
