using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Models.BarberShop;

namespace SuperBarber.Controllers
{
    public class BarberShopController : Controller
    {
        private readonly SuperBarberDbContext data;

        public BarberShopController(SuperBarberDbContext data) 
            => this.data = data;

        public IActionResult All([FromQuery]AllBarberShopQueryModel query)
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
                barberShops = barberShopQuery
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
                    .ToList();
            }

            var cities = this.data.Cities
                .Select(c => c.Name)
                .OrderBy(c => c)
                .ToList();

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
        public IActionResult Add(AddBarberShopFormModel barberShop)
        {
            if (!ModelState.IsValid)
            {
                return View(barberShop);
            }

            if (!this.data.Cities
                .Any(c => c.Name.ToLower().Trim() == barberShop.City.ToLower().Trim()))
            {
                this.data.Cities.Add(new City { Name = barberShop.City });
                this.data.SaveChanges();
            }
            
            if (!this.data.Districts
                .Any(d => d.Name.ToLower().Trim() == barberShop.District.ToLower().Trim()))
            {
                this.data.Districts.Add(new District { Name = barberShop.District });
                this.data.SaveChanges();
            }
            
            var startHourArr = barberShop.StartHour.Split(':');
            var finishHourArr = barberShop.FinishHour.Split(':');

            var startHour = new TimeSpan(int.Parse(startHourArr[0]), int.Parse(startHourArr[1]), 0);
            
            var finishHour = new TimeSpan(int.Parse(finishHourArr[0]), int.Parse(finishHourArr[1]), 0);

            if (startHour >= finishHour)
            {
                this.ModelState.AddModelError(nameof(barberShop.StartHour), "Opening hour must be smaller that the closing hour. And can not be the same as the closing hour");

                return View(barberShop);
            }

            var barberShopData = new BarberShop
            {
                Name = barberShop.Name,
                CityId = this.data.Cities
                .First(c => c.Name.ToLower().Trim() == barberShop.City.ToLower().Trim()).Id,
                DistrictId = this.data.Districts
                .First(d => d.Name.ToLower().Trim() == barberShop.District.ToLower().Trim()).Id,
                Street = barberShop.Street,
                StartHour = startHour,
                FinishHour = finishHour,
                ImageUrl = barberShop.ImageUrl
            };

            if (this.data.BarberShops
                .Any(b => b.Name.ToLower().Trim() == barberShopData.Name.ToLower().Trim()
                && b.CityId == barberShopData.CityId
                && b.DistrictId == barberShopData.DistrictId
                 && b.Street == barberShopData.Street))
            {
                this.ModelState.AddModelError(nameof(barberShop.Name), "This Barbershop already exist");

                return View(barberShop);
            }

            this.data.BarberShops.Add(barberShopData);
            this.data.SaveChanges();

            return RedirectToAction(nameof(All));
        }
        
    }
}
