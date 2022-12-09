using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperBarber.Core.Extensions;
using SuperBarber.Core.Models.Service;
using SuperBarber.Core.Services.Service;
using SuperBarber.Extensions;
using static SuperBarber.Core.Extensions.CustomRoles;
using static SuperBarber.Extensions.WebConstants;

namespace SuperBarber.Controllers
{
    [Authorize]
    public class ServiceController : Controller
    {
        private readonly IServiceService serviceService;

        public ServiceController(IServiceService serviceService)
            => this.serviceService = serviceService;

        [Authorize(Roles = BarberShopOwnerRoleName)]
        public async Task<IActionResult> Add(int barberShopId, string information)
        {
            if (barberShopId == 0 || information == null || information != await this.serviceService.GetBarberShopNameToFriendlyUrlAsync(barberShopId))
            {
                return BadRequest();
            }

            return View(new AddServiceFormModel
            {
                Categories = await serviceService.GetServiceCategoriesAsync()
            });
        }

        [HttpPost]
        [Authorize(Roles = BarberShopOwnerRoleName)]
        public async Task<IActionResult> Add(AddServiceFormModel model, int barberShopId, string information)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(new AddServiceFormModel
                    {
                        Categories = await serviceService.GetServiceCategoriesAsync()
                    });
                }

                var userId = User.Id();

                await serviceService.AddServiceAsync(model, userId, barberShopId);

                TempData[GlobalMessageKey] = $"Service was added to {information.Replace('-', ' ')}!";

                return RedirectToAction(nameof(Manage), new {barberShopId, information});
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                return View(model);
            }
        }

        [AllowAnonymous]
        [RestoreModelStateFromTempData]
        public async Task<IActionResult> All(int barberShopId, string information)
        {
            try
            {
                if (barberShopId == 0 || information == null || information != await this.serviceService.GetBarberShopNameToFriendlyUrlAsync(barberShopId))
                {
                    return BadRequest();
                }

                return View(await serviceService.ShowServicesAsync(barberShopId));
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("All", "BarberShop");
            }
        }

        [RestoreModelStateFromTempData]
        [Authorize(Roles = BarberShopOwnerOrAdmin)]
        public async Task<IActionResult> Manage(int barberShopId, string information)
        {
            try
            {
                if (barberShopId == 0 || information == null || information != await this.serviceService.GetBarberShopNameToFriendlyUrlAsync(barberShopId))
                {
                    return BadRequest();
                }

                return View(await serviceService.ShowServicesAsync(barberShopId));
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("All", "BarberShop");
            }
        }

        [HttpPost]
        [Authorize(Roles = BarberShopOwnerOrAdmin)]
        public async Task<IActionResult> Remove(int barberShopId, int serviceId, string information)
        {
            try
            {
                if (barberShopId == 0 || information == null || information != await this.serviceService.GetBarberShopNameToFriendlyUrlAsync(barberShopId) || serviceId == 0)
                {
                    return BadRequest();
                }

                var userId = User.Id();

                var userIsAdmin = User.IsAdmin();

                await serviceService.RemoveServiceAsync(barberShopId, serviceId, userId, userIsAdmin);

                TempData[GlobalMessageKey] = $"Service was removed from {information.Replace('-', ' ')}!";

                return RedirectToAction(nameof(Manage), new { barberShopId, information });
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction(nameof(Manage), new { barberShopId , information});
            }
        }
    }
}
