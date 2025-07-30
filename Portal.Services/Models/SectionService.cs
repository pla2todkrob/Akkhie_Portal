// FileName: Portal.Services/Models/SectionService.cs
using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.Services.Models
{
    public class SectionService(PortalDbContext context) : ISectionService
    {
        public async Task<IEnumerable<SectionViewModel>> GetAllAsync()
        {
            return await context.Sections
                .AsNoTracking()
                .Select(s => new SectionViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    DepartmentName = s.Department.Name,
                    DivisionName = s.Department.Division.Name,
                    CompanyName = s.Department.Division.Company.Name
                }).ToListAsync();
        }

        public async Task<SectionViewModel> GetByIdAsync(int id)
        {
            return await context.Sections
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new SectionViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    DepartmentId = s.DepartmentId,
                    DivisionId = s.Department.DivisionId,
                    CompanyId = s.Department.Division.CompanyId
                }).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<SectionViewModel>> GetByDepartmentIdAsync(int departmentId)
        {
            return await context.Sections
                .Where(s => s.DepartmentId == departmentId)
                .Select(s => new SectionViewModel
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToListAsync();
        }

        public async Task<ApiResponse<Section>> CreateAsync(SectionViewModel viewModel)
        {
            var section = new Section
            {
                Name = viewModel.Name,
                DepartmentId = viewModel.DepartmentId
            };
            context.Sections.Add(section);
            await context.SaveChangesAsync();
            return new ApiResponse<Section> { Success = true, Data = section, Message = "สร้างข้อมูลแผนกสำเร็จ" };
        }

        public async Task<ApiResponse<Section>> UpdateAsync(int id, SectionViewModel viewModel)
        {
            var section = await context.Sections.FindAsync(id);
            if (section == null)
            {
                return new ApiResponse<Section> { Success = false, Message = "ไม่พบข้อมูลแผนก" };
            }

            section.Name = viewModel.Name;
            section.DepartmentId = viewModel.DepartmentId;

            await context.SaveChangesAsync();
            return new ApiResponse<Section> { Success = true, Data = section, Message = "อัปเดตข้อมูลแผนกสำเร็จ" };
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var section = await context.Sections.FindAsync(id);
            if (section == null)
            {
                return new ApiResponse { Success = false, Message = "ไม่พบข้อมูลแผนก" };
            }

            context.Sections.Remove(section);
            await context.SaveChangesAsync();
            return new ApiResponse { Success = true, Message = "ลบข้อมูลสำเร็จ" };
        }
    }
}