// FileName: Portal.Services/Models/CompanyService.cs
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
                // 1. ดึงข้อมูลบริษัทที่ต้องการแก้ไขจากฐานข้อมูล
                //    พร้อมทั้งดึงข้อมูลลูก (Branches และ Divisions) มาด้วยทั้งหมด
                var companyInDb = await context.Companies
                    .Include(c => c.Branches)
                    .Include(c => c.Divisions)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (companyInDb == null)
                {
                    return new ApiResponse { Success = false, Message = "ไม่พบข้อมูลบริษัทที่ต้องการแก้ไข" };
                }

                // 2. อัปเดตข้อมูลหลักของบริษัท (Scalar Properties)
                companyInDb.Name = viewModel.Name;
                companyInDb.ShortName = viewModel.ShortName;

                // 3. เรียกใช้ Helper Method เพื่อจัดการข้อมูลลูก (Branches และ Divisions)
                //    เมธอดนี้จะทำการเปรียบเทียบข้อมูลเก่าและใหม่ เพื่อ เพิ่ม/ลบ/แก้ไข ได้อย่างถูกต้อง
                UpdateChildCollection(
                    companyInDb.Branches,
                    viewModel.CompanyBranchViewModels,
                    (branch, vm) => { // Logic สำหรับ Update
                        branch.Name = vm.Name;
                        branch.BranchCode = vm.BranchCode;
                    },
                    vm => new CompanyBranch
                    { // Logic สำหรับ Add
                        Name = vm.Name,
                        BranchCode = vm.BranchCode
                    });

                UpdateChildCollection(
                    companyInDb.Divisions,
                    viewModel.DivisionViewModels,
                    (division, vm) => { // Logic สำหรับ Update
                        division.Name = vm.Name;
                    },
                    vm => new Division
                    { // Logic สำหรับ Add
                        Name = vm.Name
                    });


                // 4. บันทึกการเปลี่ยนแปลงทั้งหมดลงฐานข้อมูล
                await context.SaveChangesAsync();

                return new ApiResponse { Success = true, Message = "อัปเดตข้อมูลบริษัทสำเร็จ" };
            }
            catch (DbUpdateException ex)
            {
                // จัดการ Error ที่อาจเกิดขึ้นจากฐานข้อมูล
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

        /// <summary>
        /// Helper Method สำหรับจัดการ Collection ลูก (Child Collection)
        /// ทำหน้าที่เปรียบเทียบข้อมูลจาก DB และ ViewModel เพื่อทำการ เพิ่ม/ลบ/แก้ไข ข้อมูลลูก
        /// </summary>
        /// <typeparam name="TEntity">Type ของ Entity ในฐานข้อมูล (เช่น CompanyBranch)</typeparam>
        /// <typeparam name="TViewModel">Type ของ ViewModel ที่ส่งมาจาก Frontend (เช่น CompanyBranchViewModel)</typeparam>
        /// <param name="dbCollection">Collection ของ Entity ที่ดึงมาจากฐานข้อมูล</param>
        /// <param name="viewModelCollection">Collection ของ ViewModel ที่ส่งมาจาก Frontend</param>
        /// <param name="updateAction">Action ที่จะทำเมื่อพบว่ามี Entity ที่ต้อง 'อัปเดต'</param>
        /// <param name="addAction">Function ที่จะทำงานเพื่อสร้าง Entity ใหม่เมื่อต้อง 'เพิ่ม'</param>
        private void UpdateChildCollection<TEntity, TViewModel>(
            ICollection<TEntity> dbCollection,
            ICollection<TViewModel> viewModelCollection,
            Action<TEntity, TViewModel> updateAction,
            Func<TViewModel, TEntity> addAction)
            where TEntity : class, new()
            where TViewModel : class
        {
            // ถ้า ViewModel ที่ส่งมาเป็น null ให้ถือว่าไม่มีข้อมูล
            viewModelCollection ??= [];

            // --- ขั้นตอนที่ 1: ลบ (Delete) ---
            // ค้นหารายการที่อยู่ใน DB แต่ 'ไม่มี' อยู่ใน ViewModel ที่ส่งมา แล้วสั่งลบ
            var itemsToDelete = dbCollection
                .Where(dbItem => !viewModelCollection.Any(vmItem => (int)vmItem.GetType().GetProperty("Id").GetValue(vmItem) == (int)dbItem.GetType().GetProperty("Id").GetValue(dbItem)))
                .ToList();

            foreach (var item in itemsToDelete)
            {
                context.Remove(item);
            }

            // --- ขั้นตอนที่ 2: อัปเดตและเพิ่ม (Update & Add) ---
            foreach (var vmItem in viewModelCollection)
            {
                var vmId = (int)vmItem.GetType().GetProperty("Id").GetValue(vmItem);
                var dbItem = dbCollection.FirstOrDefault(db => (int)db.GetType().GetProperty("Id").GetValue(db) == vmId);

                if (dbItem != null)
                {
                    // 'อัปเดต' รายการที่มีอยู่แล้ว
                    updateAction(dbItem, vmItem);
                }
                else
                {
                    // 'เพิ่ม' รายการใหม่ (กรณีที่ Id เป็น 0 หรือไม่มีใน DB)
                    var newItem = addAction(vmItem);
                    dbCollection.Add(newItem);
                }
            }
        }
    }
}