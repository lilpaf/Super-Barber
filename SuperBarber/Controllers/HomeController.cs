using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SuperBarber.Core.Extensions;
using SuperBarber.Core.Models.Home;
using SuperBarber.Core.Services.Home;
using SuperBarber.Extensions;
using SuperBarber.Models;
using System.Diagnostics;

namespace SuperBarber.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService homeService;

        private readonly ILogger logger;

        public HomeController(IHomeService homeService, ILogger logger)
        {
            this.homeService = homeService;
            this.logger = logger;
        }

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

                var barberShops = await homeService.SearchAvalibleBarbershopsAsync(city, district, date, time, userId);

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
        public IActionResult Error()
        {
            var feature = this.HttpContext.Features.Get<IExceptionHandlerFilter>();


            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}