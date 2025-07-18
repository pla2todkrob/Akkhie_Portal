// FileName: Portal.Services/Models/DivisionService.cs
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
    public class DivisionService : IDivisionService
    {
        private readonly PortalDbContext _context;

        public DivisionService(PortalDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DivisionViewModel>> GetAllAsync()
        {
            return await _context.Divisions
                .Select(d => new DivisionViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    TotalDepartment = d.Departments.Count()
                }).ToListAsync();
        }



        public async Task<DivisionViewModel> GetByIdAsync(int id)
        {
            return await _context.Divisions
                .Where(d => d.Id == id)
                .Select(d => new DivisionViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    CompanyId = d.CompanyId,
                    DepartmentViewModels = d.Departments.Select(dept => new DepartmentViewModel { Id = dept.Id, Name = dept.Name }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<DepartmentViewModel>> GetDepartmentsByDivisionIdAsync(int divisionId)
        {
            return await _context.Departments
                .Where(d => d.DivisionId == divisionId)
                .Select(d => new DepartmentViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    DivisionId = d.DivisionId,
                    DivisionName = d.Division.Name,
                    TotalSection = d.Sections.Count()
                }).ToListAsync();
        }

        public async Task<ApiResponse<Division>> CreateAsync(DivisionViewModel viewModel)
        {
            var division = new Division { Name = viewModel.Name, CompanyId = viewModel.CompanyId };
            foreach (var deptVm in viewModel.DepartmentViewModels)
            {
                if (!string.IsNullOrWhiteSpace(deptVm.Name))
                {
                    division.Departments.Add(new Department { Name = deptVm.Name });
                }
            }
            _context.Divisions.Add(division);
            await _context.SaveChangesAsync();
            return new ApiResponse<Division> { Success = true, Data = division };
        }

        public async Task<ApiResponse<Division>> UpdateAsync(int id, DivisionViewModel viewModel)
        {
            var division = await _context.Divisions.Include(d => d.Departments).FirstOrDefaultAsync(d => d.Id == id);
            if (division == null)
            {
                return new ApiResponse<Division> { Success = false, Message = "Division not found." };
            }

            division.Name = viewModel.Name;
            division.CompanyId = viewModel.CompanyId;

            division.Departments.Clear();
            foreach (var deptVm in viewModel.DepartmentViewModels)
            {
                if (!string.IsNullOrWhiteSpace(deptVm.Name))
                {
                    division.Departments.Add(new Department { Name = deptVm.Name });
                }
            }

            await _context.SaveChangesAsync();
            return new ApiResponse<Division> { Success = true, Data = division };
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var division = await _context.Divisions.FindAsync(id);
            if (division == null)
            {
                return new ApiResponse { Success = false, Message = "Division not found." };
            }

            _context.Divisions.Remove(division);
            await _context.SaveChangesAsync();
            return new ApiResponse { Success = true };
        }
    }
}