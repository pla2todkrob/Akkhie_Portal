using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;

namespace Portal.Services.Models
{
    public class DepartmentService(PortalDbContext context) : IDepartmentService
    {
        public async Task<List<DepartmentViewModel>> AllAsync()
        {
            return await context.Departments
                .AsNoTracking()
                .Include(i => i.Division)
                .Include(i => i.Sections)
                .Select(s => SetModel(s))
                .ToListAsync();
        }

        public async Task<DepartmentViewModel> SearchAsync(int id)
        {
            return await context.Departments
                .AsNoTracking()
                .Where(w => w.Id == id)
                .Include(i => i.Division)
                .Include(i => i.Sections)
                .Select(s => SetModel(s))
                .FirstOrDefaultAsync() ?? new DepartmentViewModel();
        }

        public async Task<List<DepartmentViewModel>> SearchByDivisionAsync(int divisionId)
        {
            return await context.Departments
                .AsNoTracking()
                .Where(w => w.DivisionId == divisionId)
                .Include(i => i.Division)
                .Include(i => i.Sections)
                .Select(s => SetModel(s))
                .ToListAsync();
        }

        public async Task<DepartmentViewModel> SaveAsync(DepartmentViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                Department department;
                if (model.Id == 0)
                {
                    department = new() { Name = model.Name, DivisionId = model.DivisionId };
                    await context.Departments.AddAsync(department);
                }
                else
                {
                    department = await context.Departments
                        .FirstOrDefaultAsync(d => d.Id == model.Id)
                        ?? throw new KeyNotFoundException("ไม่พบข้อมูลฝ่ายที่จะบันทึก");
                    department.Name = model.Name;
                    department.DivisionId = model.DivisionId;
                }
                await context.SaveChangesAsync();

                var existingSections = await context.Sections
                    .Where(w => w.DepartmentId == department.Id)
                    .ToListAsync();

                var incomingSectionIds = model.SectionViewModels?
                    .Where(w => w.Id != 0)
                    .Select(s => s.Id)
                    .ToList() ?? [];

                var sectionsToRemove = existingSections
                    .Where(w => !incomingSectionIds.Contains(w.Id))
                    .ToList();

                context.Sections.RemoveRange(sectionsToRemove);

                foreach (var sectModel in model.SectionViewModels ?? [])
                {
                    if (sectModel.Id == 0)
                    {
                        context.Sections.Add(new Section
                        {
                            DepartmentId = department.Id,
                            Name = sectModel.Name
                        });
                    }
                    else
                    {
                        var existingSect = existingSections.FirstOrDefault(f => f.Id == sectModel.Id);
                        if (existingSect != null)
                        {
                            existingSect.Name = sectModel.Name;
                        }
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return await SearchAsync(department.Id) ?? throw new InvalidOperationException("ไม่พบข้อมูลฝ่ายหลังจากบันทึก");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Error saving department", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var department = await context.Departments.FindAsync(id);
            if (department == null) return false;
            context.Departments.Remove(department);
            await context.SaveChangesAsync();
            return true;
        }

        private static DepartmentViewModel SetModel(Department department)
        {
            return new DepartmentViewModel
            {
                Id = department.Id,
                Name = department.Name,
                DivisionId = department.DivisionId,
                DivisionName = department.Division.Name,
                TotalSection = department.Sections.Count,
                SectionViewModels = [.. department.Sections.Select(s => new SectionViewModel
                {
                    Id = s.Id,
                    Name = s.Name
                })]
            };
        }
    }
}
