using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SuperBarber.Core.Extensions;
using SuperBarber.Core.Models.BarberShop;
using SuperBarber.Core.Services.BarberShops;
using SuperBarber.Extensions;
using static SuperBarber.Core.Extensions.CustomRoles;
using static SuperBarber.Extensions.WebConstants;
using static SuperBarber.Extensions.WebConstants.BarberShopControllerConstants;

namespace SuperBarber.Controllers
{
    [Authorize]
    public class BarberShopController : Controller
    {
        private readonly IBarberShopService barberShopService;
        private readonly IWebHostEnvironment hostEnvironment;

        public BarberShopController(IBarberShopService barberShopService, IWebHostEnvironment hostEnvironment)
        { 
            this.barberShopService = barberShopService;
            this.hostEnvironment = hostEnvironment;
        }

        [AllowAnonymous]
        [RestoreModelStateFromTempData]
        public async Task<IActionResult> All([FromQuery] AllBarberShopQueryModel query)
        {
            var userId = User.Id();

            if (TempData.ContainsKey("list"))
            {
                var barberShops = JsonConvert.DeserializeObject<List<BarberShopListingViewModel>>((string)TempData["list"]);

                return View(await barberShopService.AllBarberShopsAsync(query, userId, barberShops));
            }

            return View(await barberShopService.AllBarberShopsAsync(query, userId));
        }

        [Authorize(Roles = BarberRoleName)]
        public IActionResult Add() => View();

        [HttpPost]
        [Authorize(Roles = BarberRoleName)]
        public async Task<IActionResult> Add(BarberShopAddFormModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var wwwRootPath = hostEnvironment.WebRootPath;

                var userId = User.Id();

                await barberShopService.AddBarberShopAsync(model, userId, wwwRootPath);
                
                TempData[GlobalMessageKey] = AddSuccsessMessege;

                return RedirectToAction(nameof(Mine));
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                return View(model);
            }
        }

        [Authorize(Roles = BarberShopOwnerOrBarber)]
        [RestoreModelStateFromTempData]
        public async Task<IActionResult> Mine([FromQuery] MineBarberShopViewModel model)
        {
            var userId = User.Id();

            return View(await barberShopService.MineBarberShopsAsync(userId, model.CurrentPage));
        }

        [Authorize(Roles = BarberShopOwnerOrAdmin)]
        public async Task<IActionResult> Edit(int barberShopId)
            => View(await barberShopService.DisplayBarberShopInfoAsync(barberShopId));

        [HttpPost]
        [Authorize(Roles = BarberShopOwnerOrAdmin)]
        public async Task<IActionResult> Edit(int barberShopId, string information, BarberShopEditFormModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (barberShopId == 0 || information == null || information != await this.barberShopService.GetBarberShopNameToFriendlyUrlAsync(barberShopId))
                {
                    return BadRequest();
                }

                var wwwRootPath = hostEnvironment.WebRootPath;

                var userId = User.Id();

                var userIsAdmin = User.IsAdmin();

                await barberShopService.EditBarberShopAsync(model, barberShopId, userId, userIsAdmin, wwwRootPath);

                TempData[GlobalMessageKey] = string.Join(EditSuccsessMessege, information.Replace('-', ' '));

                if (userIsAdmin)
                {
                    return RedirectToAction("All", "BarberShop", new { area = "Admin" });
                }

                return RedirectToAction(nameof(Mine));
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = BarberShopOwnerOrAdmin)]
        public async Task<IActionResult> Delete(int barberShopId, string information)
        {
            try
            {
                if (barberShopId == 0 || information == null || information != await this.barberShopService.GetBarberShopNameToFriendlyUrlAsync(barberShopId))
                {
                    return BadRequest();
                }

                var wwwRootPath = hostEnvironment.WebRootPath;

                var userId = User.Id();

                var userIsAdmin = User.IsAdmin();

                await barberShopService.DeleteBarberShopAsync(barberShopId, userId, userIsAdmin, wwwRootPath);

                TempData[GlobalMessageKey] = string.Join(DeleteSuccsessMessege, information.Replace('-', ' '));

                if (userIsAdmin)
                {
                    return RedirectToAction("All", "BarberShop", new { area = "Admin" });
                }

                return RedirectToAction(nameof(Mine));
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                if (User.IsAdmin())
                {
                    return RedirectToAction("All", "BarberShop", new { area = "Admin" });
                }

                return RedirectToAction(nameof(Mine));
            }
        }
        
        [Authorize(Roles = BarberShopOwnerRoleName)]
        [RestoreModelStateFromTempData]
        public async Task<IActionResult> Manage(int barberShopId, string information)
        {
            try
            {
                if (barberShopId == 0 || information == null || information != await this.barberShopService.GetBarberShopNameToFriendlyUrlAsync(barberShopId))
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
