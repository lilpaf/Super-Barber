using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperBarber.Infrastructure;
using SuperBarber.Models.Order;
using SuperBarber.Services.Barbers;
using static SuperBarber.Infrastructure.CustomRoles;
using static SuperBarber.Infrastructure.WebConstants;

namespace SuperBarber.Controllers
{
    [Authorize]
    public class BarberController : Controller
    {
        private readonly IBarberService barberService;

        public BarberController(IBarberService barberService)
            => this.barberService = barberService;

        [HttpGet]
        public IActionResult Add() => View();

        [HttpPost]
        [ActionName("Add")]
        public async Task<IActionResult> AddBarber()
        {
            try
            {
                var userId = User.Id();

                await barberService.AddBarberAsync(userId);

                TempData[GlobalMessageKey] = $"You are barber now!";

                return RedirectToAction("All", "BarberShop");
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                return View();
            }
        }

        [HttpPost]
        [Authorize(Roles = BarberRoleName)]
        public async Task<IActionResult> AssignBarber(int barberShopId, string information)
        {
            try
            {
                if (barberShopId == 0 || information == null || information != await this.barberService.GetBarberShopNameToFriendlyUrlAsync(barberShopId))
                {
                    return BadRequest();
                }

                var userId = User.Id();

                await barberService.AsignBarberToBarberShopAsync(barberShopId, userId);

                TempData[GlobalMessageKey] = $"You started working at {information.Replace('-', ' ')}!";

                return RedirectToAction("Mine", "BarberShop");
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("All", "BarberShop"); ;
            }
        }

        [HttpPost]
        [Authorize(Roles = BarberShopOwnerRoleName)]
        public async Task<IActionResult> UnassignBarber(int barberShopId, int barberId, string information)
        {
            try
            {
                if (barberId == 0 || barberShopId == 0 || information == null || information != await this.barberService.GetBarberShopNameToFriendlyUrlAsync(barberShopId))
                {
                    return BadRequest();
                }

                var userId = User.Id();

                await barberService.UnasignBarberFromBarberShopAsync(barberShopId, barberId, userId);

                if (await barberService.CheckIfUserIsTheBabrerToFire(userId, barberId))
                {
                    return RedirectToAction("All", "BarberShop");
                }

                TempData[GlobalMessageKey] = $"{await barberService.GetBarberNameAsync(barberId)} stoped working at {information.Replace('-', ' ')}!";

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information });
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information }); ;
            }
        }

        [Authorize(Roles = BarberShopOwnerRoleName)]
        public async Task<IActionResult> MakeOwner(int barberShopId, int barberId, string information)
        {
            try
            {
                if (barberId == 0 || barberShopId == 0 || information == null || information != await this.barberService.GetBarberShopNameToFriendlyUrlAsync(barberShopId))
                {
                    return BadRequest();
                }

                var userId = User.Id();

                await barberService.AddOwnerToBarberShopAsync(barberShopId, barberId, userId);

                TempData[GlobalMessageKey] = $"{await barberService.GetBarberNameAsync(barberId)} is owner at {information.Replace('-', ' ')}!";

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information });
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information }); ;
            }
        }

        [HttpPost]
        [Authorize(Roles = BarberShopOwnerRoleName)]
        public async Task<IActionResult> RemoveOwner(int barberShopId, int barberId, string information)
        {
            try
            {
                if (barberId == 0 || barberShopId == 0 || information == null || information != await this.barberService.GetBarberShopNameToFriendlyUrlAsync(barberShopId))
                {
                    return BadRequest();
                }

                var userId = User.Id();

                await barberService.RemoveOwnerFromBarberShopAsync(barberShopId, barberId, userId);

                TempData[GlobalMessageKey] = $"{await barberService.GetBarberNameAsync(barberId)} is no longer owner at {information.Replace('-', ' ')}!";

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information });
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information }); ;
            }
        }

        [HttpPost]
        [Authorize(Roles = BarberShopOwnerRoleName)]
        public async Task<IActionResult> MakeUnavailable(int barberShopId, int barberId, string information)
        {
            try
            {
                if (barberId == 0 || barberShopId == 0 || information != await this.barberService.GetBarberShopNameToFriendlyUrlAsync(barberShopId))
                {
                    return BadRequest();
                }

                var userId = User.Id();

                await barberService.MakeBarberUnavailableAtBarberShopAsync(barberShopId, barberId, userId);

                TempData[GlobalMessageKey] = $"{await barberService.GetBarberNameAsync(barberId)} is unavailable at {information.Replace('-', ' ')}!";

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information });
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information }); ;
            }
        }

        [HttpPost]
        [Authorize(Roles = BarberShopOwnerRoleName)]
        public async Task<IActionResult> MakeAvailable(int barberShopId, int barberId, string information)
        {
            try
            {
                if (barberId == 0 || barberShopId == 0 || information == null ||  information != await this.barberService.GetBarberShopNameToFriendlyUrlAsync(barberShopId))
                {
                    return BadRequest();
                }

                var userId = User.Id();

                await barberService.MakeBarberAvailableAtBarberShopAsync(barberShopId, barberId, userId);

                TempData[GlobalMessageKey] = $"{await barberService.GetBarberNameAsync(barberId)} is available at {information.Replace('-', ' ')}!";

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information });
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information }); ;
            }
        }

        [RestoreModelStateFromTempData]
        [Authorize(Roles = BarberRoleName)]
        public async Task<IActionResult> OrdersInfo([FromQuery] OrderViewModel model)
        {
            var userId = User.Id();

            return View(await barberService.GetBarberOrdersAsync(userId, model.CurrentPage));
        }
    }
}
