// FileName: Portal.Services/Models/DepartmentService.cs
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
    public class DepartmentService : IDepartmentService
    {
        private readonly PortalDbContext _context;

        public DepartmentService(PortalDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DepartmentViewModel>> GetAllAsync()
        {
            return await _context.Departments
                .Select(d => new DepartmentViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    DivisionId = d.DivisionId,
                    DivisionName = d.Division.Name,
                    TotalSection = d.Sections.Count()
                }).ToListAsync();
        }

        public async Task<DepartmentViewModel> GetByIdAsync(int id)
        {
            return await _context.Departments
                .Where(d => d.Id == id)
                .Select(d => new DepartmentViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    DivisionId = d.DivisionId,
                    SectionViewModels = d.Sections.Select(s => new SectionViewModel { Id = s.Id, Name = s.Name }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<DepartmentViewModel>> GetByDivisionIdAsync(int divisionId)
        {
            return await _context.Departments
                .Where(d => d.DivisionId == divisionId)
                .Select(d => new DepartmentViewModel
                {
                    Id = d.Id,
                    Name = d.Name
                }).ToListAsync();
        }

        public async Task<ApiResponse<Department>> CreateAsync(DepartmentViewModel viewModel)
        {
            var department = new Department
            {
                Name = viewModel.Name,
                DivisionId = viewModel.DivisionId
            };

            foreach (var sectionVm in viewModel.SectionViewModels)
            {
                if (!string.IsNullOrWhiteSpace(sectionVm.Name))
                {
                    department.Sections.Add(new Section { Name = sectionVm.Name });
                }
            }

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            return new ApiResponse<Department> { Success = true, Data = department };
        }

        public async Task<ApiResponse<Department>> UpdateAsync(int id, DepartmentViewModel viewModel)
        {
            var department = await _context.Departments.Include(d => d.Sections).FirstOrDefaultAsync(d => d.Id == id);
            if (department == null)
            {
                return new ApiResponse<Department> { Success = false, Message = "Department not found." };
            }

            department.Name = viewModel.Name;
            department.DivisionId = viewModel.DivisionId;

            department.Sections.Clear();
            foreach (var sectionVm in viewModel.SectionViewModels)
            {
                if (!string.IsNullOrWhiteSpace(sectionVm.Name))
                {
                    department.Sections.Add(new Section { Name = sectionVm.Name });
                }
            }

            await _context.SaveChangesAsync();
            return new ApiResponse<Department> { Success = true, Data = department };
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return new ApiResponse { Success = false, Message = "Department not found." };
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return new ApiResponse { Success = true };
        }
    }
}