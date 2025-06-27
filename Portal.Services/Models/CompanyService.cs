using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;

namespace Portal.Services.Models
{
    public class CompanyService(PortalDbContext context) : ICompanyService
    {
        public async Task<List<CompanyViewModel>> AllAsync()
        {
            return await context.Companies
                .AsNoTracking()
                .Include(i => i.Branches)
                .Select(s => SetModel(s)).ToListAsync();
        }

        public async Task<CompanyViewModel?> SearchAsync(int id)
        {
            return await context.Companies
                .AsNoTracking()
                .Where(w => w.Id == id)
                .Include(i => i.Branches)
                .Select(s => SetModel(s))
                .FirstOrDefaultAsync();
        }

        public async Task<CompanyViewModel> SaveAsync(CompanyViewModel companyViewModel)
        {
            Company company;
            bool isNew = companyViewModel.Id == 0;
            using var transaction = await context.Database.BeginTransactionAsync();
            if (isNew)
            {
                // Create new company
                company = new Company
                {
                    Name = companyViewModel.Name,
                    ShortName = companyViewModel.ShortName
                };
                context.Companies.Add(company);
                await context.SaveChangesAsync();
            }
            else
            {
                // Update existing company
                company = await context.Companies
                    .AsNoTracking()
                    .Include(i => i.Branches)
                    .FirstOrDefaultAsync(f => f.Id == companyViewModel.Id) ?? throw new InvalidOperationException("ไม่พบบริษัทที่ต้องการบันทึก");

                company.Name = companyViewModel.Name;
                company.ShortName = companyViewModel.ShortName;
            }

            // Handle branches
            if (companyViewModel.CompanyBranchViewModels != null)
            {
                var existingBranches = company.Branches.ToList();
                var incomingBranches = companyViewModel.CompanyBranchViewModels.ToList();

                // Delete branches not in incoming list
                var branchesToDelete = existingBranches
                    .Where(eb => !incomingBranches.Any(ib => ib.Id == eb.Id))
                    .ToList();

                foreach (var branch in branchesToDelete)
                {
                    context.CompanyBranches.Remove(branch);
                }

                // Update or add branches
                foreach (var branchVm in incomingBranches)
                {
                    if (branchVm.Id == 0)
                    {
                        // Add new branch
                        var newBranch = new CompanyBranch
                        {
                            Name = branchVm.Name,
                            BranchCode = branchVm.BranchCode,
                            CompanyId = company.Id
                        };
                        context.CompanyBranches.Add(newBranch);
                    }
                    else
                    {
                        // Update existing branch
                        var existingBranch = existingBranches.FirstOrDefault(b => b.Id == branchVm.Id);
                        if (existingBranch != null)
                        {
                            existingBranch.Name = branchVm.Name;
                            existingBranch.BranchCode = branchVm.BranchCode;
                        }
                    }
                }
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return await SearchAsync(company.Id) ?? throw new InvalidOperationException("ไม่พบข้อมูลบริษัทหลังจากบันทึก");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var company = await context.Companies.FindAsync(id);
            if (company == null) return false;
            context.Companies.Remove(company);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CompanyBranchViewModel>> SearchBranchesByCompany(int id)
        {
            return await context.CompanyBranches
                .Where(w => w.CompanyId == id)
                .Select(s => new CompanyBranchViewModel()
                {
                    Id = s.Id,
                    Name = s.Name,
                    BranchCode = s.BranchCode,
                    CompanyId = s.CompanyId
                })
                .ToListAsync();
        }

        private static CompanyViewModel SetModel(Company company)
        {
            return new CompanyViewModel()
            {
                Id = company.Id,
                Name = company.Name,
                ShortName = company.ShortName,
                TotalBranch = company.Branches.Count,
                CompanyBranchViewModels = [.. company.Branches.Select(s2 => new CompanyBranchViewModel()
                {
                    Id = s2.Id,
                    Name = s2.Name,
                    BranchCode = s2.BranchCode,
                    CompanyId = s2.CompanyId
                })]
            };
        }
    }
}
