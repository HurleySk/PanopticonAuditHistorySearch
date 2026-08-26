using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace PanopticonAuditHistorySearch.Services
{
    public class NameResolver
    {
        public const string Missing = "(not found)";
        private const int ChunkSize = 400;

        private readonly IOrganizationService _service;
        private readonly ThrottleGuard _guard;

        public NameResolver(IOrganizationService service, ThrottleGuard guard)
        {
            _service = service;
            _guard = guard;
        }

        public IDictionary<Guid, string> ResolveUsers(IList<Guid> ids, CancellationToken token)
        {
            return Resolve("systemuser", "systemuserid", "fullname", ids, token);
        }

        public IDictionary<Guid, string> ResolveRecords(string entityLogicalName, string primaryIdAttribute,
            string primaryNameAttribute, IList<Guid> ids, CancellationToken token)
        {
            if (string.IsNullOrEmpty(primaryNameAttribute))
                return ids.ToDictionary(id => id, id => Missing);
            return Resolve(entityLogicalName, primaryIdAttribute, primaryNameAttribute, ids, token);
        }

        private IDictionary<Guid, string> Resolve(string entityLogicalName, string idAttribute,
            string nameAttribute, IList<Guid> ids, CancellationToken token)
        {
            var resolved = new Dictionary<Guid, string>();
            if (ids == null || ids.Count == 0) return resolved;

            var distinct = ids.Distinct().ToList();

            for (var offset = 0; offset < distinct.Count; offset += ChunkSize)
            {
                token.ThrowIfCancellationRequested();
                var chunk = distinct.Skip(offset).Take(ChunkSize).Cast<object>().ToArray();

                var query = new QueryExpression(entityLogicalName)
                {
                    ColumnSet = new ColumnSet(nameAttribute),
                    NoLock = true
                };
                query.Criteria.AddCondition(idAttribute, ConditionOperator.In, chunk);

                try
                {
                    var response = _guard.Execute(() => _service.RetrieveMultiple(query), token);
                    foreach (var record in response.Entities)
                        resolved[record.Id] = record.GetAttributeValue<string>(nameAttribute);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception) { }
            }

            foreach (var id in distinct)
                if (!resolved.ContainsKey(id)) resolved[id] = Missing;

            return resolved;
        }
    }
}
