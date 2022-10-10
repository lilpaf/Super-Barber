using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SuperBarber.Data;
using SuperBarber.Models;
using SuperBarber.Models.BarberShop;
using SuperBarber.Models.Home;
using System.Diagnostics;

namespace SuperBarber.Controllers
{
    public class HomeController : Controller
    {
        private readonly SuperBarberDbContext data;

        public HomeController(SuperBarberDbContext data)
            => this.data = data;

        public IActionResult Index() => View(new FilterBarberShopsViewModel
        {
            Cities = GetBarberShopCities(),
            Districts = GetBarberShopDistricts()
        });

        [HttpPost]
        public IActionResult Index(FilterBarberShopsViewModel filterCiteria)
        {
            var date = DateTime.Parse(filterCiteria.Date);

            var time = filterCiteria.Time.Split(':');

            var ts = new TimeSpan(int.Parse(time[0]), int.Parse(time[1]), 0);

            date = date.Date + ts;

            var barberShopsData = data
                .BarberShops
                .Where(b => b.City.Id == filterCiteria.City &&
                b.District.Id == filterCiteria.District &&
                b.StartHour <= date.TimeOfDay &&
                b.FinishHour >= date.TimeOfDay &&
                b.Orders.Count(o => o.Date == date) == 0)
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

            TempData["list"] = JsonConvert.SerializeObject(barberShopsData);

            return RedirectToAction("All", "BarberShop");
        }
        
        private IEnumerable<BarberShopCityViewModel> GetBarberShopCities()
            => this.data
            .Cities
            .Select(c => new BarberShopCityViewModel
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToList();
        
        private IEnumerable<BarberShopDistrictViewModel> GetBarberShopDistricts()
            => this.data
            .Districts
            .Select(c => new BarberShopDistrictViewModel
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToList();



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}