using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PanopticonAuditHistorySearch.Model;

namespace PanopticonAuditHistorySearch.Services
{
    public class AuditQueryService
    {
        public const int PageSize = 5000;

        private static readonly string[] Columns =
        {
            "auditid", "createdon", "objecttypecode", "objectid", "userid",
            "callinguserid", "action", "operation", "transactionid", "attributemask"
        };

        private readonly IOrganizationService _service;
        private readonly ThrottleGuard _guard;

        public AuditQueryService(IOrganizationService service, ThrottleGuard guard)
        {
            _service = service;
            _guard = guard;
        }

        public IEnumerable<IList<AuditRow>> Pages(EntityScope entity, DateRange window, CancellationToken token)
        {
            var query = new QueryExpression("audit")
            {
                ColumnSet = new ColumnSet(Columns),
                NoLock = true,
                PageInfo = new PagingInfo { Count = PageSize, PageNumber = 1, PagingCookie = null }
            };
            query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, entity.LogicalName);
            query.Criteria.AddCondition("createdon", ConditionOperator.OnOrAfter, window.FromUtc);
            query.Criteria.AddCondition("createdon", ConditionOperator.OnOrBefore, window.ToUtc);
            query.AddOrder("createdon", OrderType.Descending);

            while (true)
            {
                token.ThrowIfCancellationRequested();
                var response = _guard.Execute(() => _service.RetrieveMultiple(query), token);

                var rows = new List<AuditRow>(response.Entities.Count);
                foreach (var record in response.Entities)
                    rows.Add(Map(record, entity.ObjectTypeCode));

                yield return rows;

                if (!response.MoreRecords) yield break;
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = response.PagingCookie;
            }
        }

        public static AuditRow Map(Entity record, int fallbackObjectTypeCode)
        {
            return new AuditRow
            {
                AuditId = record.Id,
                CreatedOn = record.GetAttributeValue<DateTime>("createdon"),
                ObjectTypeCode = fallbackObjectTypeCode,
                ObjectId = Lookup(record, "objectid"),
                UserId = Lookup(record, "userid"),
                CallingUserId = Lookup(record, "callinguserid"),
                Action = Option(record, "action"),
                Operation = Option(record, "operation"),
                TransactionId = record.GetAttributeValue<Guid>("transactionid"),
                AttributeMask = record.GetAttributeValue<string>("attributemask")
            };
        }

        private static Guid Lookup(Entity record, string name)
        {
            var reference = record.GetAttributeValue<EntityReference>(name);
            return reference == null ? Guid.Empty : reference.Id;
        }

        private static int Option(Entity record, string name)
        {
            var option = record.GetAttributeValue<OptionSetValue>(name);
            return option == null ? 0 : option.Value;
        }
    }
}
