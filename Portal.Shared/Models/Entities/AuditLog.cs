// File: Portal.Shared/Models/Entities/AuditLog.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Shared.Models.Entities
{
    /// <summary>
    /// ตารางสำหรับเก็บประวัติการเปลี่ยนแปลงข้อมูลทั้งหมดในระบบ (ปรับปรุงใหม่)
    /// </summary>
    public class AuditLog
    {
        public int Id { get; set; }

        // --- NEW ---
        /// <summary>
        /// ID สำหรับจัดกลุ่มการเปลี่ยนแปลงทั้งหมดที่เกิดขึ้นใน Transaction เดียวกัน
        /// </summary>
        public Guid TransactionId { get; set; }
        // --- END NEW ---

        /// <summary>
        /// ID ของผู้ใช้งานที่ทำให้เกิดการเปลี่ยนแปลง (ถ้ามี)
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// ชื่อผู้ใช้งาน (Username) สำหรับการแสดงผลและค้นหาที่ง่ายขึ้น ไม่ต้อง Join
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// ประเภทของการเปลี่ยนแปลง (เช่น Added, Modified, Deleted)
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// ชื่อตาราง (Entity) ที่เกิดการเปลี่ยนแปลง
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// วันที่และเวลาที่เกิดการเปลี่ยนแปลง (UTC)
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// ค่าเก่าของข้อมูลที่เปลี่ยนแปลง (JSON)
        /// โดยจะเก็บเฉพาะ Property ที่มีการเปลี่ยนแปลง
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? OldValues { get; set; }

        /// <summary>
        /// ค่าใหม่ของข้อมูล (JSON)
        /// โดยจะเก็บเฉพาะ Property ที่มีการเปลี่ยนแปลง
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? NewValues { get; set; }

        /// <summary>
        /// รายชื่อคอลัมน์ที่ได้รับผลกระทบ (JSON Array)
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? AffectedColumns { get; set; }

        /// <summary>
        /// Primary Key ของ Record ที่ถูกเปลี่ยนแปลง (JSON)
        /// รองรับ Composite Keys
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string PrimaryKey { get; set; } = string.Empty;

        // ----- คอลัมน์ที่เพิ่มใหม่เพื่อประสิทธิภาพสูงสุด -----

        /// <summary>
        /// หมายเลข IP Address ของ Client ที่ส่ง Request มา
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// ข้อมูล User-Agent ของ Client (Browser/Application)
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// ID เฉพาะของ HTTP Request นั้นๆ เพื่อใช้ในการตรวจสอบและติดตามปัญหา
        /// </summary>
        public string? TraceId { get; set; }
    }
}
