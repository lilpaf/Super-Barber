using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Infrastructure;
using SuperBarber.Models.BarberShop;
using SuperBarber.Services.BarberShops.Models;
using static SuperBarber.Services.Common.TimeCheck;

namespace SuperBarber.Services.Home
{
    public class HomeService : IHomeService
    {
        private readonly SuperBarberDbContext data;

        public HomeService(SuperBarberDbContext data)
            => this.data = data;

        public async Task<IEnumerable<BarberShopListingViewModel>> SearchAvalibleBarbershopsAsync(string city, string district, string date, string time, string userId)
        {
            var barberShopQuery = this.data.BarberShops
                .Include(bs => bs.Orders)
                .Include(bs => bs.District)
                .Include(bs => bs.City)
                .Include(bs => bs.Barbers)
                .ThenInclude(b => b.Barber)
                .Where(b => b.IsPublic && !b.IsDeleted)
                .AsQueryable();

            DateTime dateParsed;

            var dateWasParsed = DateTime.TryParse(date, out dateParsed);

            if (!dateWasParsed)
            {
                throw new ModelStateCustomException("", "Invalid date input.");
            }

            if (!CheckIfTimeIsCorrect(time))
            {
                throw new ModelStateCustomException("", "Invalid time input.");
            }

            var timeArr = time.Split(':');

            var ts = new TimeSpan(int.Parse(timeArr[0]), int.Parse(timeArr[1]), 0);

            dateParsed = dateParsed.Date + ts;

            var cityId = await GetCityIdAsync(city);

            if (cityId == 0)
            {
                throw new ModelStateCustomException("", "Invalid city.");
            }

            if (district == "All")
            {
                barberShopQuery = barberShopQuery
                .Where(bs => bs.CityId == cityId &&
                bs.StartHour <= ts &&
                bs.FinishHour >= ts &&
                bs.Orders.All(o => o.Date != dateParsed.ToUniversalTime()) &&
                bs.Barbers.Any(b => b.IsAvailable));
            }
            else
            {
                var districtId = await GetDistrictIdAsync(district);

                if (districtId == 0)
                {
                    throw new ModelStateCustomException("", "Invalid district.");
                }

                barberShopQuery = barberShopQuery
                .Where(bs => bs.CityId == cityId &&
                bs.District.Id == districtId &&
                bs.StartHour <= dateParsed.TimeOfDay &&
                bs.FinishHour >= dateParsed.TimeOfDay &&
                bs.Orders.All(o => o.Date != dateParsed.ToUniversalTime()) &&
                bs.Barbers.Any(b => b.IsAvailable));
            }
            var barberShopsData = await barberShopQuery
                .Select(bs => new BarberShopListingViewModel
                {
                    Id = bs.Id,
                    BarberShopName = bs.Name,
                    City = bs.City.Name,
                    District = bs.District.Name,
                    Street = bs.Street,
                    StartHour = bs.StartHour.ToString(@"hh\:mm"),
                    FinishHour = bs.FinishHour.ToString(@"hh\:mm"),
                    ImageUrl = bs.ImageUrl,
                    UserIsEmployee = bs.Barbers.Any(b => b.Barber.UserId == userId),
                    UserIsOwner = bs.Barbers.Any(b => b.IsOwner && b.Barber.UserId == userId),
                    OwnersInfo = bs.Barbers.Where(b => b.IsOwner)
                                .Select(b => new OwnerListingViewModel
                                {
                                    Name = b.Barber.FirstName + ' ' + b.Barber.LastName,
                                    Email = b.Barber.Email,
                                    PhoneNumber = b.Barber.PhoneNumber
                                })
                                .ToList()
                })
                .ToListAsync();

            if (barberShopsData.Count() == 0)
            {
                throw new ModelStateCustomException("", "Right now we do not have any avalible barbershops matching your criteria.");
            }

            return barberShopsData;
        }

        private async Task<int> GetCityIdAsync(string name)
           => await this.data.Cities
               .Where(c => c.Name.ToLower().Trim() == name.ToLower().Trim())
               .Select(c => c.Id)
               .FirstOrDefaultAsync();
        
        private async Task<int> GetDistrictIdAsync(string name)
           => await this.data.Districts
               .Where(d => d.Name.ToLower().Trim() == name.ToLower().Trim())
               .Select(d => d.Id)
               .FirstOrDefaultAsync();

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
