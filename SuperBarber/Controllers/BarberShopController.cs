using Humanizer;
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

        public IActionResult Add() => View();

        public IActionResult All()
        {
            List<BarberShopListingViewModel> barberShops;
            
            if (TempData.ContainsKey("list"))
            {
                barberShops = JsonConvert.DeserializeObject<List<BarberShopListingViewModel>>((string) TempData["list"]);
                TempData.Clear();
            }
            else
            {
                barberShops = this.data.BarberShops
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

            return View(barberShops);
        }

        [HttpPost]
        public IActionResult Add(AddBarberShopFormModel barberShop)
        {
            if (!ModelState.IsValid)
            {
                return View(barberShop);
            }

            if (!this.data.Cities.Any(c => c.Name.ToLower() == barberShop.City.ToLower()))
            {
                this.data.Cities.Add(new City { Name = barberShop.City });
                this.data.SaveChanges();
            }
            
            if (!this.data.Districts.Any(d => d.Name.ToLower() == barberShop.District.ToLower()))
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
                this.ModelState.AddModelError(nameof(barberShop.StartHour), "Work start hour must be smaller that the finish hour. And can not be the same as the finish hour");

                return View(barberShop);
            }

            var barberShopData = new BarberShop
            {
                Name = barberShop.Name,
                CityId = this.data.Cities
                .First(c => c.Name.ToLower() == barberShop.City.ToLower()).Id,
                DistrictId = this.data.Districts
                .First(d => d.Name.ToLower() == barberShop.District.ToLower()).Id,
                Street = barberShop.Street,
                StartHour = startHour,
                FinishHour = finishHour,
                ImageUrl = barberShop.ImageUrl
            };

            if (this.data.BarberShops
                .Any(b => b.Name == barberShopData.Name 
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
