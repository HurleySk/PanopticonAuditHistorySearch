using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PanopticonAuditHistorySearch.Model;

namespace PanopticonAuditHistorySearch.Services
{
    public class AuditAccessProbe
    {
        private readonly IOrganizationService _service;
        private readonly AuditDetailService _details;

        public AuditAccessProbe(IOrganizationService service, AuditDetailService details)
        {
            _service = service;
            _details = details;
        }

        public AccessReport Run(CancellationToken token)
        {
            var report = new AccessReport();

            Entity probe = null;
            try
            {
                var query = new QueryExpression("audit")
                {
                    ColumnSet = new ColumnSet("auditid", "attributemask", "objecttypecode", "operation"),
                    TopCount = 25,
                    NoLock = true
                };
                query.Criteria.AddCondition("operation", ConditionOperator.Equal, 2);
                query.AddOrder("createdon", OrderType.Descending);

                var result = _service.RetrieveMultiple(query);
                report.CanReadSummary = true;
                probe = result.Entities.FirstOrDefault(
                    e => !string.IsNullOrWhiteSpace(e.GetAttributeValue<string>("attributemask")));
            }
            catch (Exception ex)
            {
                report.SummaryError = ex.Message;
                return report;
            }

            if (probe == null)
            {
                report.CanReadDetails = true;
                report.MaskUsable = true;
                report.MaskNote = "No update audit row was available to validate the changed-field mapping.";
                return report;
            }

            var payload = _details.Fetch(probe.Id, token);
            if (payload.Error != null)
            {
                report.DetailsError = payload.Error;
                return report;
            }

            report.CanReadDetails = true;
            ValidateMask(report, probe, payload, token);
            return report;
        }

        private void ValidateMask(AccessReport report, Entity probe, AuditDetailPayload payload,
            CancellationToken token)
        {
            if (payload.Changes == null || payload.Changes.Count == 0)
            {
                report.MaskUsable = true;
                report.MaskNote = "The sampled audit row carried no field changes to validate against.";
                return;
            }

            var logicalName = EntityName(probe);
            if (logicalName == null)
            {
                report.MaskUsable = true;
                report.MaskNote = "Could not identify the sampled row's table to validate the mapping.";
                return;
            }

            try
            {
                var catalog = new MetadataCatalog(_service);
                var descriptor = catalog.Describe(logicalName);
                if (descriptor == null)
                {
                    report.MaskUsable = true;
                    report.MaskNote = "The sampled row's table is no longer audit-enabled; mapping not validated.";
                    return;
                }

                var columns = catalog.Columns(descriptor.ToScope());
                var fromMask = new HashSet<string>(
                    AuditCache.ParseMask(probe.GetAttributeValue<string>("attributemask"))
                        .Where(columns.ContainsKey)
                        .Select(n => columns[n].LogicalName),
                    StringComparer.OrdinalIgnoreCase);

                var fromDetail = new HashSet<string>(
                    payload.Changes.Select(c => c.LogicalName), StringComparer.OrdinalIgnoreCase);

                fromMask.IntersectWith(fromDetail);

                report.MaskUsable = fromMask.Count > 0;
                report.MaskNote = report.MaskUsable
                    ? null
                    : "The changed-field mask did not line up with the values returned for a sampled row. "
                      + "Field filtering is disabled; every other filter still works.";
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                report.MaskUsable = true;
                report.MaskNote = "Changed-field mapping could not be validated: " + ex.Message;
            }
        }

        private static string EntityName(Entity probe)
        {
            if (!probe.Contains("objecttypecode")) return null;
            var value = probe["objecttypecode"];
            var text = value as string;
            if (text != null) return text;
            var option = value as OptionSetValue;
            return option == null ? null : option.Value.ToString();
        }
    }

    public class AccessReport
    {
        public bool CanReadSummary { get; set; }
        public bool CanReadDetails { get; set; }
        public bool MaskUsable { get; set; }
        public string SummaryError { get; set; }
        public string DetailsError { get; set; }
        public string MaskNote { get; set; }

        public bool IsUsable { get { return CanReadSummary && CanReadDetails; } }

        public string BlockingMessage()
        {
            if (!CanReadSummary)
                return "This account cannot read the audit table. The View Audit Summary privilege "
                     + "(prvReadAuditSummary) is required.\r\n\r\n" + SummaryError;
            if (!CanReadDetails)
                return "This account can list audit rows but cannot read their values. The View Audit History "
                     + "privilege (prvReadRecordAuditHistory) is required.\r\n\r\n" + DetailsError;
            return null;
        }

        public IList<string> Warnings()
        {
            var warnings = new List<string>();
            if (!string.IsNullOrEmpty(MaskNote)) warnings.Add(MaskNote);
            return warnings;
        }
    }
}
