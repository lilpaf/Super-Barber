using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Models.Service;
using SuperBarber.Services.CutomException;

namespace SuperBarber.Services.Service
{
    public class ServiceService : IServiceService
    {
        private readonly SuperBarberDbContext data;

        public ServiceService(SuperBarberDbContext data)
        => this.data = data;

        public async Task<IEnumerable<ServiceCategoryViewModel>> GetServiceCategoriesAsync()
            => await this.data
            .Categories
            .Select(c => new ServiceCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name
            })
            .OrderBy(c => c.Name)
            .ToListAsync();

        public async Task AddServiceAsync(AddServiceFormModel model)
        {
            if (!this.data.Categories.Any(c => c.Id == model.CategoryId))
            {
                throw new ModelStateCustomException(nameof(model.CategoryId), "Category does not exist");
            }

            if (this.data.Services.Any(s => s.Name.ToLower().Trim() == model.Name.ToLower().Trim() && s.CategoryId == model.CategoryId))
            {
                throw new ModelStateCustomException(nameof(model.Name), "This service already exists");
            }

            var service = new Data.Models.Service
            {
                Name = model.Name,
                Price = model.Price,
                CategoryId = model.CategoryId
            };

            await this.data.Services.AddAsync(service);

            await this.data.SaveChangesAsync();
        }
    }
}
