using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SuperBarber.Infrastructure;
using SuperBarber.Models.BarberShop;
using SuperBarber.Services.BarberShops;
using static SuperBarber.CustomRoles;

namespace SuperBarber.Controllers
{
    [Authorize]
    public class BarberShopController : Controller
    {
        private readonly IBarberShopService barberShopService;

        public BarberShopController(IBarberShopService barberShopService)
            => this.barberShopService = barberShopService;

        [AllowAnonymous]
        [RestoreModelStateFromTempData]
        public async Task<IActionResult> All([FromQuery] AllBarberShopQueryModel query)
        {
            if (TempData.ContainsKey("list"))
            {
                var barberShops = JsonConvert.DeserializeObject<List<BarberShopListingViewModel>>((string)TempData["list"]);

                return View(await barberShopService.AllBarberShopsAsync(query, barberShops));
            }

            return View(await barberShopService.AllBarberShopsAsync(query));
        }

        [Authorize(Roles = BarberRoleName)]
        public IActionResult Add() => View();

        [HttpPost]
        [Authorize(Roles = BarberRoleName)]
        public async Task<IActionResult> Add(BarberShopFormModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userId = User.Id();

                await barberShopService.AddBarberShopAsync(model, userId);

                return RedirectToAction(nameof(All));
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                return View(model);
            }
        }

        [Authorize(Roles = BarberShopOwnerOrBarber)]
        [RestoreModelStateFromTempData]
        public async Task<IActionResult> Mine()
        {
            var userId = User.Id();

            return View(await barberShopService.MineBarberShopsAsync(userId));
        }

        [Authorize(Roles = BarberShopOwnerOrAdmin)]
        public async Task<IActionResult> Edit(int barberShopId)
            => View(await barberShopService.DisplayBarberShopInfoAsync(barberShopId));

        [HttpPost]
        [Authorize(Roles = BarberShopOwnerOrAdmin)]
        public async Task<IActionResult> Edit(int barberShopId, string information, BarberShopFormModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (barberShopId == 0 || information == null)
                {
                    return BadRequest();
                }

                var userId = User.Id();

                var userIsAdmin = User.IsInRole(CustomRoles.AdministratorRoleName);

                await barberShopService.EditBarberShopAsync(model, barberShopId, userId, userIsAdmin);

                return RedirectToAction(nameof(Mine));
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                return View(model);
            }
        }
        
        [Authorize(Roles = BarberShopOwnerOrAdmin)]
        public async Task<IActionResult> Delete(int barberShopId, string information)
        {
            try
            {
                if (barberShopId == 0 || information == null)
                {
                    return BadRequest();
                }

                var userId = User.Id();

                var userIsAdmin = User.IsInRole(CustomRoles.AdministratorRoleName);

                await barberShopService.DeleteBarberShopAsync(barberShopId, userId, userIsAdmin);

                return RedirectToAction(nameof(Mine));
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction(nameof(Mine));
            }
        }
        
        [Authorize(Roles = BarberShopOwnerRoleName)]
        [RestoreModelStateFromTempData]
        public async Task<IActionResult> Manage(int barberShopId, string information)
        {
            try
            {
                if (barberShopId == 0 || information == null)
                {
                    return BadRequest();
                }

                var userId = User.Id();

                return View(await barberShopService.BarberShopInformationAsync(userId, barberShopId));
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction(nameof(Mine));
            }
        }
    }
}
