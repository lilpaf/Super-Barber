using Microsoft.AspNetCore.Mvc;
using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Models;

namespace SuperBarber.Controllers
{
    public class BarberShopController : Controller
    {
        private readonly SuperBarberDbContext data;

        public BarberShopController(SuperBarberDbContext data)
        {
            this.data = data;
        }

        public IActionResult Add() => View();

        [HttpPost]
        public IActionResult Add(AddBarberShopFormModel barberShop)
        {
            if (!ModelState.IsValid)
            {
                return View(barberShop);
            }

            if (!this.data.Cities.Any(x => x.Name.ToLower() == barberShop.City.ToLower()))
            {
                this.data.Cities.Add(new City { Name = barberShop.City });
                this.data.SaveChanges();
            }
            
            if (!this.data.Districts.Any(x => x.Name.ToLower() == barberShop.District.ToLower()))
            {
                this.data.Districts.Add(new District { Name = barberShop.District });
                this.data.SaveChanges();
            }

            var barberShopData = new BarberShop
            {
                Name = barberShop.Name,
                CityId = this.data.Cities
                .First(x => x.Name.ToLower() == barberShop.City.ToLower()).Id,
                DistrictId = this.data.Districts
                .First(x => x.Name.ToLower() == barberShop.District.ToLower()).Id,
                Street = barberShop.Street,
                ImageUrl = barberShop.ImageUrl
            };

            if (this.data.BarberShops
                .Any(x => x.Name == barberShopData.Name 
                && x.CityId == barberShopData.CityId
                && x.DistrictId == barberShopData.DistrictId
                 && x.Street == barberShopData.Street))
            {
                return View(barberShop);
            }

            this.data.BarberShops.Add(barberShopData);
            this.data.SaveChanges();

            return RedirectToAction("Index", "Home");
        }
        
    }
}
