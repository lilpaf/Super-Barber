using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperBarber.Infrastructure;
using SuperBarber.Services.Barbers;
using SuperBarber.Services.CutomException;

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
    }
}
