using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.ViewModel;

namespace Portal.Services.Models
{
    public class SectionService(PortalDbContext context) : ISectionService
    {
        public async Task<List<SectionViewModel>> AllAsync()
        {
            return await context.Sections
                .AsNoTracking()
                .Include(i => i.Department)
                .ThenInclude(ti => ti.Division)
                .Select(s => SetModel(s))
                .ToListAsync();
        }

        public async Task<SectionViewModel> SearchAsync(int id)
        {
            return await context.Sections
                .AsNoTracking()
                .Where(w => w.Id == id)
                .Include(i => i.Department)
                .ThenInclude(ti => ti.Division)
                .Select(s => SetModel(s))
                .FirstOrDefaultAsync() ?? new SectionViewModel();
        }

        public async Task<List<SectionViewModel>> SearchByDepartmentAsync(int id)
        {
            return await context.Sections
                .AsNoTracking()
                .Include(i => i.Department)
                .ThenInclude(ti => ti.Division)
                .Where(w => w.DepartmentId == id)
                .Select(s => SetModel(s))
                .ToListAsync();
        }

        public async Task<SectionViewModel> SaveAsync(SectionViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                Section section;
                if (model.Id == 0)
                {
                    section = new() { Name = model.Name, DepartmentId = model.DepartmentId };
                    await context.Sections.AddAsync(section);
                }
                else
                {
                    section = await context.Sections
                        .FirstOrDefaultAsync(f => f.Id == model.Id)
                        ?? throw new KeyNotFoundException("ไม่พบแผนกที่ต้องการบันทึก");
                    section.Name = model.Name;
                    section.DepartmentId = model.DepartmentId;
                }
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return await SearchAsync(section.Id) ?? throw new InvalidOperationException("ไม่พบข้อมูลแผนกหลังจากบันทึก");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var section = await context.Sections
                .FirstOrDefaultAsync(f => f.Id == id)
                ?? throw new KeyNotFoundException("Section not found for deletion");
            context.Sections.Remove(section);
            return await context.SaveChangesAsync() > 0;
        }

        private static SectionViewModel SetModel(Section section)
        {
            return new SectionViewModel
            {
                Id = section.Id,
                Name = section.Name,
                DepartmentId = section.DepartmentId,
                DepartmentName = section.Department.Name,
                DivisionId = section.Department.DivisionId,
                DivisionName = section.Department.Division.Name
            };
        }
    }
}
