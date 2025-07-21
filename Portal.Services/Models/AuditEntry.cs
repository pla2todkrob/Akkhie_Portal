// File: Portal.Services/Models/AuditEntry.cs
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Portal.Shared.Enums;
using Portal.Shared.Models.Entities;

namespace Portal.Services.Models
{
    /// <summary>
    /// คลาสตัวกลางสำหรับเก็บข้อมูลการเปลี่ยนแปลงของ Entity หนึ่งๆ ก่อนแปลงเป็น AuditLog Entity (ปรับปรุงใหม่)
    /// </summary>
    public class AuditEntry(EntityEntry entry, string tableName)
    {
        public EntityEntry Entry { get; } = entry;
        public List<PropertyEntry> TemporaryProperties { get; } = [];
        public Guid TransactionId { get; set; }

        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string TableName { get; set; } = tableName;
        public Dictionary<string, object> KeyValues { get; } = [];
        public Dictionary<string, object> OldValues { get; } = [];
        public Dictionary<string, object> NewValues { get; } = [];
        public AuditType AuditType { get; set; }
        public List<string> ChangedColumns { get; } = [];

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? TraceId { get; set; }

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        /// <summary>
        /// แปลงจาก AuditEntry ไปเป็น AuditLog Entity เพื่อพร้อมบันทึกลง DB
        /// </summary>
        public AuditLog ToAudit()
        {
            var audit = new AuditLog
            {
                TransactionId = TransactionId,
                UserId = UserId,
                Username = Username,
                Type = AuditType.ToString(),
                TableName = TableName,
                DateTime = DateTime.UtcNow,
                PrimaryKey = JsonConvert.SerializeObject(KeyValues),
                OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues),
                NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues),
                AffectedColumns = ChangedColumns.Count == 0 ? null : JsonConvert.SerializeObject(ChangedColumns),
                IpAddress = IpAddress,
                UserAgent = UserAgent,
                TraceId = TraceId
            };
            return audit;
        }
    }
}
