using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;
using System.Data;

namespace Portal.Services.Models
{
    public class DivisionService(PortalDbContext context) : IDivisionService
    {
        public async Task<List<DivisionViewModel>> AllAsync()
        {
            return await context.Divisions
                .AsNoTracking()
                .Include(i => i.Departments)
                .Select(s => SetModel(s)).ToListAsync();
        }

        public async Task<DivisionViewModel> SearchAsync(int id)
        {
            return await context.Divisions
                .AsNoTracking()
                .Where(w => w.Id == id)
                .Include(i => i.Departments)
                .Select(s => SetModel(s)).FirstOrDefaultAsync() ?? new DivisionViewModel();
        }

        public async Task<DivisionViewModel> SaveAsync(DivisionViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                Division division;
                if (model.Id == 0)
                {
                    division = new() { Name = model.Name };
                    await context.Divisions.AddAsync(division);
                }
                else
                {
                    division = await context.Divisions
                        .FirstOrDefaultAsync(d => d.Id == model.Id)
                        ?? throw new DataException("ไม่พบสายงานที่ต้องการบันทึก");

                    division.Name = model.Name;
                }

                await context.SaveChangesAsync();

                var existingDepartments = await context.Departments
                    .Where(w => w.DivisionId == division.Id)
                    .ToListAsync();

                var incomingDepartmentIds = model.DepartmentViewModels?
                    .Where(w => w.Id != 0)
                    .Select(s => s.Id)
                    .ToList() ?? [];

                var departmentsToRemove = existingDepartments
                    .Where(w => !incomingDepartmentIds.Contains(w.Id))
                    .ToList();

                context.Departments.RemoveRange(departmentsToRemove);

                foreach (var deptModel in model.DepartmentViewModels ?? [])
                {
                    if (deptModel.Id == 0)
                    {
                        context.Departments.Add(new Department
                        {
                            DivisionId = division.Id,
                            Name = deptModel.Name
                        });
                    }
                    else
                    {
                        var existingDept = existingDepartments.FirstOrDefault(f => f.Id == deptModel.Id);
                        if (existingDept != null)
                        {
                            existingDept.Name = deptModel.Name;
                        }
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await SearchAsync(division.Id) ?? throw new DataException("ไม่พบข้อมูลสายงานหลังจากบันทึก");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error saving Division: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var division = await context.Divisions.FindAsync(id);
            if (division == null) return false;
            context.Divisions.Remove(division);
            await context.SaveChangesAsync();
            return true;
        }

        private static DivisionViewModel SetModel(Division division)
        {
            return new DivisionViewModel
            {
                Id = division.Id,
                Name = division.Name,
                TotalDepartment = division.Departments.Count,
                DepartmentViewModels = division.Departments.Select(s => new DepartmentViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    TotalSection = s.Sections.Count
                }).ToList()
            };
        }
    }
}
