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
    public class DepartmentService(PortalDbContext context) : IDepartmentService
    {
        public async Task<IEnumerable<DepartmentViewModel>> GetAllAsync()
        {
            return await context.Departments
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
            return await context.Departments
                .Where(d => d.Id == id)
                .Select(d => new DepartmentViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    DivisionId = d.DivisionId,
                    SectionViewModels = d.Sections.Select(s => new SectionViewModel { Id = s.Id, Name = s.Name }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<SectionViewModel>> GetSectionsByDepartmentIdAsync(int departmentId)
        {
            return await context.Sections
                .Where(s => s.DepartmentId == departmentId)
                .Select(s => new SectionViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    DepartmentId = s.DepartmentId
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

            context.Departments.Add(department);
            await context.SaveChangesAsync();
            return new ApiResponse<Department> { Success = true, Data = department };
        }

        public async Task<ApiResponse<Department>> UpdateAsync(int id, DepartmentViewModel viewModel)
        {
            var department = await context.Departments.Include(d => d.Sections).FirstOrDefaultAsync(d => d.Id == id);
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

            await context.SaveChangesAsync();
            return new ApiResponse<Department> { Success = true, Data = department };
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var department = await context.Departments.FindAsync(id);
            if (department == null)
            {
                return new ApiResponse { Success = false, Message = "Department not found." };
            }

            context.Departments.Remove(department);
            await context.SaveChangesAsync();
            return new ApiResponse { Success = true };
        }
    }
}