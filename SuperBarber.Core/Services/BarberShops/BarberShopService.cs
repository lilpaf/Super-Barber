using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SuperBarber.Infrastructure.Data.Models;
using SuperBarber.Infrastructure.Data;
using SuperBarber.Core.Models.BarberShop;
using SuperBarber.Core.Extensions;
using SuperBarber.Core.Models.Barber;
using static SuperBarber.Core.Services.Common.TimeCheck;
using static SuperBarber.Core.Extensions.CustomRoles;

namespace SuperBarber.Core.Services.BarberShops
{
    public class BarberShopService : IBarberShopService
    {
        private readonly SuperBarberDbContext data;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public BarberShopService(SuperBarberDbContext data,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            this.data = data;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        /// <summary>
        /// Gets all the barbershops depening on if you want the public ones or the private ones.
        /// Filters the barbershops if the user chose any filters and makes paging of the shops.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="userId"></param>
        /// <param name="barberShops"></param>
        /// <param name="publicOnly"></param>
        /// <returns></returns>
        /// <exception cref="ModelStateCustomException"></exception>
        public async Task<AllBarberShopQueryModel> AllBarberShopsAsync(AllBarberShopQueryModel query, string userId, List<BarberShopListingViewModel>? barberShops = null, bool publicOnly = true)
        {
            var barberShopQuery = this.data.BarberShops
                .Include(bs => bs.Barbers)
                .ThenInclude(b => b.Barber)
                .Where(bs => bs.IsPublic == publicOnly && !bs.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                barberShopQuery = barberShopQuery
                .Where(b => b.Name.ToLower().Replace(" ", "")
                    .Contains(query.SearchTerm.ToLower().Replace(" ", "")));
            }

            if (!string.IsNullOrEmpty(query.City))
            {
                var cityId = await GetCityIdAsync(query.City);

                if (cityId == 0)
                {
                    throw new ModelStateCustomException("", "Invalid city.");
                }

                barberShopQuery = barberShopQuery
                    .Where(b => b.CityId == cityId);
            }

            if (!string.IsNullOrEmpty(query.District))
            {
                var districtId = await GetDistrictIdAsync(query.District);

                if (districtId == 0)
                {
                    throw new ModelStateCustomException("", "Invalid district.");
                }

                barberShopQuery = barberShopQuery
                    .Where(b => b.DistrictId == districtId);
            }

            barberShopQuery = query.Sorting switch
            {
                BarberShopSorting.City => barberShopQuery.OrderBy(b => b.City.Name),
                BarberShopSorting.District => barberShopQuery.OrderBy(b => b.District.Name),
                BarberShopSorting.Name or _ => barberShopQuery.OrderBy(b => b.Name)
            };

            barberShops ??= await barberShopQuery
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

            var totalBarberShops = barberShops.Count;

            var barberShopsPaged = barberShops
                    .Skip((query.CurrentPage - 1) * AllBarberShopQueryModel.BarberShopsPerPage)
                    .Take(AllBarberShopQueryModel.BarberShopsPerPage);

            var cities = await GetCitiesAsync();

            var districts = await GetDistrictsAsync();

            return new AllBarberShopQueryModel
            {
                BarberShops = barberShopsPaged,
                SearchTerm = query.SearchTerm,
                Cities = cities,
                City = query.City,
                Sorting = query.Sorting,
                TotalBarberShops = totalBarberShops,
                CurrentPage = query.CurrentPage,
                Districts = districts,
                District = query.District
            };
        }

        private async Task<int> GetCityIdAsync(string name)
           => await this.data.Cities
               .Where(c => c.Name.ToLower().Replace(" ", "") == name.ToLower().Replace(" ", ""))
               .Select(c => c.Id)
               .FirstOrDefaultAsync();

        private async Task<int> GetDistrictIdAsync(string name)
           => await this.data.Districts
               .Where(d => d.Name.ToLower().Replace(" ", "") == name.ToLower().Replace(" ", ""))
               .Select(d => d.Id)
               .FirstOrDefaultAsync();

        private async Task<IEnumerable<string>> GetCitiesAsync()
            => await this.data.Cities
                .Select(c => c.Name)
                .OrderBy(c => c)
                .ToListAsync();

        private async Task<IEnumerable<string>> GetDistrictsAsync()
            => await this.data.Districts
                .Select(d => d.Name)
                .OrderBy(d => d)
                .ToListAsync();

        /// <summary>
        /// This method allows barbers to add barbershops and checks if the barbershop to add was deleted.
        /// If it was deleted then it will update it and it will not create a new barbershop in the DB.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ModelStateCustomException"></exception>
        public async Task AddBarberShopAsync(BarberShopFormModel model, string userId)
        {
            if (!CheckIfTimeIsCorrect(model.StartHour))
            {
                throw new ModelStateCustomException("", "Start hour input is incorrect.");
            }
            if (!CheckIfTimeIsCorrect(model.FinishHour))
            {
                throw new ModelStateCustomException("", "Finish hour input is incorrect.");
            }

            var hoursParsed = ParseHours(model.StartHour, model.FinishHour);

            var startHour = hoursParsed.Item1;

            var finishHour = hoursParsed.Item2;

            if (startHour >= finishHour)
            {
                throw new ModelStateCustomException(nameof(model.StartHour), "Opening hour must be smaller that the closing hour. And can not be the same as the closing hour.");
            }

            await CityExists(model.City);

            await DistrictExists(model.District);

            var barberShop = new BarberShop
            {
                Name = model.Name,
                CityId = this.data.Cities
                .First(c => c.Name.ToLower().Replace(" ", "") == model.City.ToLower().Replace(" ", "")).Id,
                DistrictId = this.data.Districts
                .First(d => d.Name.ToLower().Replace(" ", "") == model.District.ToLower().Replace(" ", "")).Id,
                Street = model.Street,
                StartHour = startHour,
                FinishHour = finishHour,
                ImageUrl = model.ImageUrl,
                IsPublic = false,
                IsDeleted = false,
                DeleteDate = null
            };

            var existingBarberShop = await this.data.BarberShops.FirstOrDefaultAsync
                (b => b.Name.ToLower().Replace(" ", "") == barberShop.Name.ToLower().Replace(" ", "")
                && b.CityId == barberShop.CityId
                && b.DistrictId == barberShop.DistrictId
                && !b.IsDeleted);

            if (existingBarberShop != null
                && GetStreetNameAndNumberCaseInsensitive(existingBarberShop.Street) == GetStreetNameAndNumberCaseInsensitive(barberShop.Street))
            {
                throw new ModelStateCustomException("", "This barbershop already exist.");
            }


            var user = await this.data.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user == null)
            {
                throw new ModelStateCustomException("", "User does not exist.");
            }

            var barber = await data.Barbers
                .FirstOrDefaultAsync(b => b.UserId == userId && !b.IsDeleted);

            if (barber == null)
            {
                throw new ModelStateCustomException("", "You need to be a barber in order to do this action.");
            }

            barberShop.Barbers.Add(new BarberShopBarbers
            {
                BarberShop = barberShop,
                Barber = barber,
                IsOwner = true
            });

            await userManager.AddToRoleAsync(user, BarberShopOwnerRoleName);

            var deletedBarbershop = await this.data.BarberShops
                .FirstOrDefaultAsync(b => b.Name.ToLower().Replace(" ", "") == barberShop.Name.ToLower().Replace(" ", "")
                    && b.CityId == barberShop.CityId
                    && b.DistrictId == barberShop.DistrictId
                    && b.IsDeleted);

            if (deletedBarbershop != null 
                && GetStreetNameAndNumberCaseInsensitive(deletedBarbershop.Street) == GetStreetNameAndNumberCaseInsensitive(barberShop.Street))
            {
                deletedBarbershop.Name = barberShop.Name;
                deletedBarbershop.IsDeleted = false;
                deletedBarbershop.DeleteDate = null;
                deletedBarbershop.Barbers = barberShop.Barbers;
            }
            else
            {
                await this.data.BarberShops.AddAsync(barberShop);
            }

            await this.data.SaveChangesAsync();

            await signInManager.RefreshSignInAsync(user);
        }

        private static string GetStreetNameAndNumberCaseInsensitive(string streetNameAndNumber)
            => streetNameAndNumber.ToLower().Replace("st", "").Replace("ul", "").Replace(".", "").Replace(" ", "");
  
        /// <summary>
        /// This method gets all the barbershops that the user barber is employee or is a owner of.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MineBarberShopViewModel> MineBarberShopsAsync(string userId, int currentPage)
        {
            var userBarbershopsOwned = await this.data.BarberShops
           .Where(bs => bs.Barbers.Any(b => b.Barber.UserId == userId) && !bs.IsDeleted)
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
               UserIsEmployee = true,
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

            var totalUserBarbershopsOwned = userBarbershopsOwned.Count;

            var userBarbershopsOwnedPaged = userBarbershopsOwned
                    .Skip((currentPage - 1) * AllBarberShopQueryModel.BarberShopsPerPage)
                    .Take(AllBarberShopQueryModel.BarberShopsPerPage);

            return new MineBarberShopViewModel
            {
                BarberShops = userBarbershopsOwnedPaged,
                CurrentPage = currentPage,
                TotalBarberShops = totalUserBarbershopsOwned
            };
        }

        /// <summary>
        /// Gets the barbershop information to be displayed on GET Edit action.
        /// </summary>
        /// <param name="barberShopId"></param>
        /// <returns></returns>
        /// <exception cref="ModelStateCustomException"></exception>
        public async Task<BarberShopFormModel> DisplayBarberShopInfoAsync(int barberShopId)
        {
            var formModel = await this.data.BarberShops
                .Where(bs => bs.Id == barberShopId && !bs.IsDeleted)
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
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            return formModel;
        }

        /// <summary>
        /// This method allows the owners of the barbershop or the admin to edit it.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="barberShopId"></param>
        /// <param name="userId"></param>
        /// <param name="userIsAdmin"></param>
        /// <returns></returns>
        /// <exception cref="ModelStateCustomException"></exception>
        public async Task EditBarberShopAsync(BarberShopFormModel model, int barberShopId, string userId, bool userIsAdmin)
        {
            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId && !bs.IsDeleted);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            if (!userIsAdmin)
            {
                var barber = await this.data.Barbers
                .FirstOrDefaultAsync(b => b.UserId == userId && !b.IsDeleted &&
                        b.BarberShops.Any(bs => bs.BarberShopId == barberShop.Id && bs.IsOwner));

                if (barber == null)
                {
                    throw new ModelStateCustomException("", $"You have to be a owner of ${barberShop.Name} to do this action");
                }
            }

            if (!CheckIfTimeIsCorrect(model.StartHour))
            {
                throw new ModelStateCustomException("", "Start hour input is incorect.");
            }
            if (!CheckIfTimeIsCorrect(model.FinishHour))
            {
                throw new ModelStateCustomException("", "Finish hour input is incorect.");
            }

            var hoursParsed = ParseHours(model.StartHour, model.FinishHour);

            var startHour = hoursParsed.Item1;

            var finishHour = hoursParsed.Item2;

            if (startHour >= finishHour)
            {
                throw new ModelStateCustomException(nameof(model.StartHour), "Opening hour must be smaller that the closing hour. And can not be the same as the closing hour");
            }

            await CityExists(model.City);

            var oldCityId = barberShop.CityId;

            bool cityChanged = false;

            if (barberShop.City.Name != model.City)
            {
                barberShop.CityId = this.data.Cities
                .First(c => c.Name.ToLower().Replace(" ", "") == model.City.ToLower().Replace(" ", "")).Id;

                cityChanged = true;
            }

            await DistrictExists(model.District);

            var oldDistrictId = barberShop.DistrictId;

            bool districtChanged = false;

            if (barberShop.District.Name != model.District)
            {
                barberShop.DistrictId = this.data.Districts
                    .First(d => d.Name.ToLower().Replace(" ", "") == model.District.ToLower().Replace(" ", "")).Id;

                districtChanged = true;
            }

            barberShop.StartHour = startHour;
            barberShop.FinishHour = finishHour;
            barberShop.Name = model.Name;
            barberShop.ImageUrl = model.ImageUrl;
            barberShop.IsPublic = false;

            await this.data.SaveChangesAsync();

            if (cityChanged)
            {
                await CityIsUsed(oldCityId);
            }
            if (districtChanged)
            {
                await DistrictIsUsed(oldDistrictId);
            }
        }

        /// <summary>
        /// This method allows the owners of the barbershop or the admin to delete it.
        /// If a barbershop is deleted by owner he should be the only owner.
        /// </summary>
        /// <param name="barberShopId"></param>
        /// <param name="userId"></param>
        /// <param name="userIsAdmin"></param>
        /// <returns></returns>
        /// <exception cref="ModelStateCustomException"></exception>
        public async Task DeleteBarberShopAsync(int barberShopId, string userId, bool userIsAdmin)
        {
            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .Include(bs => bs.Orders)
                .Include(bs => bs.Services)
                .ThenInclude(s => s.Service)
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId && !bs.IsDeleted);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            var user = await this.data.Users.FirstAsync(u => u.Id == userId);

            if (!userIsAdmin)
            {
                if (barberShop.Barbers.Count(b => b.IsOwner) > 1)
                {
                    throw new ModelStateCustomException("", $"You are not the only owner of {barberShop.Name}. If you want to unasign only yourself as owner and barber from {barberShop.Name} press the 'resign' button in the barbershop manage menu. If you want to delete {barberShop.Name} you have to be the only owner.");
                }

                var barber = await this.data.Barbers
                    .Include(b => b.BarberShops)
                    .FirstOrDefaultAsync(b => b.UserId == userId && !b.IsDeleted &&
                        b.BarberShops.Any(bs => bs.BarberShopId == barberShop.Id && bs.IsOwner));

                if (barber == null)
                {
                    throw new ModelStateCustomException("", $"You have to be owner of {barberShop.Name} to do this action");
                }

                barber.BarberShops
                    .Remove(barber.BarberShops
                            .Where(bs => bs.BarberShopId == barberShop.Id)
                            .First());

                if (!barber.BarberShops.Any(bs => bs.IsOwner))
                {
                    await userManager.RemoveFromRoleAsync(user, BarberShopOwnerRoleName);
                }
            }

            if (barberShop.Orders.Any())
            {
                var orders = await this.data.Orders.Where(o => o.BarberShopId == barberShop.Id).ToListAsync();

                orders.ForEach(o => o.IsDeleted = true);
                orders.ForEach(o => o.DeleteDate = DateTime.UtcNow);
            }

            barberShop.IsPublic = false;

            barberShop.IsDeleted = true;

            barberShop.DeleteDate = DateTime.UtcNow;

            if (barberShop.Barbers.Any())
            {
                barberShop.Barbers.Clear();
            }

            var removedServices = barberShop.Services.ToList();

            if (barberShop.Services.Any())
            {
                barberShop.Services.Clear();
            }

            await this.data.SaveChangesAsync();

            await ServiceIsUsed(removedServices);

            await signInManager.RefreshSignInAsync(user);
        }

        private async Task CityExists(string city)
        {
            if (!this.data.Cities
                .Any(c => c.Name.ToLower().Replace(" ", "") == city.ToLower().Replace(" ", "")))
            {
                string cityName = city;
                
                // If the city input is sofia it will save Sofia
                if (!char.IsUpper(city[0]))
                {
                    cityName = char.ToUpper(city[0]).ToString() + city.Substring(1);
                }

                await this.data.Cities.AddAsync(new City { Name = cityName });
                await this.data.SaveChangesAsync();
            }
        }

        private async Task DistrictExists(string district)
        {
            if (!this.data.Districts
                .Any(d => d.Name.ToLower().Replace(" ", "") == district.ToLower().Replace(" ", "")))
            {
                string districtName = district;

                // If the district input is lozenets it will save Lozenets
                if (!char.IsUpper(district[0]))
                {
                    districtName = char.ToUpper(district[0]).ToString() + district.Substring(1);
                }

                await this.data.Districts.AddAsync(new District { Name = districtName });
                await this.data.SaveChangesAsync();
            }
        }

        private async Task CityIsUsed(int id)
        {
            if (!this.data.BarberShops.Any(b => b.CityId == id))
            {
                this.data.Cities.Remove(this.data.Cities.Find(id));

                await this.data.SaveChangesAsync();
            }
        }

        private async Task DistrictIsUsed(int id)
        {
            if (!this.data.BarberShops.Any(b => b.DistrictId == id))
            {
                this.data.Districts.Remove(this.data.Districts.Find(id));

                await this.data.SaveChangesAsync();
            }
        }

        private async Task ServiceIsUsed(IEnumerable<BarberShopServices> services)
        {
            foreach (var service in services)
            {
                if (!await this.data.BarberShops.AnyAsync(bs => bs.Services.Any(s => s.ServiceId == service.ServiceId)))
                {
                    service.Service.IsDeleted = true;
                    service.Service.DeleteDate = DateTime.UtcNow;

                    await this.data.SaveChangesAsync();
                }
            }
        }

        private static Tuple<TimeSpan, TimeSpan> ParseHours(string startHour, string finishHour)
        {
            var startHourArr = startHour.Split(':');
            var finishHourArr = finishHour.Split(':');

            var startHourParsed = new TimeSpan(int.Parse(startHourArr[0]), int.Parse(startHourArr[1]), 0);

            var finishHourParsed = new TimeSpan(int.Parse(finishHourArr[0]), int.Parse(finishHourArr[1]), 0);

            return Tuple.Create(startHourParsed, finishHourParsed);
        }

        public async Task<ManageBarberShopViewModel> BarberShopInformationAsync(string userId, int barberShopId)
        {
            if (!this.data.BarberShops.Any(bs => bs.Id == barberShopId && !bs.IsDeleted
                    && bs.Barbers.Any(b => b.IsOwner && !b.Barber.IsDeleted && b.Barber.UserId == userId)))
            {
                throw new ModelStateCustomException("", "You have to be owner to do this action");
            }

            return await this.data.BarberShops
            .Where(bs => bs.Id == barberShopId && !bs.IsDeleted)
            .Select(bs => new ManageBarberShopViewModel
            {
                BarberShopId = bs.Id,
                BarberShopName = bs.Name,
                Barbers = bs.Barbers.Select(b => new BarberViewModel
                {
                    BarberId = b.BarberId,
                    BarberName = b.Barber.FirstName + ' ' + b.Barber.LastName,
                    IsOwner = b.IsOwner,
                    IsAvailable = b.IsAvailable,
                    UserId = b.Barber.UserId
                })
               .ToList()
            })
            .FirstOrDefaultAsync();
        }

        public async Task<string> GetBarberShopNameToFriendlyUrlAsync(int id)
            => await this.data.BarberShops.Where(bs => bs.Id == id).Select(bs => bs.Name.Replace(' ', '-')).FirstOrDefaultAsync();

        public async Task MakeBarberShopPublicAsync(int barberShopId)
        {
            var barberShop = await this.data.BarberShops.FindAsync(barberShopId);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            barberShop.IsPublic = true;

            await this.data.SaveChangesAsync();
        }

        public async Task MakeBarberShopPrivateAsync(int barberShopId)
        {
            var barberShop = await this.data.BarberShops.FindAsync(barberShopId);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            barberShop.IsPublic = false;

            await this.data.SaveChangesAsync();
        }
    }
}
