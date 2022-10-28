using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperBarber.Models.Service;
using SuperBarber.Services.CutomException;
using SuperBarber.Services.Service;

namespace SuperBarber.Controllers
{
    [Authorize]
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
                    model.Categories = await serviceService.GetServiceCategoriesAsync();

                    return View(model);
                }

                await serviceService.AddService(model);

                return RedirectToAction("Index", "Home");
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                return View(model);
            }
        }
    }
}
