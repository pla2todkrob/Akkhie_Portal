using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;

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
                .Include(d => d.Company)
                .Include(d => d.Departments)
                .Select(d => new DivisionViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    CompanyId = d.CompanyId,
                    CompanyName = d.Company.Name,
                    TotalDepartment = d.Departments.Count(),
                    DepartmentViewModels = d.Departments.Select(dep => new DepartmentViewModel
                    {
                        Id = dep.Id,
                        Name = dep.Name
                    }).ToList()
                }).ToListAsync();
        }

        public async Task<DivisionViewModel> GetByIdAsync(int id)
        {
            var division = await _context.Divisions
                .Include(d => d.Departments)
                .Include(d => d.Company)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (division == null) return null;

            return new DivisionViewModel
            {
                Id = division.Id,
                Name = division.Name,
                CompanyId = division.CompanyId,
                CompanyName = division.Company.Name,
                DepartmentViewModels = division.Departments.Select(dep => new DepartmentViewModel
                {
                    Id = dep.Id,
                    Name = dep.Name
                }).ToList()
            };
        }

        public async Task<ApiResponse> CreateAsync(DivisionViewModel viewModel)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var division = new Division
                {
                    Name = viewModel.Name,
                    CompanyId = viewModel.CompanyId
                };

                foreach (var deptVm in viewModel.DepartmentViewModels)
                {
                    division.Departments.Add(new Department { Name = deptVm.Name });
                }

                await _context.Divisions.AddAsync(division);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ApiResponse.SuccessResponse(null, "บันทึกข้อมูลสำเร็จ");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResponse.ErrorResponse(ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateAsync(int id, DivisionViewModel viewModel)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var division = await _context.Divisions
                    .Include(d => d.Departments)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (division == null)
                {
                    return ApiResponse.ErrorResponse("ไม่พบข้อมูล");
                }

                division.Name = viewModel.Name;
                division.CompanyId = viewModel.CompanyId;

                var existingDepts = division.Departments.ToList();
                var updatedDeptIds = viewModel.DepartmentViewModels.Select(d => d.Id).ToList();

                var deptsToRemove = existingDepts.Where(d => !updatedDeptIds.Contains(d.Id)).ToList();
                _context.Departments.RemoveRange(deptsToRemove);

                foreach (var deptVm in viewModel.DepartmentViewModels)
                {
                    var existingDept = existingDepts.FirstOrDefault(d => d.Id == deptVm.Id);
                    if (existingDept != null)
                    {
                        existingDept.Name = deptVm.Name;
                    }
                    else
                    {
                        division.Departments.Add(new Department { Name = deptVm.Name });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ApiResponse.SuccessResponse(null, "อัปเดตข้อมูลสำเร็จ");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResponse.ErrorResponse(ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var division = await _context.Divisions.FindAsync(id);
            if (division == null)
            {
                return ApiResponse.ErrorResponse("ไม่พบข้อมูล");
            }

            _context.Divisions.Remove(division);
            await _context.SaveChangesAsync();
            return ApiResponse.SuccessResponse(null, "ลบข้อมูลสำเร็จ");
        }
    }
}
