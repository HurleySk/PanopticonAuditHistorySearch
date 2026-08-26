using System;
using System.Collections.Generic;

namespace PanopticonAuditHistorySearch.Model
{
    public class AuditDetailPayload
    {
        public Guid AuditId { get; set; }
        public string Kind { get; set; }
        public IList<FieldChange> Changes { get; set; }
        public string Narrative { get; set; }
        public string Error { get; set; }

        public static AuditDetailPayload Failed(Guid auditId, string message)
        {
            return new AuditDetailPayload
            {
                AuditId = auditId,
                Kind = "Error",
                Changes = new List<FieldChange>(),
                Error = message
            };
        }
    }

    public class FieldChange
    {
        public string LogicalName { get; set; }
        public string DisplayName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public bool Truncated { get; set; }
    }
}
