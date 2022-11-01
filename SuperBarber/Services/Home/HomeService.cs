using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Models.BarberShop;
using SuperBarber.Services.CutomException;

namespace SuperBarber.Services.Home
{
    public class HomeService : IHomeService
    {
        private readonly SuperBarberDbContext data;

        public HomeService(SuperBarberDbContext data)
            => this.data = data;

        public async Task<IEnumerable<BarberShopListingViewModel>> SearchAvalibleBarbershopAsync(string city, string district, string date, string time)
        {
            var barberShopQuery = this.data.BarberShops.AsQueryable();

            var dateParsed = DateTime.Parse(date);

            var timeArr = time.Split(':');

            var ts = new TimeSpan(int.Parse(timeArr[0]), int.Parse(timeArr[1]), 0);

            dateParsed = dateParsed.Date + ts;

            if (district == "All")
            {
                barberShopQuery = barberShopQuery
                .Where(b => b.City.Name.ToLower().Trim() == city.ToLower().Trim() &&
                b.StartHour <= dateParsed.TimeOfDay &&
                b.FinishHour >= dateParsed.TimeOfDay &&
                b.Orders.Count(o => o.Date == dateParsed) == 0);
            }
            else
            {
                barberShopQuery = barberShopQuery
                .Where(b => b.City.Name.ToLower().Trim() == city.ToLower().Trim() &&
                b.District.Name.ToLower().Trim() == district.ToLower().Trim() &&
                b.StartHour <= dateParsed.TimeOfDay &&
                b.FinishHour >= dateParsed.TimeOfDay &&
                b.Orders.Count(o => o.Date == dateParsed) == 0);
            }

            var barberShopsData = await barberShopQuery
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
