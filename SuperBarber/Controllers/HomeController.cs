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

        public IActionResult Index(string city, string district, string date, string time)
        {
            var barberShopQuery = this.data.BarberShops.AsQueryable();

            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(district) || string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(time))
            {
                var cities = this.data.Cities
                    .Select(c => c.Name)
                    .OrderBy(c => c)
                    .ToList();
                
                var districts = this.data.Districts
                    .Select(d => d.Name)
                    .OrderBy(d => d)
                    .ToList();

                return View(new FilterBarberShopsViewModel
                {
                    Cities = cities,
                    Districts = districts,
                    IsFound = true
                });
            }
            var dateParsed = DateTime.Parse(date);

            var timeArr = time.Split(':');

            var ts = new TimeSpan(int.Parse(timeArr[0]), int.Parse(timeArr[1]), 0);

            dateParsed = dateParsed.Date + ts;

            if (district == "All")
            {
                barberShopQuery = barberShopQuery
                .Where(b => b.City.Name.ToLower().Trim() == city.ToLower().Trim() &&
                b.StartHour <= dateParsed.TimeOfDay &&
                b.FinishHour >= dateParsed.TimeOfDay &&
                b.Orders.Count(o => o.Date == dateParsed) == 0);
            }
            else 
            {
                barberShopQuery = barberShopQuery
                .Where(b => b.City.Name.ToLower().Trim() == city.ToLower().Trim() &&
                b.District.Name.ToLower().Trim() == district.ToLower().Trim() &&
                b.StartHour <= dateParsed.TimeOfDay &&
                b.FinishHour >= dateParsed.TimeOfDay &&
                b.Orders.Count(o => o.Date == dateParsed) == 0);
            }

            var barberShopsData = barberShopQuery
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

            if (barberShopsData.Count() == 0)
            {
                var cities = this.data.Cities
                    .Select(c => c.Name)
                    .OrderBy(c => c)
                    .ToList();

                var districts = this.data.Districts
                    .Select(d => d.Name)
                    .OrderBy(d => d)
                    .ToList();

                return View(new FilterBarberShopsViewModel
                {
                    Cities = cities,
                    Districts = districts,
                    IsFound = false
                });
            }

            TempData["list"] = JsonConvert.SerializeObject(barberShopsData);

            return RedirectToAction("All", "BarberShop");
        }
       
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}