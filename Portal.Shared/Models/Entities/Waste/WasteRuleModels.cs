// File: Portal.Shared/Models/Entities/Waste/WasteRuleModels.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal.Shared.Models.Entities.Waste
{
    /// <summary>
    /// ประเภทข้อมูลของ Attribute เพื่อกำหนดรูปแบบการตีความ/การเปรียบเทียบ
    /// </summary>
    public enum AttributeDataType
    {
        Numeric = 1,     // ตัวเลข เช่น 0..500, > 60
        Boolean = 2,     // True/False เช่น ไม่เกิดฟอง = False
        Text = 3,        // ข้อความ/Enum เช่น "ของเหลว", "รถแทงค์"
        DateTime = 4     // เผื่ออนาคต
    }

    /// <summary>
    /// ตัวดำเนินการเปรียบเทียบที่รองรับ
    /// หมายเหตุ: BETWEEN ใช้ Value1=Min, Value2=Max ; IN ใช้ ValueList
    /// </summary>
    public enum ComparisonOperator
    {
        Equal = 1,          // =
        NotEqual = 2,       // !=
        GreaterThan = 3,    // >
        GreaterOrEqual = 4, // >=
        LessThan = 5,       // <
        LessOrEqual = 6,    // <=
        Between = 7,        // ระหว่างช่วง (รวมปลายช่วง)
        In = 8,             // อยู่ในเซ็ตค่าที่กำหนด (Text/Enum)
        NotIn = 9           // ไม่อยู่ในเซ็ตค่าที่กำหนด
    }

    /// <summary>
    /// นิยาม "ชนิดคุณสมบัติของ Waste" แบบขยายได้ (Dynamic Attribute)
    /// เพิ่มแถวใหม่ได้โดยไม่ต้องเปลี่ยน Schema อื่น
    /// </summary>
    [Table("WasteAttributes")]
    public class WasteAttribute
    {
        [Key]
        public int Id { get; set; }

        /// <summary>ชื่อคุณสมบัติ เช่น "สถานะทางกายภาพ", "ความหนืด", "pH", "Cl (%)" เป็นต้น</summary>
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>รหัสสั้น/Key ภายในระบบ (ไม่บังคับ) ใช้ Map กับฟอร์ม/Excel/ระบบภายนอกได้</summary>
        [MaxLength(100)]
        public string? Code { get; set; }

        /// <summary>หน่วย เช่น "mPa·s", "%", "°C", "kcal" (ปล่อยว่างได้ถ้า Boolean/ไม่มีหน่วย)</summary>
        [MaxLength(50)]
        public string? Unit { get; set; }

        /// <summary>ประเภทข้อมูลของ Attribute</summary>
        [Required]
        public AttributeDataType DataType { get; set; }

        /// <summary>
        /// ค่า Enum/ชุดค่าที่อนุญาต (ถ้ามี) เก็บเป็น CSV เช่น "ของเหลว,ของแข็ง,กึ่งของแข็ง" หรือ "รถแทงค์,ถัง200ลิตร,IBC"
        /// </summary>
        [MaxLength(1000)]
        public string? AllowedValuesCsv { get; set; }

        /// <summary>สถานะการใช้งาน</summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// สูตร/เกณฑ์การพิจารณาวิธีดำเนินการกับ Waste (เช่น "เกณฑ์การพิจารณาเข้า Dilute (กรด)")
    /// </summary>
    [Table("WasteFormulas")]
    public class WasteFormula
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(300)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>เฉพาะผู้ใช้ที่ IsSystemRole = true เท่านั้นจึงจะสร้าง/แก้ไขได้ (ตรวจสอบที่ชั้น Controller/Service)</summary>
        public Guid? CreatedByEmployeeId { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        /// <summary>เงื่อนไขย่อยทั้งหมดของสูตร (AND ข้าม Attribute, แต่ OR ภายใน Attribute เดียวกัน)</summary>
        public List<WasteFormulaCondition> Conditions { get; set; } = [];
    }

    /// <summary>
    /// เงื่อนไขย่อยของสูตรหนึ่งข้อ ผูกกับ WasteAttribute หนึ่งตัว
    /// - OR ภายใน Attribute เดียวกัน: สร้างหลายแถวที่อ้าง AttributeId เดียวกัน
    /// - AND ระหว่าง Attribute ที่ต่างกัน: ต้องผ่านครบทั้งหมด
    /// </summary>
    [Table("WasteFormulaConditions")]
    public class WasteFormulaCondition
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Formula))]
        public int FormulaId { get; set; }

        public WasteFormula Formula { get; set; } = null!;

        [ForeignKey(nameof(Attribute))]
        public int AttributeId { get; set; }

        public WasteAttribute Attribute { get; set; } = null!;

        [Required]
        public ComparisonOperator Operator { get; set; }

        /// <summary>
        /// ค่าเปรียบเทียบที่ 1 (Numeric/Text/Boolean) ใช้กับ =, !=, >, >=, <, <= หรือ Min ของ BETWEEN
        /// - Numeric/Boolean เก็บเป็น string แล้วให้ Service แปลงก่อนเทียบเพื่อความยืดหยุ่น
        /// </summary>
        [MaxLength(200)]
        public string? Value1 { get; set; }

        /// <summary>
        /// ค่าเปรียบเทียบที่ 2 (ใช้กับ BETWEEN เป็น Max)
        /// </summary>
        [MaxLength(200)]
        public string? Value2 { get; set; }

        /// <summary>
        /// รายการค่า (CSV) ใช้กับ IN/NOT IN เช่น "รถแทงค์,ถัง200ลิตร,IBC" หรือ "ของเหลว,ของแข็ง"
        /// หมายเหตุ: ถ้าต้องการ OR หลายช่วงของ Numeric (เช่น pH 1–4 หรือ 12–14)
        /// ให้สร้างหลายแถว (BETWEEN 1..4) และ (BETWEEN 12..14) ที่ AttributeId เดียวกัน
        /// </summary>
        [MaxLength(2000)]
        public string? ValueListCsv { get; set; }

        /// <summary>
        /// ข้อความอธิบายเพิ่มเติมของเงื่อนไขนี้ (สำหรับแสดงผล Report หรือ Tooltips)
        /// </summary>
        [MaxLength(500)]
        public string? Note { get; set; }

        /// <summary>
        /// ลำดับการแสดงผลของเงื่อนไขในหน้า UI (ภายในสูตรเดียวกัน)
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// เปิด/ปิดเงื่อนไขแถวนี้ (บางครั้งอยากพักใช้ชั่วคราว)
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
