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
            => this.data = data;

        public IActionResult Add() => View();

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

            var barberShopData = new BarberShop
            {
                Name = barberShop.Name,
                CityId = this.data.Cities
                .First(c => c.Name.ToLower() == barberShop.City.ToLower()).Id,
                DistrictId = this.data.Districts
                .First(d => d.Name.ToLower() == barberShop.District.ToLower()).Id,
                Street = barberShop.Street,
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

            return RedirectToAction("Index", "Home");
        }
        
    }
}
