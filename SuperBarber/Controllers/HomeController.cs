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
            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(district) || string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(time))
            {
                return View(new FilterBarberShopsViewModel
                {
                    Cities = this.data.Cities.Select(c => c.Name).OrderBy(c => c).ToList(),
                    Districts = this.data.Districts.Select(d => d.Name).OrderBy(c => c).ToList()
                });
            }
            var dateParsed = DateTime.Parse(date);

            var timeArr = time.Split(':');

            var ts = new TimeSpan(int.Parse(timeArr[0]), int.Parse(timeArr[1]), 0);

            dateParsed = dateParsed.Date + ts;

            var barberShopsData = data
                .BarberShops
                .Where(b => b.City.Name.ToLower().Trim() == city.ToLower().Trim() &&
                b.District.Name.ToLower().Trim() == district.ToLower().Trim() &&
                b.StartHour <= dateParsed.TimeOfDay &&
                b.FinishHour >= dateParsed.TimeOfDay &&
                b.Orders.Count(o => o.Date == dateParsed) == 0)
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
       
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}