using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Data.Models;
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

        public async Task AddServiceAsync(AddServiceFormModel model, string userId)
        {
            if (!this.data.Categories.Any(c => c.Id == model.CategoryId))
            {
                throw new ModelStateCustomException(nameof(model.CategoryId), "Category does not exist");
            }

            if (this.data.Services.Any(s => s.Name.ToLower().Trim() == model.Name.ToLower().Trim() && s.CategoryId == model.CategoryId))
            {
                var service = await this.data.Services
                    .FirstAsync(s => s.Name.ToLower().Trim() == model.Name.ToLower().Trim() && s.CategoryId == model.CategoryId);

                var barberShop = await this.data.BarberShops
                    .FirstAsync(bs => bs.Barbers.Any(b => b.UserId == userId));

                if (barberShop.Services.Any(s => s.ServiceId == service.Id))
                {
                    throw new ModelStateCustomException(nameof(model.Name), "This service already exists in your barber shop");
                }

                barberShop.Services.Add(new BarberShopServices
                {
                    BarberShop = barberShop,
                    Service = service
                });

                await this.data.SaveChangesAsync();
            }
            else
            {
                var service = new Data.Models.Service
                {
                    Name = model.Name,
                    Price = model.Price,
                    CategoryId = model.CategoryId
                };

                await this.data.Services.AddAsync(service);

                var barberShop = await this.data.BarberShops
                    .FirstAsync(bs => bs.Barbers.Any(b => b.UserId == userId));

                barberShop.Services.Add(new BarberShopServices
                {
                    BarberShop = barberShop,
                    Service = service
                });

                await this.data.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ServiceListingViewModel>> ShowServicesAsync(int barberShopId)
        {
            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Services)
                .ThenInclude(s => s.Service)
                .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barber shop does not exist");
            }

            return barberShop.Services
                .Select(s => new ServiceListingViewModel
                {
                    BarberShopId = barberShop.Id,
                    ServiceId = s.Service.Id,
                    Name = s.Service.Name,
                    Price = s.Service.Price,
                    Category = s.Service.Category.Name
                })
                .ToList()
                .OrderBy(s => s.Category);
        }
    }
}
