// FileName: Portal.Services/Models/DivisionService.cs
using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.Services.Models
{
    public class DivisionService(PortalDbContext context) : IDivisionService
    {
        public async Task<IEnumerable<DivisionViewModel>> GetAllAsync()
        {
            return await context.Divisions
                .AsNoTracking()
                .Select(d => new DivisionViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    CompanyName = d.Company.Name,
                    TotalDepartment = d.Departments.Count()
                }).ToListAsync();
        }

        public async Task<DivisionViewModel> GetByIdAsync(int id)
        {
            return await context.Divisions
                .AsNoTracking()
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
            return await context.Departments
                .AsNoTracking()
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

        public async Task<ApiResponse> CreateAsync(DivisionViewModel viewModel)
        {
            try
            {
                var division = new Division
                {
                    Name = viewModel.Name,
                    CompanyId = viewModel.CompanyId,
                    Departments = viewModel.DepartmentViewModels?
                        .Where(d => !string.IsNullOrWhiteSpace(d.Name))
                        .Select(d => new Department { Name = d.Name })
                        .ToList() ?? []
                };

                context.Divisions.Add(division);
                await context.SaveChangesAsync();
                return new ApiResponse { Success = true, Message = "สร้างข้อมูลสายงานสำเร็จ" };
            }
            catch (Exception ex)
            {
                return new ApiResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse> UpdateAsync(int id, DivisionViewModel viewModel)
        {
            try
            {
                var divisionInDb = await context.Divisions
                    .Include(d => d.Departments)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (divisionInDb == null)
                {
                    return new ApiResponse { Success = false, Message = "ไม่พบข้อมูลสายงานที่ต้องการแก้ไข" };
                }

                // อัปเดตข้อมูลหลัก
                divisionInDb.Name = viewModel.Name;
                divisionInDb.CompanyId = viewModel.CompanyId;

                // เรียกใช้ Helper Method เพื่อจัดการ Departments (เพิ่ม/ลบ/แก้ไข)
                UpdateChildCollection(
                    divisionInDb.Departments,
                    viewModel.DepartmentViewModels,
                    (department, vm) => { // Logic สำหรับ Update
                        department.Name = vm.Name;
                    },
                    vm => new Department
                    { // Logic สำหรับ Add
                        Name = vm.Name
                    });

                await context.SaveChangesAsync();
                return new ApiResponse { Success = true, Message = "อัปเดตข้อมูลสายงานสำเร็จ" };
            }
            catch (DbUpdateException ex)
            {
                return new ApiResponse { Success = false, Message = $"เกิดข้อผิดพลาดในการบันทึกข้อมูล: {ex.InnerException?.Message ?? ex.Message}" };
            }
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var division = await context.Divisions.FindAsync(id);
            if (division == null)
            {
                return new ApiResponse { Success = false, Message = "ไม่พบข้อมูลสายงาน" };
            }

            context.Divisions.Remove(division);
            await context.SaveChangesAsync();
            return new ApiResponse { Success = true, Message = "ลบข้อมูลสำเร็จ" };
        }

        /// <summary>
        /// Helper Method สำหรับจัดการ Collection ลูก (Child Collection)
        /// </summary>
        private void UpdateChildCollection<TEntity, TViewModel>(
            ICollection<TEntity> dbCollection,
            ICollection<TViewModel> viewModelCollection,
            Action<TEntity, TViewModel> updateAction,
            Func<TViewModel, TEntity> addAction)
            where TEntity : class, new()
            where TViewModel : class
        {
            viewModelCollection ??= [];

            // ลบรายการที่ไม่มีอยู่ใน ViewModel
            var itemsToDelete = dbCollection
                .Where(dbItem => !viewModelCollection.Any(vmItem => (int)vmItem.GetType().GetProperty("Id").GetValue(vmItem) == (int)dbItem.GetType().GetProperty("Id").GetValue(dbItem)))
                .ToList();

            foreach (var item in itemsToDelete)
            {
                context.Remove(item);
            }

            // อัปเดตและเพิ่มรายการใหม่
            foreach (var vmItem in viewModelCollection)
            {
                var vmId = (int)vmItem.GetType().GetProperty("Id").GetValue(vmItem);
                var dbItem = dbCollection.FirstOrDefault(db => (int)db.GetType().GetProperty("Id").GetValue(db) == vmId);

                if (dbItem != null)
                {
                    // อัปเดต
                    updateAction(dbItem, vmItem);
                }
                else
                {
                    // เพิ่ม
                    var newItem = addAction(vmItem);
                    dbCollection.Add(newItem);
                }
            }
        }
    }
}
