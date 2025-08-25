// File: Portal.Services/Controllers/WasteAdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal.Services.Models;
using Portal.Shared.Models.Entities.Waste;
using System.Security.Claims;

namespace Portal.Services.Controllers
{
    /// <summary>
    /// ชุด API สำหรับ "จัดการ" Waste Attributes / Formulas / Conditions
    /// หมายเหตุ: อนุญาตเฉพาะพนักงานที่มี IsSystemRole = true
    /// </summary>
    [ApiController]
    [Route("api/waste/admin")]
    [Authorize] // ต้องล็อกอินก่อน
    public class WasteAdminController(PortalDbContext db, ILogger<WasteAdminController> logger) : ControllerBase
    {

        #region Helpers

        private bool IsSystemRole()
        {
            var val = User.FindFirst("IsSystemRole")?.Value ?? "false";
            return val.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private Guid? CurrentEmployeeId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (Guid.TryParse(id, out var g)) return g;
            return null;
        }

        private ForbidResult? ForbidIfNotSystemRole()
        {
            if (!IsSystemRole())
                return Forbid(); // 403
            return null;
        }

        #endregion

        // -----------------------------------------------------------
        // ATTRIBUTES
        // -----------------------------------------------------------

        public record AttributeUpsertDto(
            string Name,
            string? Code,
            AttributeDataType DataType,
            string? Unit,
            string? AllowedValuesCsv,
            bool IsActive = true
        );

        [HttpGet("attributes")]
        public async Task<IActionResult> GetAttributes([FromQuery] bool includeInactive = false, CancellationToken ct = default)
        {
            var q = db.WasteAttributes.AsNoTracking();
            if (!includeInactive) q = q.Where(x => x.IsActive);
            var list = await q.OrderBy(a => a.Name).ToListAsync(ct);
            return Ok(list);
        }

        [HttpGet("attributes/{id:int}")]
        public async Task<IActionResult> GetAttribute([FromRoute] int id, CancellationToken ct = default)
        {
            var a = await db.WasteAttributes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            return a is null ? NotFound() : Ok(a);
        }

        [HttpPost("attributes")]
        public async Task<IActionResult> CreateAttribute([FromBody] AttributeUpsertDto dto, CancellationToken ct)
        {
            var forbid = ForbidIfNotSystemRole();
            if (forbid is not null) return forbid;

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required.");

            var entity = new WasteAttribute
            {
                Name = dto.Name.Trim(),
                Code = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code.Trim(),
                DataType = dto.DataType,
                Unit = string.IsNullOrWhiteSpace(dto.Unit) ? null : dto.Unit.Trim(),
                AllowedValuesCsv = string.IsNullOrWhiteSpace(dto.AllowedValuesCsv) ? null : dto.AllowedValuesCsv.Trim(),
                IsActive = dto.IsActive
            };

            db.WasteAttributes.Add(entity);
            await db.SaveChangesAsync(ct);
            return CreatedAtAction(nameof(GetAttribute), new { id = entity.Id }, entity);
        }

        [HttpPut("attributes/{id:int}")]
        public async Task<IActionResult> UpdateAttribute([FromRoute] int id, [FromBody] AttributeUpsertDto dto, CancellationToken ct)
        {
            var forbid = ForbidIfNotSystemRole();
            if (forbid is not null) return forbid;

            var entity = await db.WasteAttributes.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null) return NotFound();

            entity.Name = dto.Name.Trim();
            entity.Code = string.IsNullOrWhiteSpace(dto.Code) ? null : dto.Code.Trim();
            entity.DataType = dto.DataType;
            entity.Unit = string.IsNullOrWhiteSpace(dto.Unit) ? null : dto.Unit.Trim();
            entity.AllowedValuesCsv = string.IsNullOrWhiteSpace(dto.AllowedValuesCsv) ? null : dto.AllowedValuesCsv.Trim();
            entity.IsActive = dto.IsActive;

            await db.SaveChangesAsync(ct);
            return Ok(entity);
        }

        [HttpDelete("attributes/{id:int}")]
        public async Task<IActionResult> DeleteAttribute([FromRoute] int id, [FromQuery] bool hard = false, CancellationToken ct = default)
        {
            var forbid = ForbidIfNotSystemRole();
            if (forbid is not null) return forbid;

            var entity = await db.WasteAttributes.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null) return NotFound();

            // ถ้ามีการอ้างอิงในเงื่อนไข ห้าม hard delete
            var referenced = await db.WasteFormulaConditions.AnyAsync(c => c.AttributeId == id, ct);

            if (hard)
            {
                if (referenced)
                    return Conflict("Attribute นี้ถูกใช้งานอยู่ในเงื่อนไขของสูตร ไม่สามารถลบแบบถาวรได้");
                db.WasteAttributes.Remove(entity);
            }
            else
            {
                entity.IsActive = false; // soft-delete
            }

            await db.SaveChangesAsync(ct);
            return NoContent();
        }

        // -----------------------------------------------------------
        // FORMULAS
        // -----------------------------------------------------------

        public record FormulaUpsertDto(
            string Name,
            string? Description,
            bool IsActive = true
        );

        [HttpGet("formulas")]
        public async Task<IActionResult> GetFormulas([FromQuery] bool includeInactive = false, CancellationToken ct = default)
        {
            var list = await db.WasteFormulas
                .AsNoTracking()
                .Where(f => includeInactive || f.IsActive)
                .OrderBy(f => f.Name)
                .Select(f => new
                {
                    f.Id,
                    f.Name,
                    f.Description,
                    f.IsActive,
                    ConditionsCount = f.Conditions.Count // EF จะทำเป็น subquery ให้อัตโนมัติ ไม่ต้อง Include
                })
                .ToListAsync(ct);

            return Ok(list);
        }


        [HttpGet("formulas/{id:int}")]
        public async Task<IActionResult> GetFormula([FromRoute] int id, CancellationToken ct = default)
        {
            var f = await db.WasteFormulas
                             .AsNoTracking()
                             .Include(x => x.Conditions)
                                .ThenInclude(c => c.Attribute)
                             .FirstOrDefaultAsync(x => x.Id == id, ct);
            return f is null ? NotFound() : Ok(f);
        }

        [HttpPost("formulas")]
        public async Task<IActionResult> CreateFormula([FromBody] FormulaUpsertDto dto, CancellationToken ct)
        {
            var forbid = ForbidIfNotSystemRole();
            if (forbid is not null) return forbid;

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required.");

            var entity = new WasteFormula
            {
                Name = dto.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                CreatedAtUtc = DateTime.UtcNow,
                CreatedByEmployeeId = CurrentEmployeeId(),
                IsActive = dto.IsActive
            };

            db.WasteFormulas.Add(entity);
            await db.SaveChangesAsync(ct);
            return CreatedAtAction(nameof(GetFormula), new { id = entity.Id }, entity);
        }

        [HttpPut("formulas/{id:int}")]
        public async Task<IActionResult> UpdateFormula([FromRoute] int id, [FromBody] FormulaUpsertDto dto, CancellationToken ct)
        {
            var forbid = ForbidIfNotSystemRole();
            if (forbid is not null) return forbid;

            var entity = await db.WasteFormulas.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null) return NotFound();

            entity.Name = dto.Name.Trim();
            entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            entity.IsActive = dto.IsActive;

            await db.SaveChangesAsync(ct);
            return Ok(entity);
        }

        [HttpDelete("formulas/{id:int}")]
        public async Task<IActionResult> DeleteFormula([FromRoute] int id, [FromQuery] bool hard = false, CancellationToken ct = default)
        {
            var forbid = ForbidIfNotSystemRole();
            if (forbid is not null) return forbid;

            var entity = await db.WasteFormulas.Include(f => f.Conditions).FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null) return NotFound();

            if (hard)
            {
                if (entity.Conditions.Count != 0)
                    return Conflict("สูตรนี้มีเงื่อนไขอยู่ ต้องลบเงื่อนไขทั้งหมดก่อนจึงจะลบสูตรแบบถาวรได้");
                db.WasteFormulas.Remove(entity);
            }
            else
            {
                entity.IsActive = false; // soft-delete
            }

            await db.SaveChangesAsync(ct);
            return NoContent();
        }

        // -----------------------------------------------------------
        // CONDITIONS
        // -----------------------------------------------------------

        public record ConditionUpsertDto(
            int AttributeId,
            ComparisonOperator Operator,
            string? Value1,
            string? Value2,
            string? ValueListCsv,
            int? DisplayOrder,
            string? Note,
            bool IsActive = true
        );

        [HttpGet("formulas/{formulaId:int}/conditions")]
        public async Task<IActionResult> GetConditions([FromRoute] int formulaId, CancellationToken ct = default)
        {
            var exists = await db.WasteFormulas.AnyAsync(f => f.Id == formulaId, ct);
            if (!exists) return NotFound("สูตรไม่พบ");

            var list = await db.WasteFormulaConditions
                                .AsNoTracking()
                                .Include(c => c.Attribute)
                                .Where(c => c.FormulaId == formulaId)
                                .OrderBy(c => c.DisplayOrder)
                                .ThenBy(c => c.Id)
                                .ToListAsync(ct);
            return Ok(list);
        }

        [HttpPost("formulas/{formulaId:int}/conditions")]
        public async Task<IActionResult> CreateCondition([FromRoute] int formulaId, [FromBody] ConditionUpsertDto dto, CancellationToken ct)
        {
            var forbid = ForbidIfNotSystemRole();
            if (forbid is not null) return forbid;

            var formula = await db.WasteFormulas.FirstOrDefaultAsync(f => f.Id == formulaId, ct);
            if (formula == null) return NotFound("สูตรไม่พบ");

            // ตรวจ attribute exists
            var attr = await db.WasteAttributes.AsNoTracking().FirstOrDefaultAsync(a => a.Id == dto.AttributeId, ct);
            if (attr == null) return BadRequest("AttributeId ไม่ถูกต้อง");

            // ตรวจความสอดคล้อง operator ↔ datatype แบบเบื้องต้น
            if (!ValidateOperatorWithDataType(attr.DataType, dto.Operator, dto, out var error))
                return BadRequest(error);

            int displayOrder = dto.DisplayOrder ?? await db.WasteFormulaConditions.Where(c => c.FormulaId == formulaId).Select(c => (int?)c.DisplayOrder).MaxAsync(ct) + 1 ?? 1;

            var cond = new WasteFormulaCondition
            {
                FormulaId = formulaId,
                AttributeId = dto.AttributeId,
                Operator = dto.Operator,
                Value1 = string.IsNullOrWhiteSpace(dto.Value1) ? null : dto.Value1.Trim(),
                Value2 = string.IsNullOrWhiteSpace(dto.Value2) ? null : dto.Value2.Trim(),
                ValueListCsv = string.IsNullOrWhiteSpace(dto.ValueListCsv) ? null : dto.ValueListCsv.Trim(),
                DisplayOrder = displayOrder,
                Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim(),
                IsActive = dto.IsActive
            };

            db.WasteFormulaConditions.Add(cond);
            await db.SaveChangesAsync(ct);
            return CreatedAtAction(nameof(GetConditions), new { formulaId }, cond);
        }

        [HttpPut("conditions/{conditionId:int}")]
        public async Task<IActionResult> UpdateCondition([FromRoute] int conditionId, [FromBody] ConditionUpsertDto dto, CancellationToken ct)
        {
            var forbid = ForbidIfNotSystemRole();
            if (forbid is not null) return forbid;

            var cond = await db.WasteFormulaConditions.Include(c => c.Attribute).FirstOrDefaultAsync(c => c.Id == conditionId, ct);
            if (cond == null) return NotFound();

            // ถ้าเปลี่ยน AttributeId ต้องโหลดเพื่อ validate
            var attr = cond.Attribute;
            if (cond.AttributeId != dto.AttributeId)
            {
                attr = await db.WasteAttributes.FirstOrDefaultAsync(a => a.Id == dto.AttributeId, ct);
                if (attr == null) return BadRequest("AttributeId ไม่ถูกต้อง");
            }

            if (!ValidateOperatorWithDataType(attr!.DataType, dto.Operator, dto, out var error))
                return BadRequest(error);

            cond.AttributeId = dto.AttributeId;
            cond.Operator = dto.Operator;
            cond.Value1 = string.IsNullOrWhiteSpace(dto.Value1) ? null : dto.Value1.Trim();
            cond.Value2 = string.IsNullOrWhiteSpace(dto.Value2) ? null : dto.Value2.Trim();
            cond.ValueListCsv = string.IsNullOrWhiteSpace(dto.ValueListCsv) ? null : dto.ValueListCsv.Trim();
            cond.DisplayOrder = dto.DisplayOrder ?? cond.DisplayOrder;
            cond.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();
            cond.IsActive = dto.IsActive;

            await db.SaveChangesAsync(ct);
            return Ok(cond);
        }

        [HttpDelete("conditions/{conditionId:int}")]
        public async Task<IActionResult> DeleteCondition([FromRoute] int conditionId, CancellationToken ct = default)
        {
            var forbid = ForbidIfNotSystemRole();
            if (forbid is not null) return forbid;

            var cond = await db.WasteFormulaConditions.FirstOrDefaultAsync(c => c.Id == conditionId, ct);
            if (cond == null) return NotFound();

            db.WasteFormulaConditions.Remove(cond);
            await db.SaveChangesAsync(ct);
            return NoContent();
        }

        // (ทางเลือก) แทนทั้งชุดของสูตร (ลบเดิม-เพิ่มใหม่ในทรานแซคชันเดียว)
        public record BulkConditionDto(int AttributeId, ComparisonOperator Operator, string? Value1, string? Value2, string? ValueListCsv, int DisplayOrder, string? Note, bool IsActive = true);

        [HttpPut("formulas/{formulaId:int}/conditions:bulk")]
        public async Task<IActionResult> ReplaceConditions([FromRoute] int formulaId, [FromBody] List<BulkConditionDto> items, CancellationToken ct = default)
        {
            var forbid = ForbidIfNotSystemRole();
            if (forbid is not null) return forbid;

            var formula = await db.WasteFormulas.FirstOrDefaultAsync(f => f.Id == formulaId, ct);
            if (formula == null) return NotFound("สูตรไม่พบ");

            // Validate ล่วงหน้า
            var attrIds = items.Select(i => i.AttributeId).Distinct().ToList();
            var attrs = await db.WasteAttributes.Where(a => attrIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id, ct);
            foreach (var i in items)
            {
                if (!attrs.TryGetValue(i.AttributeId, out var a))
                    return BadRequest($"AttributeId {i.AttributeId} ไม่ถูกต้อง");
                if (!ValidateOperatorWithDataType(a.DataType, i.Operator, new ConditionUpsertDto(i.AttributeId, i.Operator, i.Value1, i.Value2, i.ValueListCsv, i.DisplayOrder, i.Note, i.IsActive), out var err))
                    return BadRequest($"DisplayOrder {i.DisplayOrder}: {err}");
            }

            using var tx = await db.Database.BeginTransactionAsync(ct);

            var old = db.WasteFormulaConditions.Where(c => c.FormulaId == formulaId);
            db.WasteFormulaConditions.RemoveRange(old);
            await db.SaveChangesAsync(ct);

            var news = items.Select(i => new WasteFormulaCondition
            {
                FormulaId = formulaId,
                AttributeId = i.AttributeId,
                Operator = i.Operator,
                Value1 = string.IsNullOrWhiteSpace(i.Value1) ? null : i.Value1.Trim(),
                Value2 = string.IsNullOrWhiteSpace(i.Value2) ? null : i.Value2.Trim(),
                ValueListCsv = string.IsNullOrWhiteSpace(i.ValueListCsv) ? null : i.ValueListCsv.Trim(),
                DisplayOrder = i.DisplayOrder,
                Note = string.IsNullOrWhiteSpace(i.Note) ? null : i.Note.Trim(),
                IsActive = i.IsActive
            }).ToList();

            db.WasteFormulaConditions.AddRange(news);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return Ok(new { replaced = news.Count });
        }

        // -----------------------------------------------------------
        // Validation helper
        // -----------------------------------------------------------
        private static bool ValidateOperatorWithDataType(AttributeDataType dt, ComparisonOperator op, ConditionUpsertDto dto, out string error)
        {
            error = string.Empty;

            switch (dt)
            {
                case AttributeDataType.Boolean:
                    if (op is ComparisonOperator.Equal or ComparisonOperator.NotEqual)
                    {
                        if (string.IsNullOrWhiteSpace(dto.Value1))
                        {
                            error = "Boolean ต้องระบุ Value1 เป็น true/false";
                            return false;
                        }
                        var t = dto.Value1.Trim().ToLowerInvariant();
                        if (t is not ("true" or "false" or "1" or "0" or "yes" or "no" or "y" or "n" or "ใช่" or "ไม่" or "เกิดฟอง" or "ไม่เกิดฟอง"))
                        {
                            error = "รูปแบบ Boolean ไม่ถูกต้อง";
                            return false;
                        }
                        return true;
                    }
                    error = "Boolean รองรับเฉพาะ Equal/NotEqual";
                    return false;

                case AttributeDataType.Text:
                    if (op is ComparisonOperator.Equal or ComparisonOperator.NotEqual)
                    {
                        if (string.IsNullOrWhiteSpace(dto.Value1))
                        {
                            error = "ต้องระบุ Value1 สำหรับ Text";
                            return false;
                        }
                        return true;
                    }
                    if (op is ComparisonOperator.In or ComparisonOperator.NotIn)
                    {
                        if (string.IsNullOrWhiteSpace(dto.ValueListCsv))
                        {
                            error = "ต้องระบุ ValueListCsv สำหรับ In/NotIn";
                            return false;
                        }
                        return true;
                    }
                    error = "Text รองรับ Equal/NotEqual/ In/NotIn เท่านั้น";
                    return false;

                case AttributeDataType.Numeric:
                    if (op is ComparisonOperator.Between)
                    {
                        if (string.IsNullOrWhiteSpace(dto.Value1) || string.IsNullOrWhiteSpace(dto.Value2))
                        {
                            error = "Between ต้องระบุ Value1 และ Value2";
                            return false;
                        }
                        return true;
                    }
                    if (op is ComparisonOperator.In or ComparisonOperator.NotIn)
                    {
                        if (string.IsNullOrWhiteSpace(dto.ValueListCsv))
                        {
                            error = "ต้องระบุ ValueListCsv สำหรับ In/NotIn";
                            return false;
                        }
                        return true;
                    }
                    // =, !=, >, >=, <, <= ต้องมี Value1
                    if (string.IsNullOrWhiteSpace(dto.Value1))
                    {
                        error = "ต้องระบุ Value1 สำหรับตัวเลข";
                        return false;
                    }
                    return true;

                case AttributeDataType.DateTime:
                    if (op is ComparisonOperator.Between)
                    {
                        if (string.IsNullOrWhiteSpace(dto.Value1) || string.IsNullOrWhiteSpace(dto.Value2))
                        {
                            error = "Between วันที่ ต้องระบุ Value1 และ Value2";
                            return false;
                        }
                        return true;
                    }
                    if (op is ComparisonOperator.Equal or ComparisonOperator.NotEqual or ComparisonOperator.GreaterThan or ComparisonOperator.GreaterOrEqual or ComparisonOperator.LessThan or ComparisonOperator.LessOrEqual)
                    {
                        if (string.IsNullOrWhiteSpace(dto.Value1))
                        {
                            error = "ต้องระบุ Value1 สำหรับวันที่";
                            return false;
                        }
                        return true;
                    }
                    error = "DateTime รองรับ =, !=, >, >=, <, <=, Between เท่านั้น";
                    return false;

                default:
                    error = "DataType ไม่รองรับ";
                    return false;
            }
        }
    }
}
