using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PanopticonAuditHistorySearch.Model;

namespace PanopticonAuditHistorySearch.Services
{
    public class EstimateService
    {
        private const int AggregateLimitErrorCode = -2147164125;
        private const int SampleDays = 3;

        private readonly IOrganizationService _service;
        private readonly ThrottleGuard _guard;

        public EstimateService(IOrganizationService service, ThrottleGuard guard)
        {
            _service = service;
            _guard = guard;
        }

        public SyncEstimate Estimate(SyncScope scope, CancellationToken token)
        {
            var estimate = new SyncEstimate { Entities = new List<EntityEstimate>() };

            foreach (var entity in scope.Entities)
            {
                token.ThrowIfCancellationRequested();
                estimate.Entities.Add(EstimateEntity(entity, scope, token));
            }

            foreach (var entry in estimate.Entities)
                if (entry.Sampled) estimate.AnySampled = true;

            return estimate;
        }

        private EntityEstimate EstimateEntity(EntityScope entity, SyncScope scope, CancellationToken token)
        {
            try
            {
                var exact = Count(entity, scope.EffectiveFromUtc, scope.EffectiveToUtc, token);
                return new EntityEstimate { Entity = entity, Rows = exact, Sampled = false };
            }
            catch (FaultException<OrganizationServiceFault> fault)
                when (fault.Detail != null && fault.Detail.ErrorCode == AggregateLimitErrorCode)
            {
                return Sample(entity, scope, token);
            }
        }

        private EntityEstimate Sample(EntityScope entity, SyncScope scope, CancellationToken token)
        {
            var sampleTo = scope.EffectiveToUtc;
            var sampleFrom = sampleTo.AddDays(-SampleDays);
            if (sampleFrom < scope.EffectiveFromUtc) sampleFrom = scope.EffectiveFromUtc;

            var sampleDays = Math.Max(0.5, (sampleTo - sampleFrom).TotalDays);

            try
            {
                var sampled = Count(entity, sampleFrom, sampleTo, token);
                var perDay = sampled / sampleDays;
                return new EntityEstimate
                {
                    Entity = entity,
                    Rows = (long)Math.Round(perDay * scope.EffectiveSpanDays),
                    Sampled = true,
                    Note = string.Format("extrapolated from {0:N0} rows over the last {1:N0} day(s)",
                        sampled, sampleDays)
                };
            }
            catch (FaultException<OrganizationServiceFault>)
            {
                return new EntityEstimate
                {
                    Entity = entity,
                    Rows = 0,
                    Sampled = true,
                    Note = "too large to count; even a " + SampleDays + "-day sample exceeds the aggregate limit"
                };
            }
        }

        private long Count(EntityScope entity, DateTime fromUtc, DateTime toUtc, CancellationToken token)
        {
            var columnSet = new ColumnSet();
            columnSet.AttributeExpressions.Add(
                new XrmAttributeExpression("auditid", XrmAggregateType.Count, "total"));

            var query = new QueryExpression("audit") { ColumnSet = columnSet, NoLock = true };
            query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, entity.LogicalName);
            query.Criteria.AddCondition("createdon", ConditionOperator.OnOrAfter, fromUtc);
            query.Criteria.AddCondition("createdon", ConditionOperator.OnOrBefore, toUtc);

            var result = _guard.Execute(() => _service.RetrieveMultiple(query), token);
            if (result.Entities.Count == 0) return 0;

            var aliased = result.Entities[0]["total"] as AliasedValue;
            return aliased == null ? 0 : Convert.ToInt64(aliased.Value);
        }
    }
}
