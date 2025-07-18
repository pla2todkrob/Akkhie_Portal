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
    public class SectionService : ISectionService
    {
        private readonly PortalDbContext _context;

        public SectionService(PortalDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SectionViewModel>> GetAllAsync()
        {
            return await _context.Sections
                .Select(s => new SectionViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    DepartmentId = s.DepartmentId,
                    DepartmentName = s.Department.Name
                }).ToListAsync();
        }

        public async Task<SectionViewModel> GetByIdAsync(int id)
        {
            return await _context.Sections
                .Where(s => s.Id == id)
                .Select(s => new SectionViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    DepartmentId = s.DepartmentId,
                    DivisionId = s.Department.DivisionId
                }).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<SectionViewModel>> GetByDepartmentIdAsync(int departmentId)
        {
            return await _context.Sections
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
            _context.Sections.Add(section);
            await _context.SaveChangesAsync();
            return new ApiResponse<Section> { Success = true, Data = section };
        }

        public async Task<ApiResponse<Section>> UpdateAsync(int id, SectionViewModel viewModel)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null)
            {
                return new ApiResponse<Section> { Success = false, Message = "Section not found." };
            }

            section.Name = viewModel.Name;
            section.DepartmentId = viewModel.DepartmentId;

            await _context.SaveChangesAsync();
            return new ApiResponse<Section> { Success = true, Data = section };
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null)
            {
                return new ApiResponse { Success = false, Message = "Section not found." };
            }

            _context.Sections.Remove(section);
            await _context.SaveChangesAsync();
            return new ApiResponse { Success = true };
        }
    }
}