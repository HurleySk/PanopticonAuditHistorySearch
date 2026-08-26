using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using PanopticonAuditHistorySearch.Model;

namespace PanopticonAuditHistorySearch.Services
{
    public class AuditDetailService
    {
        public const int BatchSize = 100;
        public const int TimelinePageSize = 50;

        private readonly IOrganizationService _service;
        private readonly ThrottleGuard _guard;

        public AuditDetailService(IOrganizationService service, ThrottleGuard guard)
        {
            _service = service;
            _guard = guard;
        }

        public AuditDetailPayload Fetch(Guid auditId, CancellationToken token)
        {
            try
            {
                var response = _guard.Execute(
                    () => (RetrieveAuditDetailsResponse)_service.Execute(
                        new RetrieveAuditDetailsRequest { AuditId = auditId }),
                    token);
                return Convert(auditId, response.AuditDetail);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                return AuditDetailPayload.Failed(auditId, ex.Message);
            }
        }

        public IList<AuditDetailPayload> FetchBatch(IList<Guid> auditIds, CancellationToken token)
        {
            var results = new List<AuditDetailPayload>(auditIds.Count);

            for (var offset = 0; offset < auditIds.Count; offset += BatchSize)
            {
                token.ThrowIfCancellationRequested();
                var slice = auditIds.Skip(offset).Take(BatchSize).ToList();

                var batch = new ExecuteMultipleRequest
                {
                    Settings = new ExecuteMultipleSettings { ContinueOnError = true, ReturnResponses = true },
                    Requests = new OrganizationRequestCollection()
                };
                foreach (var id in slice)
                    batch.Requests.Add(new RetrieveAuditDetailsRequest { AuditId = id });

                var response = _guard.Execute(
                    () => (ExecuteMultipleResponse)_service.Execute(batch), token);

                foreach (var item in response.Responses)
                {
                    var auditId = slice[item.RequestIndex];
                    if (item.Fault != null)
                    {
                        results.Add(AuditDetailPayload.Failed(auditId, item.Fault.Message));
                        continue;
                    }

                    var detail = ((RetrieveAuditDetailsResponse)item.Response).AuditDetail;
                    results.Add(Convert(auditId, detail));
                }
            }

            return results;
        }

        public IList<TimelineEntry> Timeline(string entityLogicalName, Guid recordId, int maxPages,
            CancellationToken token)
        {
            var entries = new List<TimelineEntry>();
            var paging = new PagingInfo { Count = TimelinePageSize, PageNumber = 1, ReturnTotalRecordCount = true };

            for (var page = 0; page < maxPages; page++)
            {
                token.ThrowIfCancellationRequested();

                var request = new RetrieveRecordChangeHistoryRequest
                {
                    Target = new EntityReference(entityLogicalName, recordId),
                    PagingInfo = paging
                };

                var response = _guard.Execute(
                    () => (RetrieveRecordChangeHistoryResponse)_service.Execute(request), token);

                var collection = response.AuditDetailCollection;
                foreach (var detail in collection.AuditDetails)
                {
                    var record = detail.AuditRecord;
                    var auditId = record == null ? Guid.Empty : record.Id;
                    entries.Add(new TimelineEntry
                    {
                        AuditId = auditId,
                        CreatedOn = record == null ? DateTime.MinValue : record.GetAttributeValue<DateTime>("createdon"),
                        UserName = record == null ? null : ReferenceName(record, "userid"),
                        ActionLabel = record == null ? null : AuditLabels.Action(OptionValue(record, "action")),
                        Detail = Convert(auditId, detail)
                    });
                }

                if (!collection.MoreRecords) break;
                paging.PageNumber++;
                paging.PagingCookie = collection.PagingCookie;
            }

            return entries;
        }

        public static AuditDetailPayload Convert(Guid auditId, AuditDetail detail)
        {
            var payload = new AuditDetailPayload
            {
                AuditId = auditId,
                Changes = new List<FieldChange>(),
                Kind = detail == null ? "None" : detail.GetType().Name
            };

            var attributeDetail = detail as AttributeAuditDetail;
            if (attributeDetail != null)
            {
                payload.Changes = FieldChanges(attributeDetail);
                return payload;
            }

            var shareDetail = detail as ShareAuditDetail;
            if (shareDetail != null)
            {
                payload.Narrative = string.Format("{0}: {1} -> {2}",
                    shareDetail.Principal == null ? "(unknown principal)" : shareDetail.Principal.Name,
                    shareDetail.OldPrivileges, shareDetail.NewPrivileges);
                return payload;
            }

            var relationshipDetail = detail as RelationshipAuditDetail;
            if (relationshipDetail != null)
            {
                var targets = relationshipDetail.TargetRecords == null
                    ? new string[0]
                    : relationshipDetail.TargetRecords.Select(r => r.Name ?? r.Id.ToString()).ToArray();
                payload.Narrative = string.Format("{0}: {1}",
                    relationshipDetail.RelationshipName, string.Join(", ", targets));
                return payload;
            }

            var roleDetail = detail as RolePrivilegeAuditDetail;
            if (roleDetail != null)
            {
                payload.Narrative = string.Format("{0} old privilege(s), {1} new privilege(s)",
                    Length(roleDetail.OldRolePrivileges), Length(roleDetail.NewRolePrivileges));
                return payload;
            }

            var accessDetail = detail as UserAccessAuditDetail;
            if (accessDetail != null)
            {
                payload.Narrative = string.Format("Access at {0}, interval {1}",
                    accessDetail.AccessTime, accessDetail.Interval);
                return payload;
            }

            payload.Narrative = "No value detail is recorded for this event type.";
            return payload;
        }

        private static IList<FieldChange> FieldChanges(AttributeAuditDetail detail)
        {
            var changes = new List<FieldChange>();
            var oldRecord = detail.OldValue;
            var newRecord = detail.NewValue;
            var seen = new HashSet<string>();

            if (oldRecord != null)
            {
                foreach (var key in oldRecord.Attributes.Keys)
                {
                    seen.Add(key);
                    changes.Add(new FieldChange
                    {
                        LogicalName = Normalize(key),
                        OldValue = Display(oldRecord, key),
                        NewValue = newRecord != null && newRecord.Contains(key) ? Display(newRecord, key) : null
                    });
                }
            }

            if (newRecord != null)
            {
                foreach (var key in newRecord.Attributes.Keys)
                {
                    if (seen.Contains(key)) continue;
                    changes.Add(new FieldChange
                    {
                        LogicalName = Normalize(key),
                        OldValue = null,
                        NewValue = Display(newRecord, key)
                    });
                }
            }

            foreach (var change in changes)
                change.Truncated = IsTruncated(change.OldValue) || IsTruncated(change.NewValue);

            return changes.OrderBy(c => c.LogicalName, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static bool IsTruncated(string value)
        {
            return value != null && value.Length >= 4990 && value.EndsWith("…");
        }

        private static string Normalize(string key)
        {
            if (key.StartsWith("_") && key.EndsWith("_value"))
                return key.Substring(1, key.Length - 7);
            return key;
        }

        private static string Display(Entity record, string key)
        {
            if (record.FormattedValues.Contains(key)) return record.FormattedValues[key];

            var value = record[key];
            if (value == null) return null;

            var reference = value as EntityReference;
            if (reference != null) return reference.Name ?? reference.Id.ToString();

            var option = value as OptionSetValue;
            if (option != null) return option.Value.ToString();

            var money = value as Money;
            if (money != null) return money.Value.ToString("N2");

            return value.ToString();
        }

        private static string ReferenceName(Entity record, string key)
        {
            var reference = record.GetAttributeValue<EntityReference>(key);
            return reference == null ? null : (reference.Name ?? reference.Id.ToString());
        }

        private static int OptionValue(Entity record, string key)
        {
            var option = record.GetAttributeValue<OptionSetValue>(key);
            return option == null ? 0 : option.Value;
        }

        private static int Length<T>(T[] values) { return values == null ? 0 : values.Length; }
    }

    public class TimelineEntry
    {
        public Guid AuditId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UserName { get; set; }
        public string ActionLabel { get; set; }
        public AuditDetailPayload Detail { get; set; }
    }
}
