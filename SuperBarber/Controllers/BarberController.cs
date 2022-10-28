using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperBarber.Models.Barber;
using SuperBarber.Services.Barbers;
using SuperBarber.Services.CutomException;
using System.Security.Claims;

namespace SuperBarber.Controllers
{
    [Authorize]
    public class BarberController : Controller
    {
        private readonly IBarberService barberService;

        public BarberController(IBarberService barberService)
            => this.barberService = barberService;

        public IActionResult Add()
            => View();

        [HttpPost]
        public async Task<IActionResult> Add(AddBarberFormModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userEmail = User.FindFirstValue(ClaimTypes.Email);

                await barberService.AddBarber(userId, userEmail, model);

                return RedirectToAction("All", "BarberShop");
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                return View(model);
            }
        }

    }
}
