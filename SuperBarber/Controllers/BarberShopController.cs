using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SuperBarber.Models.BarberShop;
using SuperBarber.Services.BarberShops;
using SuperBarber.Services.CutomException;

namespace SuperBarber.Controllers
{
    [Authorize]
    public class BarberShopController : Controller
    {
        private readonly IBarberShopService barberShopService;

        public BarberShopController(IBarberShopService barberShopService)
            => this.barberShopService = barberShopService;

        [AllowAnonymous]
        public async Task<IActionResult> All([FromQuery]AllBarberShopQueryModel query)
        {
            if (TempData.ContainsKey("list"))
            {
                var barberShops = JsonConvert.DeserializeObject<List<BarberShopListingViewModel>>((string) TempData["list"]);
                TempData.Clear();

                return View(await barberShopService.AllBarberShops(query, barberShops));
            }

            return View(await barberShopService.AllBarberShops(query));
        }

        public IActionResult Add() => View();

        [HttpPost]
        public async Task<IActionResult> Add(AddBarberShopFormModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                await barberShopService.AddBarberShop(model);

                return RedirectToAction(nameof(All));
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                return View(model);
            }
        }
        
    }
}
