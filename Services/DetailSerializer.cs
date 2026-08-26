using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using PanopticonAuditHistorySearch.Model;

namespace PanopticonAuditHistorySearch.Services
{
    public static class DetailSerializer
    {
        private static readonly DataContractJsonSerializer Serializer =
            new DataContractJsonSerializer(typeof(AuditDetailPayload));

        public static string Serialize(AuditDetailPayload payload)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.WriteObject(stream, payload);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public static AuditDetailPayload Deserialize(string json)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (AuditDetailPayload)Serializer.ReadObject(stream);
            }
        }
    }
}
