using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using PanopticonAuditHistorySearch.Model;

namespace PanopticonAuditHistorySearch.Services
{
    public class MetadataCatalog
    {
        private readonly IOrganizationService _service;
        private readonly Dictionary<int, Dictionary<int, ColumnInfo>> _columnsByOtc =
            new Dictionary<int, Dictionary<int, ColumnInfo>>();
        private readonly object _gate = new object();
        private List<EntityDescriptor> _entities;

        public MetadataCatalog(IOrganizationService service)
        {
            _service = service;
        }

        public IList<EntityDescriptor> AuditedEntities()
        {
            lock (_gate)
            {
                if (_entities != null) return _entities;

                var request = new RetrieveMetadataChangesRequest
                {
                    Query = new EntityQueryExpression
                    {
                        Properties = new MetadataPropertiesExpression(
                            "LogicalName", "DisplayName", "ObjectTypeCode",
                            "IsAuditEnabled", "PrimaryNameAttribute", "IsIntersect")
                    }
                };

                var response = (RetrieveMetadataChangesResponse)_service.Execute(request);

                _entities = response.EntityMetadata
                    .Where(e => e.ObjectTypeCode.HasValue
                                && e.IsAuditEnabled != null && e.IsAuditEnabled.Value
                                && !(e.IsIntersect ?? false))
                    .Select(e => new EntityDescriptor
                    {
                        LogicalName = e.LogicalName,
                        DisplayName = Label(e.DisplayName, e.LogicalName),
                        ObjectTypeCode = e.ObjectTypeCode.Value,
                        PrimaryNameAttribute = e.PrimaryNameAttribute
                    })
                    .OrderBy(e => e.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();

                return _entities;
            }
        }

        public EntityDescriptor Describe(int objectTypeCode)
        {
            return AuditedEntities().FirstOrDefault(e => e.ObjectTypeCode == objectTypeCode);
        }

        public EntityDescriptor Describe(string logicalName)
        {
            return AuditedEntities().FirstOrDefault(e =>
                string.Equals(e.LogicalName, logicalName, StringComparison.OrdinalIgnoreCase));
        }

        public Dictionary<int, ColumnInfo> Columns(EntityScope entity)
        {
            lock (_gate)
            {
                Dictionary<int, ColumnInfo> cached;
                if (_columnsByOtc.TryGetValue(entity.ObjectTypeCode, out cached)) return cached;

                var request = new RetrieveEntityRequest
                {
                    LogicalName = entity.LogicalName,
                    EntityFilters = EntityFilters.Attributes,
                    RetrieveAsIfPublished = true
                };

                var response = (RetrieveEntityResponse)_service.Execute(request);

                var map = new Dictionary<int, ColumnInfo>();
                foreach (var attribute in response.EntityMetadata.Attributes)
                {
                    if (!attribute.ColumnNumber.HasValue) continue;
                    map[attribute.ColumnNumber.Value] = new ColumnInfo
                    {
                        ColumnNumber = attribute.ColumnNumber.Value,
                        LogicalName = attribute.LogicalName,
                        DisplayName = Label(attribute.DisplayName, attribute.LogicalName),
                        IsAuditEnabled = attribute.IsAuditEnabled != null && attribute.IsAuditEnabled.Value
                    };
                }

                _columnsByOtc[entity.ObjectTypeCode] = map;
                return map;
            }
        }

        public IList<ColumnInfo> AuditedColumns(EntityScope entity)
        {
            lock (_gate)
            {
                return Columns(entity).Values
                    .Where(c => c.IsAuditEnabled)
                    .OrderBy(c => c.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();
            }
        }

        private static string Label(Microsoft.Xrm.Sdk.Label label, string fallback)
        {
            if (label != null && label.UserLocalizedLabel != null &&
                !string.IsNullOrWhiteSpace(label.UserLocalizedLabel.Label))
                return label.UserLocalizedLabel.Label;
            return fallback;
        }
    }

    public class EntityDescriptor
    {
        public string LogicalName { get; set; }
        public string DisplayName { get; set; }
        public int ObjectTypeCode { get; set; }
        public string PrimaryNameAttribute { get; set; }

        public EntityScope ToScope()
        {
            return new EntityScope
            {
                LogicalName = LogicalName,
                DisplayName = DisplayName,
                ObjectTypeCode = ObjectTypeCode
            };
        }

        public override string ToString() { return DisplayName + "  (" + LogicalName + ")"; }
    }

    public class ColumnInfo
    {
        public int ColumnNumber { get; set; }
        public string LogicalName { get; set; }
        public string DisplayName { get; set; }
        public bool IsAuditEnabled { get; set; }
        public override string ToString() { return DisplayName + "  (" + LogicalName + ")"; }
    }
}
