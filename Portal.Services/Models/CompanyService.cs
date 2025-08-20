using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;

namespace Portal.Services.Models
{
    public class CompanyService(PortalDbContext context) : ICompanyService
    {
        public async Task<IEnumerable<CompanyViewModel>> GetAllAsync()
        {
            return await context.Companies
                .Select(c => new CompanyViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    ShortName = c.ShortName,
                    TotalBranch = c.Branches.Count(),
                    TotalDivision = c.Divisions.Count(),
                }).ToListAsync();
        }

        public async Task<CompanyViewModel> GetByIdAsync(int id)
        {
            return await context.Companies
                .Where(c => c.Id == id)
                .Select(c => new CompanyViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    ShortName = c.ShortName,
                    CompanyBranchViewModels = c.Branches.Select(b => new CompanyBranchViewModel
                    {
                        Id = b.Id,
                        Name = b.Name,
                        BranchCode = b.BranchCode
                    }).ToList(),
                    DivisionViewModels = c.Divisions.Select(d => new DivisionViewModel
                    {
                        Id = d.Id,
                        Name = d.Name,
                        CompanyName = d.Company.Name
                    }).ToList()
                }).FirstOrDefaultAsync() ?? new CompanyViewModel();
        }

        public async Task<IEnumerable<CompanyBranchViewModel>> GetBranchesByCompanyIdAsync(int companyId)
        {
            return await context.CompanyBranches
                .Where(b => b.CompanyId == companyId)
                .Select(b => new CompanyBranchViewModel
                {
                    Id = b.Id,
                    Name = b.Name,
                    BranchCode = b.BranchCode
                }).ToListAsync();
        }

        public async Task<IEnumerable<DivisionViewModel>> GetDivisionsByCompanyIdAsync(int companyId)
        {
            return await context.Divisions
                .Where(d => d.CompanyId == companyId)
                .Select(d => new DivisionViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    TotalDepartment = d.Departments.Count()
                }).ToListAsync();
        }

        public async Task<ApiResponse> CreateAsync(CompanyViewModel viewModel)
        {
            var company = new Company
            {
                Name = viewModel.Name,
                ShortName = viewModel.ShortName
            };

            foreach (var branchVm in viewModel.CompanyBranchViewModels)
            {
                if (!string.IsNullOrWhiteSpace(branchVm.Name))
                {
                    company.Branches.Add(new CompanyBranch
                    {
                        Name = branchVm.Name,
                        BranchCode = branchVm.BranchCode
                    });
                }
            }

            if (viewModel.DivisionViewModels != null)
            {
                foreach (var divisionVm in viewModel.DivisionViewModels)
                {
                    if (!string.IsNullOrWhiteSpace(divisionVm.Name))
                    {
                        company.Divisions.Add(new Division
                        {
                            Name = divisionVm.Name
                        });
                    }
                }
            }
            

            context.Companies.Add(company);
            await context.SaveChangesAsync();
            return new ApiResponse { Success = true, Message= "เพิ่มข้อมูลบริษัทสำเร็จ" };
        }

        public async Task<ApiResponse> UpdateAsync(int id, CompanyViewModel viewModel)
        {
            try
            {
                var companyInDb = await context.Companies
                    .Include(c => c.Branches)
                    .Include(c => c.Divisions)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (companyInDb == null)
                {
                    return new ApiResponse { Success = false, Message = "ไม่พบข้อมูลบริษัทที่ต้องการแก้ไข" };
                }

                companyInDb.Name = viewModel.Name;
                companyInDb.ShortName = viewModel.ShortName;

                UpdateChildCollection(
                    companyInDb.Branches,
                    viewModel.CompanyBranchViewModels,
                    (branch, vm) => {
                        branch.Name = vm.Name;
                        branch.BranchCode = vm.BranchCode;
                    },
                    vm => new CompanyBranch
                    {
                        Name = vm.Name,
                        BranchCode = vm.BranchCode
                    });

                UpdateChildCollection(
                    companyInDb.Divisions,
                    viewModel.DivisionViewModels,
                    (division, vm) => {
                        division.Name = vm.Name;
                    },
                    vm => new Division
                    {
                        Name = vm.Name
                    });


                await context.SaveChangesAsync();

                return new ApiResponse { Success = true, Message = "อัปเดตข้อมูลบริษัทสำเร็จ" };
            }
            catch (DbUpdateException ex)
            {
                return new ApiResponse { Success = false, Message = $"เกิดข้อผิดพลาดในการบันทึกข้อมูล: {ex.InnerException?.Message ?? ex.Message}" };
            }
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var company = await context.Companies.FindAsync(id);
            if (company == null)
            {
                return new ApiResponse { Success = false, Message = "Company not found." };
            }

            context.Companies.Remove(company);
            await context.SaveChangesAsync();
            return new ApiResponse { Success = true };
        }

        private void UpdateChildCollection<TEntity, TViewModel>(
            ICollection<TEntity> dbCollection,
            ICollection<TViewModel> viewModelCollection,
            Action<TEntity, TViewModel> updateAction,
            Func<TViewModel, TEntity> addAction)
            where TEntity : class, new()
            where TViewModel : class
        {
            viewModelCollection ??= [];

            var itemsToDelete = dbCollection
                .Where(dbItem => !viewModelCollection.Any(vmItem => (int)vmItem.GetType().GetProperty("Id").GetValue(vmItem) == (int)dbItem.GetType().GetProperty("Id").GetValue(dbItem)))
                .ToList();

            foreach (var item in itemsToDelete)
            {
                context.Remove(item);
            }

            foreach (var vmItem in viewModelCollection)
            {
                var vmId = (int)vmItem.GetType().GetProperty("Id").GetValue(vmItem);
                var dbItem = dbCollection.FirstOrDefault(db => (int)db.GetType().GetProperty("Id").GetValue(db) == vmId);

                if (dbItem != null)
                {
                    updateAction(dbItem, vmItem);
                }
                else
                {
                    var newItem = addAction(vmItem);
                    dbCollection.Add(newItem);
                }
            }
        }
    }
}