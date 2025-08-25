using System.ComponentModel.DataAnnotations;

namespace Portal.Shared.Models.DTOs.WasteRules
{
    // --- ชนิดข้อมูลของค่า Attribute ---
    public enum WasteDataType
    {
        Numeric = 1,   // ตัวเลข เช่น pH, TS, Cl, S, ความหนืด ฯลฯ
        Boolean = 2,   // ใช่/ไม่ใช่ เช่น "เกิดฟอง"
        Text = 3,      // ข้อความสั้น เช่น สถานะ (ของแข็ง/ของเหลว) เมื่อ mapping เป็นตัวเลือก
        Option = 4,    // ชนิดตัวเลือก (มีชุดค่าให้เลือก เช่น "รถแทงค์", "ถัง200ลิตร", "IBC")
        DateTime = 5   // เผื่ออนาคต
    }

    // --- Operator ที่ใช้สร้างเงื่อนไข ---
    public enum WasteComparisonOperator
    {
        Equal = 1,                 // =
        NotEqual = 2,              // !=
        GreaterThan = 3,           // >
        GreaterThanOrEqual = 4,    // >=
        LessThan = 5,              // <
        LessThanOrEqual = 6,       // <=
        Between = 7,               // อยู่ในช่วง (รองรับหลายช่วง OR)
        In = 8,                    // อยู่ในชุดค่า
        NotIn = 9                  // ไม่อยู่ในชุดค่า
    }

    // --- หน่วยของ Attribute (ยืดหยุ่น) ---
    public class WasteUnitDto
    {
        [Required]
        public string Key { get; set; } = string.Empty;    // เช่น "mPa-s", "%", "kcal", "C"
        public string DisplayName { get; set; } = string.Empty; // ชื่อโชว์
    }

    // --- ค่าแบบตัวเลือก (สำหรับ Option/Text ที่ควบคุมค่า) ---
    public class WasteOptionDto
    {
        [Required]
        public string Value { get; set; } = string.Empty;      // ค่าเก็บจริง เช่น "รถแทงค์"
        public string DisplayName { get; set; } = string.Empty; // ชื่อโชว์
    }

    // --- คำอธิบาย Attribute (สคีมา) ---
    public class WasteAttributeDefinitionDto
    {
        [Required]
        public string Key { get; set; } = string.Empty;    // คีย์ภายใน เช่น "state", "viscosity", "ph", "cl", "s", "hv", "ts", "flashPoint", "foaming", "storageType"

        [Required]
        public string Name { get; set; } = string.Empty;   // ชื่อแสดง เช่น "สถานะ", "ความหนืด", "pH", "Cl (%)", "กำมะถัน (%)", "Hv", "TS (%)", "Flash Point (°C)", "เกิดฟอง", "ประเภทจัดเก็บ"

        [Required]
        public WasteDataType DataType { get; set; }

        // หน่วยที่รองรับ (เช่น ความหนืด mPa-s / pH ไม่มีหน่วย)
        public List<WasteUnitDto> Units { get; set; } = [];

        // Operator ที่ Attribute นี้รองรับ
        public List<WasteComparisonOperator> AllowedOperators { get; set; } = [];

        // ค่าตัวเลือกที่อนุญาต (กรณี DataType == Option หรือ Text ที่จำกัดค่า)
        public List<WasteOptionDto> Options { get; set; } = [];
    }

    // --- ใช้กำหนดช่วงตัวเลข (รองรับหลายช่วง OR) ---
    public class NumericRangeDto
    {
        public decimal? Min { get; set; }            // null = ไม่กำหนดด้านล่าง
        public bool InclusiveMin { get; set; } = true;
        public decimal? Max { get; set; }            // null = ไม่กำหนดด้านบน
        public bool InclusiveMax { get; set; } = true;

        public override string ToString()
        {
            var left = InclusiveMin ? "[" : "(";
            var right = InclusiveMax ? "]" : ")";
            var minText = Min?.ToString() ?? "-∞";
            var maxText = Max?.ToString() ?? "+∞";
            return $"{left}{minText}, {maxText}{right}";
        }
    }

    // --- เงื่อนไขในสูตร: 1 Attribute ต่อ 1 เงื่อนไข ---
    public class WasteConditionDto
    {
        [Required]
        public string AttributeKey { get; set; } = string.Empty;

        [Required]
        public WasteComparisonOperator Operator { get; set; }

        // หน่วยที่ใช้เทียบ (ถ้ามี)
        public string? UnitKey { get; set; }

        // สำหรับตัวเลข:
        // - ใช้ Value/Min/Max เมื่อเป็น >, >=, <, <=, = (Equal ใช้ Value)
        // - ใช้ Ranges เมื่อเป็น Between (รองรับหลายช่วง OR)
        public decimal? Value { get; set; }     // ใช้กับ Equal ฯลฯ
        public decimal? Min { get; set; }       // เผื่อกรณีอยากกำหนดช่วงแบบเดี่ยว
        public decimal? Max { get; set; }
        public List<NumericRangeDto> Ranges { get; set; } = []; // สำหรับ Between แบบหลายช่วง (เช่น pH 1-4 หรือ 12-14)

        // สำหรับ Boolean
        public bool? BoolValue { get; set; }

        // สำหรับ Option/Text
        public string? TextValue { get; set; }
        public List<string> InValues { get; set; } = []; // สำหรับ In/NotIn

        // ข้อความอธิบายเพิ่มเติม (สำหรับแสดงผล)
        public string? Note { get; set; }
    }

    // --- สูตร (Formula) ---
    public class WasteFormulaDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // เงื่อนไขทั้งหมดตีความเป็น AND กัน
        // (หากต้องการ OR แบบซับซ้อน สามารถแตกเป็นหลายสูตร)
        public List<WasteConditionDto> Conditions { get; set; } = [];
    }

    // --- Schema Response ---
    public class WasteSchemaResponseDto
    {
        public List<WasteAttributeDefinitionDto> Attributes { get; set; } = [];
    }

    // --------------------------
    // ส่วนของการ "ทดสอบค่า Waste"
    // --------------------------

    // ค่าที่ผู้ใช้กรอกเข้ามาในฟอร์มทดสอบ
    public class WasteTestInputAttributeDto
    {
        [Required]
        public string AttributeKey { get; set; } = string.Empty;

        // หน่วยที่ผู้ใช้เลือก (ถ้ามี)
        public string? UnitKey { get; set; }

        // เก็บค่าแบบยืดหยุ่น (ใช้ตามชนิดของ Attribute)
        public decimal? DecimalValue { get; set; } // สำหรับ Numeric
        public bool? BoolValue { get; set; }       // สำหรับ Boolean
        public string? TextValue { get; set; }     // สำหรับ Text/Option
        public DateTime? DateTimeValue { get; set; } // เผื่ออนาคต
    }

    public class WasteEvaluateRequestDto
    {
        // ค่าที่กรอกจากแบบฟอร์ม
        [Required]
        public List<WasteTestInputAttributeDto> Attributes { get; set; } = [];
    }

    // --- ผลตรวจระดับ Attribute ---
    public class AttributeCheckResultDto
    {
        public string AttributeKey { get; set; } = string.Empty;
        public string AttributeName { get; set; } = string.Empty;
        public bool IsSatisfied { get; set; }

        // Operator และ Expected/Actual ไว้แสดงผลแบบอ่านง่าย
        public WasteComparisonOperator Operator { get; set; }
        public string ExpectedDescription { get; set; } = string.Empty;
        public string ActualDescription { get; set; } = string.Empty;

        // รายละเอียดเพิ่มเติม (เช่น "ผ่านช่วง [1,4] แต่ไม่ผ่าน [12,14]")
        public string? Details { get; set; }
    }

    // --- ผลตรวจระดับสูตร (Formula) ---
    public class FormulaMatchResultDto
    {
        public int FormulaId { get; set; }
        public string FormulaName { get; set; } = string.Empty;

        // true = ผ่านทุกเงื่อนไขของสูตร
        public bool IsMatch { get; set; }

        // รายการผลตรวจระดับ Attribute (เพื่อบอกว่าข้อไหนผ่าน/ไม่ผ่าน)
        public List<AttributeCheckResultDto> AttributeResults { get; set; } = [];
    }

    // --- ผลลัพธ์รวมจากการ Evaluate ---
    public class WasteEvaluateResponseDto
    {
        // สูตรทั้งหมดที่ตรวจเทียบ พร้อมสถานะผ่าน/ไม่ผ่านและรายละเอียด
        public List<FormulaMatchResultDto> Results { get; set; } = [];

        // จำนวนสูตรที่ “ผ่าน” ทั้งหมด
        public int TotalMatchedCount => Results.Count(r => r.IsMatch);
    }
}
