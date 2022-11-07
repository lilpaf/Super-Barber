using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperBarber.Infrastructure;
using SuperBarber.Models.Service;
using SuperBarber.Services.CutomException;
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

        public async Task<IActionResult> Add() => View(new AddServiceFormModel
        {
            Categories = await serviceService.GetServiceCategoriesAsync()
        });

        [HttpPost]
        public async Task<IActionResult> Add(AddServiceFormModel model)
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

                await serviceService.AddServiceAsync(model, userId);

                return RedirectToAction("Index", "Home");
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                return View(model);
            }
        }

        public async Task<IActionResult> All(int Id)
        {
            try
            {
                return View(await serviceService.ShowServicesAsync(Id));
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                return RedirectToAction("All", "BarberShop");
            }
        }
    }
}
