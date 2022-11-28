using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Infrastructure;
using SuperBarber.Models.BarberShop;

namespace SuperBarber.Services.Home
{
    public class HomeService : IHomeService
    {
        private readonly SuperBarberDbContext data;

        public HomeService(SuperBarberDbContext data)
            => this.data = data;

        public async Task<IEnumerable<BarberShopListingViewModel>> SearchAvalibleBarbershopAsync(string city, string district, string date, string time, string userId)
        {
            var barberShopQuery = this.data.BarberShops
                .Include(bs => bs.Barbers)
                .ThenInclude(b => b.Barber)
                .AsQueryable();

            var dateParsed = DateTime.Parse(date);

            var timeArr = time.Split(':');

            var ts = new TimeSpan(int.Parse(timeArr[0]), int.Parse(timeArr[1]), 0);

            dateParsed = dateParsed.Date + ts;

            if (district == "All")
            {
                barberShopQuery = barberShopQuery
                .Where(b => b.City.Name.ToLower().Trim() == city.ToLower().Trim() &&
                b.StartHour <= ts &&
                b.FinishHour >= ts &&
                b.Orders.All(o => o.Date != dateParsed.ToUniversalTime()) &&
                b.Barbers.Any(b => b.IsAvailable));
            }
            else
            {
                barberShopQuery = barberShopQuery
                .Where(b => b.City.Name.ToLower().Trim() == city.ToLower().Trim() &&
                b.District.Name.ToLower().Trim() == district.ToLower().Trim() &&
                b.StartHour <= dateParsed.TimeOfDay &&
                b.FinishHour >= dateParsed.TimeOfDay &&
                b.Orders.All(o => o.Date != dateParsed.ToUniversalTime()) &&
                b.Barbers.Any(b => b.IsAvailable));
            }

            var barberShopsData = await barberShopQuery
                .Select(bs => new BarberShopListingViewModel
                {
                    Id = bs.Id,
                    Name = bs.Name,
                    City = bs.City.Name,
                    District = bs.District.Name,
                    Street = bs.Street,
                    StartHour = bs.StartHour.ToString(@"hh\:mm"),
                    FinishHour = bs.FinishHour.ToString(@"hh\:mm"),
                    ImageUrl = bs.ImageUrl,
                    UserIsEmployee = bs.Barbers.Any(b => b.Barber.UserId == userId),
                    UserIsOwner = bs.Barbers.Any(b => b.IsOwner && b.Barber.UserId == userId)
                })
                .ToListAsync();

            if (barberShopsData.Count() == 0)
            {
                throw new ModelStateCustomException("", "Right now we do not have any avalible barbershops matching your criteria.");
            }

            return barberShopsData;
        }

        public async Task<IEnumerable<string>> GetCitiesAsync()
            => await this.data.Cities
                .Select(c => c.Name)
                .OrderBy(c => c)
                .ToListAsync();
        
        public async Task<IEnumerable<string>> GetDistrictsAsync()
            => await this.data.Districts
                .Select(d => d.Name)
                .OrderBy(d => d)
                .ToListAsync();
    }
}
