using SuperBarber.Models.BarberShop;
using SuperBarber.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBarber.Data.Models;
using Microsoft.AspNetCore.Identity;
using static SuperBarber.Infrastructure.CustomRoles;
using SuperBarber.Infrastructure;
using SuperBarber.Models.Barber;
using SuperBarber.Services.BarberShops.Models;

namespace SuperBarber.Services.BarberShops
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

        public async Task<AllBarberShopQueryModel> AllBarberShopsAsync([FromQuery] AllBarberShopQueryModel query, string userId, List<BarberShopListingViewModel>? barberShops = null, bool publicOnly = true)
        {
            var barberShopQuery = this.data.BarberShops
                .Include(bs => bs.Barbers)
                .ThenInclude(b => b.Barber)
                .Where(b => b.IsPublic == publicOnly)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                barberShopQuery = barberShopQuery
                .Where(b => b.Name.ToLower().Trim()
                    .Contains(query.SearchTerm.ToLower().Trim()));
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
               .Where(c => c.Name.ToLower().Trim() == name.ToLower().Trim())
               .Select(c => c.Id)
               .FirstOrDefaultAsync();

        private async Task<int> GetDistrictIdAsync(string name)
           => await this.data.Districts
               .Where(d => d.Name.ToLower().Trim() == name.ToLower().Trim())
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
                ImageUrl = model.ImageUrl,
                IsPublic = false
            };

            if (this.data.BarberShops
                .Any(b => b.Name.ToLower().Trim() == barberShop.Name.ToLower().Trim()
                && b.CityId == barberShop.CityId
                && b.DistrictId == barberShop.DistrictId
                 && b.Street == barberShop.Street))
            {
                throw new ModelStateCustomException(nameof(model.Name), "This barbershop already exist");
            }

            var barber = await data.Barbers
                .FirstAsync(b => b.UserId == userId);

            var user = await this.data.Users
                .FirstAsync(u => u.Id == userId);

            barberShop.Barbers.Add(new BarberShopBarbers
            {
                BarberShop = barberShop,
                Barber = barber,
                IsOwner = true
            });

            await userManager.AddToRoleAsync(user, BarberShopOwnerRoleName);

            await this.data.BarberShops.AddAsync(barberShop);

            await this.data.SaveChangesAsync();

            await signInManager.RefreshSignInAsync(user);
        }

        public async Task<IEnumerable<BarberShopListingViewModel>> MineBarberShopsAsync(string userId)
            => await this.data.BarberShops
               .Where(bs => bs.Barbers.Any(b => b.Barber.UserId == userId))
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
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            return formModel;
        }

        public async Task EditBarberShopAsync(BarberShopFormModel model, int barberShopId, string userId, bool userIsAdmin)
        {
            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            if (!userIsAdmin)
            {
                var barber = await this.data.Barbers
                .FirstOrDefaultAsync(b => b.UserId == userId &&
                        b.BarberShops.Any(bs => bs.BarberShopId == barberShop.Id && bs.IsOwner));

                if (barber == null)
                {
                    throw new ModelStateCustomException("", "You have to be a owner to do this action");
                }

                if (!barberShop.Barbers.Any(b => b.BarberId == barber.Id))
                {
                    throw new ModelStateCustomException("", "You can not delete this barbershop");
                }
            }
            
            await CityExists(model.City);

            var oldCityId = barberShop.CityId;

            barberShop.CityId = this.data.Cities
                .First(c => c.Name.ToLower().Trim() == model.City.ToLower().Trim()).Id;

            await DistrictExists(model.District);

            var oldDistrictId = barberShop.DistrictId;

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
            barberShop.IsPublic = false;

            await this.data.SaveChangesAsync();

            await CityIsUsed(oldCityId);

            await DistrictIsUsed(oldDistrictId);
        }

        public async Task DeleteBarberShopAsync(int barberShopId, string userId, bool userIsAdmin)
        {
            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .Include(bs => bs.Orders)
                .Include(bs => bs.Services)
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            if (barberShop.Orders.Any() && userIsAdmin)
            {
                var orders = await this.data.Orders.Where(o => o.BarberShopId == barberShop.Id).ToListAsync();

                this.data.Orders.RemoveRange(orders);
            }

            var user = await this.data.Users.FirstAsync(u => u.Id == userId);

            if (!userIsAdmin)
            {
                // In development
                /*
                if (barberShop.Barbers.Where(b => b.Owner).Count() > 1)
                {
                    throw new ModelStateCustomException("", "A message was sent to the other owners to confirm deleteing the barbershop. If you want to unasign only yourself as owner and barber from this shop press the 'I do not work here anymore' button");
                }
                */

                if (barberShop.Barbers.Count(b => b.IsOwner) > 1)
                {
                    throw new ModelStateCustomException("", $"You are not the only owner of {barberShop.Name}. If you want to unasign only yourself as owner and barber from {barberShop.Name} press the 'resign' button in the barbershop manage menu");
                }

                var barber = await this.data.Barbers
                    .Include(b => b.BarberShops)
                    .FirstOrDefaultAsync(b => b.UserId == userId &&
                        b.BarberShops.Any(bs => bs.BarberShopId == barberShop.Id && bs.IsOwner));

                if (barber == null)
                {
                    throw new ModelStateCustomException("", "You have to be owner to do this action");
                }

                if (!barberShop.Barbers.Any(b => b.BarberId == barber.Id))
                {
                    throw new ModelStateCustomException("", "You can not delete this barbershop");
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

                this.data.Orders.RemoveRange(orders);
            }

            this.data.BarberShops.Remove(barberShop);

            await this.data.SaveChangesAsync();

            var barshopServices = barberShop.Services.ToList();

            await ServiceIsUsed(barshopServices);

            await CityIsUsed(barberShop.CityId);

            await DistrictIsUsed(barberShop.DistrictId);

            await signInManager.RefreshSignInAsync(user);
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
                    this.data.Services
                        .Remove(this.data.Services.Find(service.ServiceId));

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
            if (!this.data.BarberShops.Any(bs => bs.Id == barberShopId 
                    && bs.Barbers.Any(b => b.IsOwner && b.Barber.UserId == userId)))
            {
                throw new ModelStateCustomException("", "You have to be owner to do this action");
            }

            return await this.data.BarberShops
            .Where(bs => bs.Id == barberShopId)
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
