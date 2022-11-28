using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Infrastructure;
using SuperBarber.Models.Service;

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

        public async Task AddServiceAsync(AddServiceFormModel model, string userId, int barberShopId)
        {
            if (!this.data.Categories.Any(c => c.Id == model.CategoryId))
            {
                throw new ModelStateCustomException(nameof(model.CategoryId), "Category does not exist");
            }

            var barberShop = await this.data.BarberShops
                    .Include(bs => bs.Services)
                    .FirstOrDefaultAsync(bs => bs.Id == barberShopId 
                        && bs.Barbers.Any(b => b.IsOwner && b.Barber.UserId == userId));

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "You are not the owner of this barbershop");
            }

            if (this.data.Services.Any(s => s.Name.ToLower().Trim() == model.Name.ToLower().Trim() && s.CategoryId == model.CategoryId))
            {
                var service = await this.data.Services
                    .FirstAsync(s => s.Name.ToLower().Trim() == model.Name.ToLower().Trim() && s.CategoryId == model.CategoryId);

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
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            return barberShop.Services
                .Select(s => new ServiceListingViewModel
                {
                    BarberShopId = barberShop.Id,
                    ServiceId = s.Service.Id,
                    ServiceName = s.Service.Name,
                    Price = s.Service.Price,
                    Category = s.Service.Category.Name,
                    BarberShopName = barberShop.Name
                })
                .ToList()
                .OrderBy(s => s.Category);
        }

        public async Task RemoveServiceAsync(int barberShopId, int serviceId, string userId)
        {
            var barberShop = await this.data.BarberShops
               .Include(bs => bs.Services)
               .Include(bs => bs.Barbers)
               .ThenInclude(b => b.Barber)
               .FirstOrDefaultAsync(bs => bs.Id == barberShopId);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            if (!barberShop.Barbers.Any(b => b.IsOwner && b.Barber.UserId == userId))
            {
                throw new ModelStateCustomException("", $"You have to be owner of {barberShop.Name} to perform this action");
            }

            if (!barberShop.Services.Any(s => s.ServiceId == serviceId))
            {
                throw new ModelStateCustomException("", $"This service does not exist in {barberShop.Name}");
            }

            barberShop.Services
                .Remove(barberShop.Services
                .First(s => s.ServiceId == serviceId));

            await this.data.SaveChangesAsync();

            if (!await this.data.BarberShops.AnyAsync(bs => bs.Services.Any(s => s.ServiceId == serviceId)))
            {
                this.data.Services
                    .Remove(this.data.Services
                    .First(s => s.Id == serviceId));

                await this.data.SaveChangesAsync();
            }
        }
    }
}
