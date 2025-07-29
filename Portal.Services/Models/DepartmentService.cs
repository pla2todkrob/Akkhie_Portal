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
                .AsNoTracking()
                .Include(d => d.Division)
                    .ThenInclude(div => div.Company)
                .Select(d => new DepartmentViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    DivisionId = d.DivisionId,
                    DivisionName = d.Division.Name,
                    CompanyId = d.Division.CompanyId,
                    CompanyName = d.Division.Company.Name,
                    TotalSection = d.Sections.Count()
                }).ToListAsync();
        }

        public async Task<DepartmentViewModel> GetByIdAsync(int id)
        {
            return await context.Departments
                .AsNoTracking()
                .Where(d => d.Id == id)
                .Include(d => d.Division)
                .Select(d => new DepartmentViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    CompanyId = d.Division.CompanyId,
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

            if (viewModel.SectionViewModels != null)
            {
                foreach (var sectionVm in viewModel.SectionViewModels)
                {
                    if (!string.IsNullOrWhiteSpace(sectionVm.Name))
                    {
                        department.Sections.Add(new Section { Name = sectionVm.Name });
                    }
                }
            }

            context.Departments.Add(department);
            await context.SaveChangesAsync();
            return new ApiResponse<Department> { Success = true, Data = department, Message = "สร้างข้อมูลฝ่ายสำเร็จ" };
        }

        public async Task<ApiResponse<Department>> UpdateAsync(int id, DepartmentViewModel viewModel)
        {
            var department = await context.Departments.Include(d => d.Sections).FirstOrDefaultAsync(d => d.Id == id);
            if (department == null)
            {
                return new ApiResponse<Department> { Success = false, Message = "ไม่พบข้อมูลฝ่าย" };
            }

            department.Name = viewModel.Name;
            department.DivisionId = viewModel.DivisionId;

            // ใช้ Helper Method เพื่อจัดการข้อมูลลูก (Sections)
            UpdateChildCollection(
                department.Sections,
                viewModel.SectionViewModels,
                (section, vm) => { // Logic สำหรับ Update
                    section.Name = vm.Name;
                },
                vm => new Section
                { // Logic สำหรับ Add
                    Name = vm.Name
                });

            await context.SaveChangesAsync();
            return new ApiResponse<Department> { Success = true, Data = department, Message = "อัปเดตข้อมูลฝ่ายสำเร็จ" };
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var department = await context.Departments.FindAsync(id);
            if (department == null)
            {
                return new ApiResponse { Success = false, Message = "ไม่พบข้อมูลฝ่าย" };
            }

            context.Departments.Remove(department);
            await context.SaveChangesAsync();
            return new ApiResponse { Success = true, Message = "ลบข้อมูลสำเร็จ" };
        }

        // Helper Method สำหรับจัดการ Collection ลูก (Child Collection)
        private void UpdateChildCollection<TEntity, TViewModel>(
            ICollection<TEntity> dbCollection,
            ICollection<TViewModel> viewModelCollection,
            Action<TEntity, TViewModel> updateAction,
            Func<TViewModel, TEntity> addAction)
            where TEntity : class, new()
            where TViewModel : class
        {
            viewModelCollection ??= new List<TViewModel>();

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