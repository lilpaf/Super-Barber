using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Models.BarberShop;

namespace SuperBarber.Controllers
{
    [Authorize]
    public class BarberShopController : Controller
    {
        private readonly SuperBarberDbContext data;

        public BarberShopController(SuperBarberDbContext data) 
            => this.data = data;

        [AllowAnonymous]
        public async Task<IActionResult> All([FromQuery]AllBarberShopQueryModel query)
        {
            var barberShopQuery = this.data.BarberShops.AsQueryable();

            List<BarberShopListingViewModel> barberShops;

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
            
            if (TempData.ContainsKey("list"))
            {
                barberShops = JsonConvert.DeserializeObject<List<BarberShopListingViewModel>>((string) TempData["list"]);
                TempData.Clear();
            }
            else
            {
                barberShops = await barberShopQuery
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
            }

            var cities = await this.data.Cities
                .Select(c => c.Name)
                .OrderBy(c => c)
                .ToListAsync();

            return View(new AllBarberShopQueryModel
            {
                BarberShops = barberShops,
                SearchTerm = query.SearchTerm,
                Cities = cities,
                Sorting = query.Sorting,
                //Districts = districts
            });
        }

        public IActionResult Add() => View();

        [HttpPost]
        public async Task<IActionResult> Add(AddBarberShopFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

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
                this.ModelState.AddModelError(nameof(model.StartHour), "Opening hour must be smaller that the closing hour. And can not be the same as the closing hour");

                return View(model);
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
                this.ModelState.AddModelError(nameof(model.Name), "This Barbershop already exist");

                return View(model);
            }

            await this.data.BarberShops.AddAsync(barberShop);
            await this.data.SaveChangesAsync();

            return RedirectToAction(nameof(All));
        }
        
    }
}
