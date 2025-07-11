using Microsoft.EntityFrameworkCore;
using Portal.Services.Interfaces;
using Portal.Shared.Models.Entities.Support;
using Portal.Shared.Models.ViewModel.Support;

namespace Portal.Services.Models
{
    public class SupportCategoryService(PortalDbContext context) : ISupportCategoryService
    {
        public async Task<IEnumerable<SupportCategoryViewModel>> GetAllAsync()
        {
            return await context.SupportTicketCategories
                .AsNoTracking()
                .Select(c => ToViewModel(c))
                .ToListAsync();
        }

        public async Task<SupportCategoryViewModel?> GetByIdAsync(int id)
        {
            var category = await context.SupportTicketCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            return category == null ? null : ToViewModel(category);
        }

        public async Task<SupportCategoryViewModel> CreateAsync(SupportCategoryViewModel model)
        {
            var entity = new SupportTicketCategory
            {
                Name = model.Name,
                Description = model.Description,
                CategoryType = model.CategoryType
            };

            context.SupportTicketCategories.Add(entity);
            await context.SaveChangesAsync();

            model.Id = entity.Id;
            return model;
        }

        public async Task<SupportCategoryViewModel> UpdateAsync(SupportCategoryViewModel model)
        {
            var entity = await context.SupportTicketCategories.FindAsync(model.Id)
                         ?? throw new KeyNotFoundException("Category not found.");

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.CategoryType = model.CategoryType;

            await context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await context.SupportTicketCategories.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            context.SupportTicketCategories.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        private static SupportCategoryViewModel ToViewModel(SupportTicketCategory entity)
        {
            return new SupportCategoryViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                CategoryType = entity.CategoryType
            };
        }
    }
}
