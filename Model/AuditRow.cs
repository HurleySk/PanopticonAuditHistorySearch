using System;
using System.Collections.Generic;

namespace PanopticonAuditHistorySearch.Model
{
    public class AuditRow
    {
        public Guid AuditId { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ObjectTypeCode { get; set; }
        public string EntityLogicalName { get; set; }
        public string EntityDisplayName { get; set; }
        public Guid ObjectId { get; set; }
        public string ObjectName { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public Guid CallingUserId { get; set; }
        public int Action { get; set; }
        public int Operation { get; set; }
        public Guid TransactionId { get; set; }
        public string AttributeMask { get; set; }
        public IList<string> ChangedFields { get; set; }

        public string ActionLabel { get { return AuditLabels.Action(Action); } }
        public string OperationLabel { get { return AuditLabels.Operation(Operation); } }
        public string ChangedFieldsLabel
        {
            get { return ChangedFields == null ? string.Empty : string.Join(", ", ChangedFields); }
        }
    }
}
