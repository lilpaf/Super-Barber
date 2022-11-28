using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperBarber.Data.Models;
using SuperBarber.Infrastructure;
using SuperBarber.Services.Barbers;
using SuperBarber.Services.Cart;
using static SuperBarber.Infrastructure.CustomRoles;

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
        
        [Authorize(Roles = BarberShopOwnerRoleName)]
        public async Task<IActionResult> UnassignBarber(int barberShopId, int barberId, string information)
        {
            try
            {
                if (barberId == 0 || barberShopId == 0 || information == null)
                {
                    return BadRequest();
                }

                var userId = User.Id();

                await barberService.UnasignBarberFromBarberShopAsync(barberShopId, barberId, userId);

                if (await barberService.CheckIfUserIsTheBabrerToFire(userId, barberId))
                {
                    return RedirectToAction("All", "BarberShop");
                }

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
                if (barberId == 0 || barberShopId == 0 || information == null)
                {
                    return BadRequest();
                }

                var userId = User.Id();

                await barberService.AddOwnerToBarberShop(barberShopId, barberId, userId);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information });
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information }); ;
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

                var userId = User.Id();

                await barberService.RemoveOwnerFromBarberShop(barberShopId, barberId, userId);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information });
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information }); ;
            }
        }
        
        [Authorize(Roles = BarberShopOwnerRoleName)]
        public async Task<IActionResult> MakeUnavailable(int barberShopId, int barberId, string information)
        {
            try
            {
                if (barberId == 0 || barberShopId == 0 || information == null)
                {
                    return BadRequest();
                }

                var userId = User.Id();

                await barberService.MakeBarberUnavailableFromBarberShop(barberShopId, barberId, userId);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information });
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information }); ;
            }
        }
        
        [Authorize(Roles = BarberShopOwnerRoleName)]
        public async Task<IActionResult> MakeAvailable(int barberShopId, int barberId, string information)
        {
            try
            {
                if (barberId == 0 || barberShopId == 0 || information == null)
                {
                    return BadRequest();
                }

                var userId = User.Id();

                await barberService.MakeBarberAvailableFromBarberShop(barberShopId, barberId, userId);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information });
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("Manage", "BarberShop", new { barberShopId, information }); ;
            }
        }
    }
}
