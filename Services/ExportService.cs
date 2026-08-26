using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PanopticonAuditHistorySearch.Model;

namespace PanopticonAuditHistorySearch.Services
{
    public static class ExportService
    {
        private static readonly string[] BaseHeaders =
        {
            "Changed Date (UTC)", "Table", "Record", "Record Id", "Event", "Operation",
            "Changed By", "Changed Fields", "Transaction Id", "Audit Id"
        };

        private static readonly string[] DetailHeaders = { "Field", "Old Value", "New Value" };

        public static int Write(string path, IList<AuditRow> rows,
            IDictionary<Guid, AuditDetailPayload> details)
        {
            var includeDetails = details != null;
            var written = 0;

            using (var writer = new StreamWriter(path, false, new UTF8Encoding(true)))
            {
                var headers = includeDetails ? BaseHeaders.Concat(DetailHeaders) : BaseHeaders;
                writer.WriteLine(string.Join(",", headers.Select(Escape)));

                foreach (var row in rows)
                {
                    var cells = BaseCells(row);

                    if (!includeDetails)
                    {
                        writer.WriteLine(string.Join(",", cells.Select(Escape)));
                        written++;
                        continue;
                    }

                    AuditDetailPayload payload;
                    details.TryGetValue(row.AuditId, out payload);

                    var changes = payload == null || payload.Changes == null || payload.Changes.Count == 0
                        ? null
                        : payload.Changes;

                    if (changes == null)
                    {
                        var narrative = payload == null ? string.Empty : (payload.Narrative ?? payload.Error);
                        writer.WriteLine(string.Join(",",
                            cells.Concat(new[] { string.Empty, string.Empty, narrative ?? string.Empty })
                                 .Select(Escape)));
                        written++;
                        continue;
                    }

                    foreach (var change in changes)
                    {
                        writer.WriteLine(string.Join(",",
                            cells.Concat(new[]
                            {
                                change.DisplayName ?? change.LogicalName,
                                change.OldValue ?? string.Empty,
                                change.NewValue ?? string.Empty
                            }).Select(Escape)));
                        written++;
                    }
                }
            }

            return written;
        }

        private static string[] BaseCells(AuditRow row)
        {
            return new[]
            {
                row.CreatedOn.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff"),
                row.EntityDisplayName ?? row.EntityLogicalName,
                row.ObjectName,
                row.ObjectId == Guid.Empty ? string.Empty : row.ObjectId.ToString(),
                row.ActionLabel,
                row.OperationLabel,
                row.UserName,
                row.ChangedFieldsLabel,
                row.TransactionId == Guid.Empty ? string.Empty : row.TransactionId.ToString(),
                row.AuditId.ToString()
            };
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            var needsQuotes = value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0
                              || value[0] == ' ' || value[value.Length - 1] == ' '
                              || value[0] == '=' || value[0] == '+' || value[0] == '-' || value[0] == '@';

            if (!needsQuotes) return value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
