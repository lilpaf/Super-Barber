using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperBarber.Data.Models;
using SuperBarber.Infrastructure;
using SuperBarber.Services.Barbers;
using SuperBarber.Services.Cart;
using static SuperBarber.CustomRoles;

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

                return RedirectToAction("All", "BarberShop");
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                return View();
            }
        }

        [Authorize(Roles = BarberRoleName)]
        public async Task<IActionResult> AssignBarber(int barberShopId, string information)
        {
            try
            {
                if (barberShopId == 0 || information == null)
                {
                    return BadRequest();
                }

                var userId = User.Id();

                await barberService.AsignBarberToBarberShopAsync(barberShopId, userId);

                return RedirectToAction("Mine", "BarberShop");
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("All", "BarberShop"); ;
            }
        }
        
        [Authorize(Roles = BarberRoleName)]
        public async Task<IActionResult> UnassignBarber(int barberShopId, string information)
        {
            try
            {
                if (barberShopId == 0 || information == null)
                {
                    return BadRequest();
                }

                var userId = User.Id();

                await barberService.UnasignBarberFromBarberShopAsync(barberShopId, userId, null);

                return RedirectToAction("Mine", "BarberShop");
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("Mine", "BarberShop"); ;
            }
        }
        
        [Authorize(Roles = BarberShopOwnerRoleName)]
        public async Task<IActionResult> FireBarber(int barberShopId, int barberId, string information)
        {
            try
            {
                if (barberId == 0 || barberShopId == 0 || information == null)
                {
                    return BadRequest();
                }

                await barberService.UnasignBarberFromBarberShopAsync(barberShopId, null, barberId);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information = barberService.GetBarberShopNameToFriendlyUrlAsync(barberShopId) });
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information = barberService.GetBarberShopNameToFriendlyUrlAsync(barberShopId) }); ;
            }
        }
        
        [Authorize(Roles = BarberShopOwnerRoleName)]
        public async Task<IActionResult> MakeOwner(int barberShopId, int barberId, string information)
        {
            try
            {
                if (barberId == 0 || barberShopId == 0 || information == null)
                {
                    return BadRequest();
                }

                await barberService.AddOwnerToBarberShop(barberShopId, barberId);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information = barberService.GetBarberShopNameToFriendlyUrlAsync(barberShopId) });
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information = barberService.GetBarberShopNameToFriendlyUrlAsync(barberShopId) }); ;
            }
        }
        
        [Authorize(Roles = BarberShopOwnerRoleName)]
        public async Task<IActionResult> RemoveOwner(int barberShopId, int barberId, string information)
        {
            try
            {
                if (barberId == 0 || barberShopId == 0 || information == null)
                {
                    return BadRequest();
                }

                await barberService.RemoveOwnerFromBarberShop(barberShopId, barberId);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information = barberService.GetBarberShopNameToFriendlyUrlAsync(barberShopId) });
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information = barberService.GetBarberShopNameToFriendlyUrlAsync(barberShopId) }); ;
            }
        }
    }
}
