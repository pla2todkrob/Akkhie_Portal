using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileUploadController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// Endpoint สำหรับอัพโหลดไฟล์
        /// </summary>
        /// <returns>ข้อมูลไฟล์ที่อัพโหลดสำเร็จ</returns>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(ApiResponse<List<FileUploadResultDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload()
        {
            try
            {
                var files = Request.Form.Files.ToList();
                if (files == null || !files.Any())
                {
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "No files uploaded." });
                }

                var uploadedFileEntities = await _fileService.UploadFilesAsync(files);

                // Map Entity to DTO
                var resultDto = uploadedFileEntities.Select(f => new FileUploadResultDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    OriginalFileName = f.OriginalFileName,
                    ContentType = f.ContentType,
                    FileSize = f.FileSize,
                    UploadPath = f.UploadPath
                }).ToList();

                return Ok(new ApiResponse<List<FileUploadResultDto>> { Success = true, Data = resultDto, Message = "Files uploaded successfully." });
            }
            catch (Exception ex)
            {
                // ควรมี Logger เพื่อบันทึก Exception
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }
    }
}
