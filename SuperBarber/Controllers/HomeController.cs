using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SuperBarber.Infrastructure;
using SuperBarber.Models;
using SuperBarber.Models.Home;
using SuperBarber.Services.Home;
using System.Diagnostics;

namespace SuperBarber.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService homeService;

        public HomeController(IHomeService homeService)
            => this.homeService = homeService;

        [RestoreModelStateFromTempData]
        public async Task<IActionResult> Index(string city, string district, string date, string time)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(district) || string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(time))
                {
                    return View(new FilterBarberShopsViewModel
                    {
                        Cities = await homeService.GetCitiesAsync(),
                        Districts = await homeService.GetDistrictsAsync(),
                    });
                }

                var userId = User.Id();

                var barberShops = await homeService.SearchAvalibleBarbershopAsync(city, district, date, time, userId);

                TempData["list"] = JsonConvert.SerializeObject(barberShops);

                return RedirectToAction("All", "BarberShop");
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                return View(new FilterBarberShopsViewModel
                {
                    Cities = await homeService.GetCitiesAsync(),
                    Districts = await homeService.GetDistrictsAsync(),
                });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}