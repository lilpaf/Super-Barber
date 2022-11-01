using SuperBarber.Models.BarberShop;
using SuperBarber.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBarber.Data.Models;
using SuperBarber.Services.CutomException;

namespace SuperBarber.Services.BarberShops
{
    public class BarberShopService : IBarberShopService
    {
        private readonly SuperBarberDbContext data;
        
        public BarberShopService(SuperBarberDbContext data)
            => this.data = data;

        public async Task<AllBarberShopQueryModel> AllBarberShopsAsync([FromQuery] AllBarberShopQueryModel query, List<BarberShopListingViewModel> barberShops = null)
        {
            var barberShopQuery = this.data.BarberShops.AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                barberShopQuery = barberShopQuery
                .Where(b => b.Name
                    .ToLower()
                .Trim()
                    .Contains(query.SearchTerm.ToLower().Trim()));
            }

            if (!string.IsNullOrEmpty(query.City))
            {
                barberShopQuery = barberShopQuery
                    .Where(b => b.City.Name == query.City);
            }

            barberShopQuery = query.Sorting switch
            {
                BarberShopSorting.City => barberShopQuery.OrderBy(b => b.City.Name),
                BarberShopSorting.Name or _ => barberShopQuery.OrderBy(b => b.Name)
            };

            barberShops ??= await barberShopQuery
                     .Select(b => new BarberShopListingViewModel
                     {
                         Id = b.Id,
                         Name = b.Name,
                         City = b.City.Name,
                         District = b.District.Name,
                         Street = b.Street,
                         StartHour = b.StartHour.ToString(@"hh\:mm"),
                         FinishHour = b.FinishHour.ToString(@"hh\:mm"),
                         ImageUrl = b.ImageUrl
                     })
                     .ToListAsync();

            var cities = await GetCitiesAsync();

            return new AllBarberShopQueryModel
            {
                BarberShops = barberShops,
                SearchTerm = query.SearchTerm,
                Cities = cities,
                Sorting = query.Sorting,
                //Districts = districts
            };
        }

        private async Task<IEnumerable<string>> GetCitiesAsync()
            => await this.data.Cities
                .Select(c => c.Name)
                .OrderBy(c => c)
                .ToListAsync();


        public async Task AddBarberShopAsync(AddBarberShopFormModel model)
        {
            if (!this.data.Cities
                .Any(c => c.Name.ToLower().Trim() == model.City.ToLower().Trim()))
            {
                await this.data.Cities.AddAsync(new City { Name = model.City });
                await this.data.SaveChangesAsync();
            }

            if (!this.data.Districts
                .Any(d => d.Name.ToLower().Trim() == model.District.ToLower().Trim()))
            {
                await this.data.Districts.AddAsync(new District { Name = model.District });
                await this.data.SaveChangesAsync();
            }

            var startHourArr = model.StartHour.Split(':');
            var finishHourArr = model.FinishHour.Split(':');

            var startHour = new TimeSpan(int.Parse(startHourArr[0]), int.Parse(startHourArr[1]), 0);

            var finishHour = new TimeSpan(int.Parse(finishHourArr[0]), int.Parse(finishHourArr[1]), 0);

            if (startHour >= finishHour)
            {
                throw new ModelStateCustomException(nameof(model.StartHour), "Opening hour must be smaller that the closing hour. And can not be the same as the closing hour");
            }

            var barberShop = new BarberShop
            {
                Name = model.Name,
                CityId = this.data.Cities
                .First(c => c.Name.ToLower().Trim() == model.City.ToLower().Trim()).Id,
                DistrictId = this.data.Districts
                .First(d => d.Name.ToLower().Trim() == model.District.ToLower().Trim()).Id,
                Street = model.Street,
                StartHour = startHour,
                FinishHour = finishHour,
                ImageUrl = model.ImageUrl
            };

            if (this.data.BarberShops
                .Any(b => b.Name.ToLower().Trim() == barberShop.Name.ToLower().Trim()
                && b.CityId == barberShop.CityId
                && b.DistrictId == barberShop.DistrictId
                 && b.Street == barberShop.Street))
            {
                throw new ModelStateCustomException(nameof(model.Name), "This Barbershop already exist");
            }

            await this.data.BarberShops.AddAsync(barberShop);
            await this.data.SaveChangesAsync();
        }
    }
}
