﻿// FileName: Portal.Services/Models/CompanyService.cs
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
    public class CompanyService : ICompanyService
    {
        private readonly PortalDbContext _context;

        public CompanyService(PortalDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CompanyViewModel>> GetAllAsync()
        {
            return await _context.Companies
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
            return await _context.Companies
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
                        TotalDepartment = d.Departments.Count()
                    }).ToList()
                }).FirstOrDefaultAsync() ?? new CompanyViewModel();
        }

        public async Task<IEnumerable<CompanyBranchViewModel>> GetBranchesByCompanyIdAsync(int companyId)
        {
            return await _context.CompanyBranches
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
            return await _context.Divisions
                .Where(d => d.CompanyId == companyId)
                .Select(d => new DivisionViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    TotalDepartment = d.Departments.Count()
                }).ToListAsync();
        }

        public async Task<ApiResponse<Company>> CreateAsync(CompanyViewModel viewModel)
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

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return new ApiResponse<Company> { Success = true, Data = company };
        }

        public async Task<ApiResponse<Company>> UpdateAsync(int id, CompanyViewModel viewModel)
        {
            var company = await _context.Companies.Include(c => c.Branches).FirstOrDefaultAsync(c => c.Id == id);
            if (company == null)
            {
                return new ApiResponse<Company> { Success = false, Message = "Company not found." };
            }

            company.Name = viewModel.Name;
            company.ShortName = viewModel.ShortName;

            company.Branches.Clear();
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

            await _context.SaveChangesAsync();
            return new ApiResponse<Company> { Success = true, Data = company };
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return new ApiResponse { Success = false, Message = "Company not found." };
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return new ApiResponse { Success = true };
        }
    }
}