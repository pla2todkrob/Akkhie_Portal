// File: Portal.Services/Controllers/WasteRulesController.cs
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Services.Waste;

namespace Portal.Services.Controllers
{
    /// <summary>
    /// API สำหรับดึงสคีมา Attribute เพื่อสร้างฟอร์ม และประเมินค่า Waste กับสูตร
    /// </summary>
    [ApiController]
    [Route("api/waste")]
    public class WasteRulesController(WasteRuleEvaluatorService evaluator, ILogger<WasteRulesController> logger) : ControllerBase
    {

        /// <summary>
        /// ดึงสคีมาของ Waste Attributes ทั้งหมดที่ Active
        /// ใช้สร้างแบบฟอร์มกรอกค่าในหน้า UI
        /// </summary>
        [HttpGet("schema")]
        [ProducesResponseType(typeof(List<WasteAttributeSchemaItem>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSchema(CancellationToken ct)
        {
            var schema = await evaluator.GetInputSchemaAsync(ct);
            return Ok(schema);
        }

        /// <summary>
        /// ประเมินค่าที่กรอกเทียบกับ "ทุกสูตร" ที่ Active
        /// </summary>
        [HttpPost("evaluate")]
        [ProducesResponseType(typeof(EvaluationSummary), StatusCodes.Status200OK)]
        public async Task<IActionResult> EvaluateAll([FromBody] List<WasteAttributeInput> inputs, CancellationToken ct)
        {
            if (inputs == null) return BadRequest("payload is null");
            var summary = await evaluator.EvaluateAllFormulasAsync(inputs, ct);
            return Ok(summary);
        }

        /// <summary>
        /// ประเมินค่าที่กรอกเทียบกับ "สูตรเดียว" ตาม id
        /// </summary>
        [HttpPost("evaluate/{formulaId:int}")]
        [ProducesResponseType(typeof(EvaluationFormulaResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EvaluateOne([FromRoute] int formulaId, [FromBody] List<WasteAttributeInput> inputs, CancellationToken ct)
        {
            if (inputs == null) return BadRequest("payload is null");
            var result = await evaluator.EvaluateFormulaAsync(formulaId, inputs, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
