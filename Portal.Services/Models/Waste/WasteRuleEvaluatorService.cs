// File: Portal.Services/Models/Waste/WasteRuleEvaluatorService.cs
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Portal.Services.Models;
using Portal.Shared.Models.Entities.Waste;

namespace Portal.Services.Models.Waste
{
    #region DTOs: Request/Response

    /// <summary>
    /// ค่าที่ผู้ใช้กรอกเข้ามาเพื่อทดสอบ (อ้างด้วย Attribute Code หรือ Id อย่างใดอย่างหนึ่ง)
    /// แนะนำให้ใช้ Code (เช่น "Viscosity", "pH", "StorageType")
    /// </summary>
    public class WasteAttributeInput
    {
        public int? AttributeId { get; set; }
        public string? AttributeCode { get; set; }
        /// <summary>
        /// ค่าเป็น string เสมอแล้วให้ Service แปลงตาม DataType ของ Attribute
        /// ตัวอย่าง: "250", "ของเหลว", "False", "IBC"
        /// </summary>
        public string? Value { get; set; }
    }

    /// <summary>
    /// ผลการตรวจสอบในระดับ "เงื่อนไข 1 ข้อ"
    /// </summary>
    public class EvaluationConditionResult
    {
        public int ConditionId { get; set; }
        public int AttributeId { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string Expected { get; set; } = string.Empty;
        public string Actual { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string? Note { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// ผลการตรวจสอบในระดับ "สูตร 1 ตัว"
    /// </summary>
    public class EvaluationFormulaResult
    {
        public int FormulaId { get; set; }
        public string FormulaName { get; set; } = string.Empty;
        public bool Passed { get; set; }

        /// <summary>
        /// เงื่อนไขทั้งหมดของสูตร (รวมที่ผ่านและไม่ผ่าน)
        /// </summary>
        public List<EvaluationConditionResult> Conditions { get; set; } = [];

        /// <summary>
        /// รายการ Attribute ที่ "ผ่าน" (OR ภายใน Attribute เดียวกัน) และ "ไม่ผ่าน"
        /// </summary>
        public List<int> PassedAttributeIds { get; set; } = [];
        public List<int> FailedAttributeIds { get; set; } = [];
    }

    /// <summary>
    /// ผลสรุปทั้งหมด (ทุกสูตร)
    /// </summary>
    public class EvaluationSummary
    {
        public List<EvaluationFormulaResult> FormulaResults { get; set; } = [];

        /// <summary>สูตรที่ผ่านทั้งหมด</summary>
        public IEnumerable<EvaluationFormulaResult> PassedFormulas =>
            FormulaResults.Where(f => f.Passed);

        /// <summary>สูตรที่ไม่ผ่านทั้งหมด</summary>
        public IEnumerable<EvaluationFormulaResult> FailedFormulas =>
            FormulaResults.Where(f => !f.Passed);
    }

    /// <summary>
    /// DTO สำหรับสร้างแบบฟอร์มอินพุตในหน้า "ทดสอบสูตร"
    /// </summary>
    public class WasteAttributeSchemaItem
    {
        public int AttributeId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public AttributeDataType DataType { get; set; }
        public string? Unit { get; set; }
        public string[] AllowedValues { get; set; } = [];
        public bool IsActive { get; set; }
    }

    #endregion

    /// <summary>
    /// เซอร์วิสหลักสำหรับ:
    /// - ดึงสคีมา Attributes เพื่อสร้างฟอร์มทดสอบ
    /// - ประเมินค่าที่กรอกกับสูตรทั้งหมด/ทีละสูตร
    /// - คืนรายงานผ่าน/ไม่ผ่านรายข้อ อย่างละเอียด
    /// </summary>
    public class WasteRuleEvaluatorService(PortalDbContext db)
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        #region Public APIs

        /// <summary>
        /// ดึงสคีมาสำหรับสร้างฟอร์มกรอกค่า Waste (เฉพาะ Attributes ที่ IsActive)
        /// </summary>
        public async Task<List<WasteAttributeSchemaItem>> GetInputSchemaAsync(CancellationToken ct = default)
        {
            var attrs = await db.WasteAttributes
                                 .AsNoTracking()
                                 .Where(a => a.IsActive)
                                 .OrderBy(a => a.Name)
                                 .ToListAsync(ct);

            return [.. attrs.Select(a => new WasteAttributeSchemaItem
            {
                AttributeId = a.Id,
                Code = a.Code ?? string.Empty,
                Name = a.Name,
                DataType = a.DataType,
                Unit = a.Unit,
                AllowedValues = SplitCsv(a.AllowedValuesCsv),
                IsActive = a.IsActive
            })];
        }

        /// <summary>
        /// ประเมินกับ "ทุกสูตรที่ Active"
        /// </summary>
        public async Task<EvaluationSummary> EvaluateAllFormulasAsync(IEnumerable<WasteAttributeInput> inputs, CancellationToken ct = default)
        {
            // โหลดสูตรทั้งหมด + เงื่อนไข + Attribute
            var formulas = await db.WasteFormulas
                                    .AsNoTracking()
                                    .Where(f => f.IsActive)
                                    .Include(f => f.Conditions)
                                        .ThenInclude(c => c.Attribute)
                                    .ToListAsync(ct);

            var results = new List<EvaluationFormulaResult>();
            foreach (var f in formulas)
            {
                results.Add(EvaluateSingleFormula(f, inputs));
            }

            // จัดลำดับแสดงผล: ผ่านมาก่อน
            results = [.. results.OrderByDescending(r => r.Passed)
                             .ThenBy(r => r.FailedAttributeIds.Count)
                             .ThenBy(r => r.FormulaName)];

            return new EvaluationSummary { FormulaResults = results };
        }

        /// <summary>
        /// ประเมิน "สูตรเดียว" (ใช้เมื่อผู้ใช้เลือกสูตรมาทดสอบเจาะจง)
        /// </summary>
        public async Task<EvaluationFormulaResult?> EvaluateFormulaAsync(int formulaId, IEnumerable<WasteAttributeInput> inputs, CancellationToken ct = default)
        {
            var formula = await db.WasteFormulas
                                   .AsNoTracking()
                                   .Include(f => f.Conditions)
                                       .ThenInclude(c => c.Attribute)
                                   .FirstOrDefaultAsync(f => f.Id == formulaId && f.IsActive, ct);
            if (formula == null) return null;
            return EvaluateSingleFormula(formula, inputs);
        }

        #endregion

        #region Core Evaluation

        private static EvaluationFormulaResult EvaluateSingleFormula(WasteFormula formula, IEnumerable<WasteAttributeInput> inputs)
        {
            // Map อินพุตเพื่อ lookup เร็ว
            var inputById = inputs
                .Where(i => i.AttributeId.HasValue && !string.IsNullOrWhiteSpace(i.Value))
                .GroupBy(i => i.AttributeId!.Value)
                .ToDictionary(g => g.Key, g => g.First().Value!.Trim());

            var inputByCode = inputs
                .Where(i => !string.IsNullOrWhiteSpace(i.AttributeCode) && !string.IsNullOrWhiteSpace(i.Value))
                .GroupBy(i => NormalizeKey(i.AttributeCode!))
                .ToDictionary(g => g.Key, g => g.First().Value!.Trim());

            var result = new EvaluationFormulaResult
            {
                FormulaId = formula.Id,
                FormulaName = formula.Name,
                Passed = false
            };

            // Group เงื่อนไขตาม Attribute (OR ภายในกลุ่มเดียวกัน)
            var groups = formula.Conditions
                                .Where(c => c.IsActive)
                                .GroupBy(c => c.AttributeId);

            var allGroupsPassed = true;

            foreach (var g in groups)
            {
                var attribute = g.First().Attribute;
                var attrId = attribute.Id;

                // เลือกค่าจาก input: ถ้าไม่เจอ -> Fail กลุ่มนี้
                var actualStr = FindInputValue(attribute, inputById, inputByCode);

                var groupAnyPassed = false;

                foreach (var cond in g.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id))
                {
                    var (passed, expected, actual, note) = EvaluateCondition(attribute, cond, actualStr);
                    result.Conditions.Add(new EvaluationConditionResult
                    {
                        ConditionId = cond.Id,
                        AttributeId = attribute.Id,
                        AttributeName = attribute.Name,
                        Operator = cond.Operator.ToString(),
                        Expected = expected,
                        Actual = actual,
                        Passed = passed,
                        Note = string.IsNullOrWhiteSpace(cond.Note) ? note : cond.Note,
                        DisplayOrder = cond.DisplayOrder
                    });
                    groupAnyPassed = groupAnyPassed || passed;
                }

                if (groupAnyPassed)
                    result.PassedAttributeIds.Add(attrId);
                else
                {
                    result.FailedAttributeIds.Add(attrId);
                    allGroupsPassed = false;
                }
            }

            result.Passed = allGroupsPassed;
            return result;
        }

        /// <summary>
        /// ประเมินเงื่อนไขหนึ่งข้อ เทียบค่าอินพุตที่เป็น string (หรือ null ถ้าไม่ได้กรอก)
        /// คืน: (Passed, ExpectedText, ActualText, Note)
        /// </summary>
        private static (bool passed, string expected, string actual, string? note) EvaluateCondition(
            WasteAttribute attribute, WasteFormulaCondition cond, string? actualStr)
        {
            // สร้างข้อความ Expected สำหรับรายงาน
            var expectedText = BuildExpectedText(attribute, cond);

            // หากไม่มีค่าอินพุต -> ไม่ผ่านและรายงาน
            if (string.IsNullOrWhiteSpace(actualStr))
            {
                var actualText = "(ไม่มีค่าอินพุต)";
                return (false, expectedText, actualText, "ไม่ได้ระบุค่า");
            }

            // แปลงค่าอินพุตตามชนิดข้อมูล
            switch (attribute.DataType)
            {
                case AttributeDataType.Boolean:
                    {
                        if (!TryParseBoolean(actualStr, out var actualBool))
                            return (false, expectedText, actualStr, $"รูปแบบ Boolean ไม่ถูกต้อง (ตัวอย่างที่ยอมรับ: true/false, 1/0, ใช่/ไม่, เกิดฟอง/ไม่เกิดฟอง)");

                        // Value1 สำหรับ Equal/NotEqual, ส่วน IN/NOT IN ไม่รองรับกับ Boolean
                        if (cond.Operator == ComparisonOperator.Equal || cond.Operator == ComparisonOperator.NotEqual)
                        {
                            if (!TryParseBoolean(cond.Value1, out var expectedBool))
                                return (false, expectedText, actualStr, "ค่าที่กำหนดในสูตร (Value1) ไม่ใช่ Boolean");

                            var eq = actualBool == expectedBool;
                            return (cond.Operator == ComparisonOperator.Equal ? eq : !eq, expectedText, actualStr, null);
                        }

                        return (false, expectedText, actualStr, "Operator นี้ไม่รองรับกับ Boolean");
                    }

                case AttributeDataType.Numeric:
                    {
                        if (!TryParseDouble(actualStr, out var actualNum))
                            return (false, expectedText, actualStr, "รูปแบบตัวเลขไม่ถูกต้อง");

                        switch (cond.Operator)
                        {
                            case ComparisonOperator.Equal:
                            case ComparisonOperator.NotEqual:
                            case ComparisonOperator.GreaterThan:
                            case ComparisonOperator.GreaterOrEqual:
                            case ComparisonOperator.LessThan:
                            case ComparisonOperator.LessOrEqual:
                                if (!TryParseDouble(cond.Value1, out var v1))
                                    return (false, expectedText, actualStr, "สูตรกำหนดค่า Value1 ไม่ถูกต้อง");
                                var ok = CompareNumber(actualNum, v1, cond.Operator);
                                return (ok, expectedText, actualStr, null);

                            case ComparisonOperator.Between:
                                if (!TryParseDouble(cond.Value1, out var min) || !TryParseDouble(cond.Value2, out var max))
                                    return (false, expectedText, actualStr, "สูตรกำหนดช่วงไม่ถูกต้อง");
                                var inRange = actualNum >= min && actualNum <= max;
                                return (inRange, expectedText, actualStr, null);

                            case ComparisonOperator.In:
                            case ComparisonOperator.NotIn:
                                // รองรับชุดค่าตัวเลขผ่าน CSV
                                var list = SplitCsv(cond.ValueListCsv)
                                            .Select(s => TryParseDouble(s, out var n) ? n : (double?)null)
                                            .Where(n => n.HasValue)
                                            .Select(n => n!.Value)
                                            .ToHashSet();
                                var contains = list.Contains(actualNum);
                                return (cond.Operator == ComparisonOperator.In ? contains : !contains, expectedText, actualStr, null);

                            default:
                                return (false, expectedText, actualStr, "Operator นี้ไม่รองรับกับ Numeric");
                        }
                    }

                case AttributeDataType.Text:
                    {
                        var actualNorm = NormalizeText(actualStr);

                        switch (cond.Operator)
                        {
                            case ComparisonOperator.Equal:
                            case ComparisonOperator.NotEqual:
                                {
                                    var v1 = NormalizeText(cond.Value1);
                                    var eq = actualNorm == v1;
                                    return (cond.Operator == ComparisonOperator.Equal ? eq : !eq, expectedText, actualStr, null);
                                }
                            case ComparisonOperator.In:
                            case ComparisonOperator.NotIn:
                                {
                                    var set = SplitCsv(cond.ValueListCsv).Select(NormalizeText).ToHashSet();
                                    var contains = set.Contains(actualNorm);
                                    return (cond.Operator == ComparisonOperator.In ? contains : !contains, expectedText, actualStr, null);
                                }
                            default:
                                return (false, expectedText, actualStr, "Operator นี้ไม่รองรับกับ Text/Enum");
                        }
                    }

                case AttributeDataType.DateTime:
                    {
                        if (!DateTime.TryParse(actualStr, Invariant, DateTimeStyles.AssumeLocal, out var actualDate))
                            return (false, expectedText, actualStr, "รูปแบบวันที่ไม่ถูกต้อง (เช่น 2025-08-23 14:30)");

                        switch (cond.Operator)
                        {
                            case ComparisonOperator.Equal:
                            case ComparisonOperator.NotEqual:
                            case ComparisonOperator.GreaterThan:
                            case ComparisonOperator.GreaterOrEqual:
                            case ComparisonOperator.LessThan:
                            case ComparisonOperator.LessOrEqual:
                                if (!DateTime.TryParse(cond.Value1, Invariant, DateTimeStyles.AssumeLocal, out var dt1))
                                    return (false, expectedText, actualStr, "สูตรกำหนดค่า Value1 (วันที่) ไม่ถูกต้อง");
                                var ok = CompareDate(actualDate, dt1, cond.Operator);
                                return (ok, expectedText, actualStr, null);

                            case ComparisonOperator.Between:
                                if (!DateTime.TryParse(cond.Value1, Invariant, DateTimeStyles.AssumeLocal, out var dmin) ||
                                    !DateTime.TryParse(cond.Value2, Invariant, DateTimeStyles.AssumeLocal, out var dmax))
                                    return (false, expectedText, actualStr, "สูตรกำหนดช่วงวันที่ไม่ถูกต้อง");
                                var inRange = actualDate >= dmin && actualDate <= dmax;
                                return (inRange, expectedText, actualStr, null);

                            default:
                                return (false, expectedText, actualStr, "Operator นี้ไม่รองรับกับ DateTime");
                        }
                    }

                default:
                    return (false, expectedText, actualStr, "DataType ไม่รองรับ");
            }
        }

        #endregion

        #region Helpers: Parsing/Comparison/Text

        private static string BuildExpectedText(WasteAttribute attr, WasteFormulaCondition cond)
        {
            string unit = string.IsNullOrWhiteSpace(attr.Unit) ? "" : $" {attr.Unit}";
            string op = cond.Operator.ToString();

            string expected = cond.Operator switch
            {
                ComparisonOperator.Between => $"{attr.Name} อยู่ระหว่าง {cond.Value1}{unit} ถึง {cond.Value2}{unit}",
                ComparisonOperator.In => $"{attr.Name} เป็นหนึ่งใน ({string.Join(", ", SplitCsv(cond.ValueListCsv))})",
                ComparisonOperator.NotIn => $"{attr.Name} ไม่เป็นหนึ่งใน ({string.Join(", ", SplitCsv(cond.ValueListCsv))})",
                _ => $"{attr.Name} {OpSymbol(cond.Operator)} {cond.Value1}{unit}"
            };

            return expected;
        }

        private static string OpSymbol(ComparisonOperator op) => op switch
        {
            ComparisonOperator.Equal => "=",
            ComparisonOperator.NotEqual => "!=",
            ComparisonOperator.GreaterThan => ">",
            ComparisonOperator.GreaterOrEqual => ">=",
            ComparisonOperator.LessThan => "<",
            ComparisonOperator.LessOrEqual => "<=",
            _ => op.ToString()
        };

        private static string? FindInputValue(WasteAttribute attribute,
            Dictionary<int, string> inputById,
            Dictionary<string, string> inputByCode)
        {
            if (inputById.TryGetValue(attribute.Id, out var v)) return v;

            if (!string.IsNullOrWhiteSpace(attribute.Code))
            {
                var key = NormalizeKey(attribute.Code);
                if (inputByCode.TryGetValue(key, out var v2)) return v2;
            }

            // เผื่อกรณีส่งชื่อ (Name) มาเป็น key
            var fallbackKey = NormalizeKey(attribute.Name);
            if (inputByCode.TryGetValue(fallbackKey, out var v3)) return v3;

            return null;
        }

        private static bool CompareNumber(double actual, double v1, ComparisonOperator op) => op switch
        {
            ComparisonOperator.Equal => NearlyEqual(actual, v1),
            ComparisonOperator.NotEqual => !NearlyEqual(actual, v1),
            ComparisonOperator.GreaterThan => actual > v1,
            ComparisonOperator.GreaterOrEqual => actual >= v1,
            ComparisonOperator.LessThan => actual < v1,
            ComparisonOperator.LessOrEqual => actual <= v1,
            _ => false
        };

        private static bool CompareDate(DateTime actual, DateTime v1, ComparisonOperator op) => op switch
        {
            ComparisonOperator.Equal => actual == v1,
            ComparisonOperator.NotEqual => actual != v1,
            ComparisonOperator.GreaterThan => actual > v1,
            ComparisonOperator.GreaterOrEqual => actual >= v1,
            ComparisonOperator.LessThan => actual < v1,
            ComparisonOperator.LessOrEqual => actual <= v1,
            _ => false
        };

        private static bool NearlyEqual(double a, double b, double eps = 1e-9) => Math.Abs(a - b) <= eps;

        private static bool TryParseDouble(string? s, out double value)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                value = 0;
                return false;
            }

            // รองรับทั้งจุดและจุลภาค โดยลบ comma ก่อน
            var cleaned = s.Replace(",", "").Trim();
            return double.TryParse(cleaned, NumberStyles.Float | NumberStyles.AllowThousands, Invariant, out value);
        }

        private static bool TryParseBoolean(string? s, out bool value)
        {
            value = false;
            if (string.IsNullOrWhiteSpace(s)) return false;

            var t = s.Trim().ToLowerInvariant();

            // คำที่มักใช้
            if (t is "true" or "1" or "yes" or "y" or "ใช่" or "เกิดฟอง") { value = true; return true; }
            if (t is "false" or "0" or "no" or "n" or "ไม่" or "ไม่เกิดฟอง") { value = false; return true; }

            // ลอง bool มาตรฐาน
            return bool.TryParse(t, out value);
        }

        private static string NormalizeKey(string s)
        {
            return new string([.. s.Where(ch => !char.IsWhiteSpace(ch))]).ToLowerInvariant();
        }

        private static string NormalizeText(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            // ตัดช่องว่างส่วนเกิน + แปลงเป็นตัวเล็ก เพื่อเทียบแบบ case-insensitive และไม่สนใจช่องว่าง
            return NormalizeKey(s);
        }

        private static string[] SplitCsv(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return [];
            return csv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion
    }
}
