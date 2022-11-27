using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperBarber.Infrastructure;
using SuperBarber.Models.Service;
using SuperBarber.Services.Service;
using static SuperBarber.CustomRoles;

namespace SuperBarber.Controllers
{
    [Authorize(Roles = BarberShopOwnerRoleName)]
    public class ServiceController : Controller
    {
        private readonly IServiceService serviceService;

        public ServiceController(IServiceService serviceService)
            => this.serviceService = serviceService;

        public async Task<IActionResult> Add(int barberShopId, string information)
        {
            if (barberShopId == 0 || information == null)
            {
                return BadRequest();
            }

            return View(new AddServiceFormModel
            {
                Categories = await serviceService.GetServiceCategoriesAsync()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddServiceFormModel model, int barberShopId)
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

                return RedirectToAction("Index", "Home");
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
                if (barberShopId == 0 || information == null)
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
    }
}
