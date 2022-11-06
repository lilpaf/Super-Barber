using SuperBarber.Models.BarberShop;
using SuperBarber.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBarber.Data.Models;
using SuperBarber.Services.CutomException;
using Microsoft.AspNetCore.Identity;
using static SuperBarber.CustomRoles;

namespace SuperBarber.Services.BarberShops
{
    public class BarberShopService : IBarberShopService
    {
        private readonly SuperBarberDbContext data;
        private readonly UserManager<User> userManager;

        public BarberShopService(SuperBarberDbContext data, UserManager<User> userManager)
        {
            this.data = data;
            this.userManager = userManager;
        }

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


        public async Task AddBarberShopAsync(BarberShopFormModel model, string userId)
        {
            await CityExists(model.City);

            await DistrictExists(model.District);

            var hoursParsed = ParseHours(model.StartHour, model.FinishHour);

            var startHour = hoursParsed.Item1;

            var finishHour = hoursParsed.Item2;

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

            var barber = await this.data.Barbers
                .FirstAsync(b => b.UserId == userId);
            
            var user = await this.data.Users
                .FirstAsync(u => u.Id == userId);

            barberShop.Barbers.Add(barber);

            await userManager.AddToRoleAsync(user, BarberShopOwnerRoleName);

            await this.data.BarberShops.AddAsync(barberShop);

            //barber.BarberShopId = barberShop.Id;

            await this.data.SaveChangesAsync();
        }

        public async Task<IEnumerable<BarberShopListingViewModel>> MineBarberShopsAsync(string userId)
            => await this.data.BarberShops
               .Where(bs => bs.Barbers.Any(b => b.UserId == userId))
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

        public async Task<BarberShopFormModel> DisplayBarberShopInfoAsync(int barberShopId)
        {
            var formModel = await this.data.BarberShops
                .Where(bs => bs.Id == barberShopId)
                .Select(bs => new BarberShopFormModel
                {
                    Name = bs.Name,
                    City = bs.City.Name,
                    District = bs.District.Name,
                    Street = bs.Street,
                    ImageUrl = bs.ImageUrl,
                    StartHour = bs.StartHour.ToString(@"hh\:mm"),
                    FinishHour = bs.FinishHour.ToString(@"hh\:mm"),
                })
                .FirstOrDefaultAsync();

            if (formModel == null)
            {
                throw new ModelStateCustomException("", "This Barbershop does not exist");
            }

            return formModel;
        }
        
        public async Task EditBarberShopAsync(BarberShopFormModel model, int barberShopId, string userId, bool userIsAdmin)
        {
            var barberShop = await this.data.BarberShops.FindAsync(barberShopId);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This Barbershop does not exist");
            }

            var barber = await this.data.Barbers
                .FirstAsync(b => b.UserId == userId);

            if (!barberShop.Barbers.Any(b => b.Id == barber.Id) || !userIsAdmin)
            {
                throw new ModelStateCustomException("", "You can not delete this barber shop");
            }

            await CityExists(model.City);

            barberShop.CityId = this.data.Cities
                .First(c => c.Name.ToLower().Trim() == model.City.ToLower().Trim()).Id;

            await DistrictExists(model.District);

            barberShop.DistrictId = this.data.Districts
                .First(d => d.Name.ToLower().Trim() == model.District.ToLower().Trim()).Id;

            var hoursParsed = ParseHours(model.StartHour, model.FinishHour);

            var startHour = hoursParsed.Item1;

            var finishHour = hoursParsed.Item2;

            if (startHour >= finishHour)
            {
                throw new ModelStateCustomException(nameof(model.StartHour), "Opening hour must be smaller that the closing hour. And can not be the same as the closing hour");
            }

            barberShop.StartHour = startHour;
            barberShop.FinishHour = finishHour;
            barberShop.Name = model.Name;
            barberShop.ImageUrl = model.ImageUrl;
            
            await this.data.SaveChangesAsync();
        }
        
        public async Task DeleteBarberShopAsync(int barberShopId, string userId, bool userIsAdmin)
        {
            var barberShop = await this.data.BarberShops.FindAsync(barberShopId);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barber shop does not exist");
            }
            
            var barber = await this.data.Barbers
                .FirstAsync(b => b.UserId == userId);

            if (!barberShop.Barbers.Any(b => b.Id == barber.Id) && !userIsAdmin)
            {
                throw new ModelStateCustomException("", "You can not delete this barber shop");
            }

            barber.BarberShopId = null;

            this.data.BarberShops.Remove(barberShop);

            await this.data.SaveChangesAsync();
        }

        private async Task CityExists(string city)
        {
            if (!this.data.Cities
                .Any(c => c.Name.ToLower().Trim() == city.ToLower().Trim()))
            {
                await this.data.Cities.AddAsync(new City { Name = city });
                await this.data.SaveChangesAsync();
            }
        }
        
        private async Task DistrictExists(string district)
        {
            if (!this.data.Districts
                .Any(d => d.Name.ToLower().Trim() == district.ToLower().Trim()))
            {
                await this.data.Districts.AddAsync(new District { Name = district });
                await this.data.SaveChangesAsync();
            }
        }
        
        private Tuple<TimeSpan, TimeSpan> ParseHours(string startHour, string finishHour)
        {
            var startHourArr = startHour.Split(':');
            var finishHourArr = finishHour.Split(':');

            var startHourParsed = new TimeSpan(int.Parse(startHourArr[0]), int.Parse(startHourArr[1]), 0);

            var finishHourParsed = new TimeSpan(int.Parse(finishHourArr[0]), int.Parse(finishHourArr[1]), 0);

            return Tuple.Create(startHourParsed, finishHourParsed);
        }
    }
}
