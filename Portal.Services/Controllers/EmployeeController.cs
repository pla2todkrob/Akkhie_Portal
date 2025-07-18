﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Auth;
using Portal.Shared.Models.DTOs.Shared;
using System;
using System.Threading.Tasks;

namespace Portal.Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeeController(IAuthService authService, IEmployeeService employeeService, ILogger<EmployeeController> logger) : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await authService.LoginAsync(request, ipAddress);

                if (!result.Success)
                {
                    return BadRequest(ApiResponse<LoginResponse>.ErrorResponse(result.ErrorMessage ?? "การเข้าสู่ระบบล้มเหลว"));
                }

                return Ok(ApiResponse<LoginResponse>.SuccessResponse(result, "เข้าสู่ระบบสำเร็จ"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during employee login");
                return StatusCode(500, ApiResponse<LoginResponse>.ErrorResponse($"เกิดข้อผิดพลาด: {ex.GetBaseException().Message}"));
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await authService.RegisterAsync(request, ipAddress);

                if (!result.Success)
                {
                    return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse(
                        "การลงทะเบียนล้มเหลว",
                        result.Errors));
                }

                return Ok(ApiResponse<RegisterResponse>.SuccessResponse(result, "ลงทะเบียนสำเร็จ"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during employee registration");
                return StatusCode(500, ApiResponse<RegisterResponse>.ErrorResponse($"เกิดข้อผิดพลาด: {ex.GetBaseException().Message}"));
            }
        }

        [HttpGet("{employeeId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEmployeeById(Guid employeeId)
        {
            try
            {
                if (employeeId == Guid.Empty)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Invalid employee ID."));
                }

                var employee = await employeeService.SearchAsync(employeeId);
                if (employee == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Employee not found."));
                }

                return Ok(ApiResponse.SuccessResponse(employee));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during getting employee by ID");
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาด: {ex.GetBaseException().Message}"));
            }
        }

        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetEmployeeByUsername(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest(ApiResponse.ErrorResponse("Username cannot be empty."));
                }

                var employee = await employeeService.SearchByUsernameAsync(username);
                if (employee == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Employee not found."));
                }

                return Ok(ApiResponse.SuccessResponse(employee));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during getting employee by username");
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาด: {ex.GetBaseException().Message}"));
            }
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetEmployeeByEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(ApiResponse.ErrorResponse("Email cannot be empty."));
                }

                var employee = await employeeService.SearchByEmailAsync(email);
                if (employee == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Employee not found."));
                }

                return Ok(ApiResponse.SuccessResponse(employee));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during getting employee by email");
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาด: {ex.GetBaseException().Message}"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees([FromQuery] int? companyId, [FromQuery] int? divisionId, [FromQuery] int? departmentId, [FromQuery] int? sectionId)
        {
            try
            {
                var employees = await employeeService.GetAsync(companyId, divisionId, departmentId, sectionId);
                if (employees == null || employees.Count == 0)
                {
                    return NotFound(ApiResponse.ErrorResponse("No employees found matching the criteria."));
                }

                return Ok(ApiResponse.SuccessResponse(employees));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during getting employees");
                return StatusCode(500, ApiResponse.ErrorResponse($"เกิดข้อผิดพลาด: {ex.GetBaseException().Message}"));
            }
        }

    }
}
