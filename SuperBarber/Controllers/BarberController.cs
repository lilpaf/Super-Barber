using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Models.Barber;
using SuperBarber.Services;
using System.Security.Claims;

namespace SuperBarber.Controllers
{
    [Authorize]
    public class BarberController : Controller
    {
        private readonly SuperBarberDbContext data;

        public BarberController(SuperBarberDbContext data)
            => this.data = data;

        public IActionResult Add()
            => View();

        [HttpPost]
        public async Task<IActionResult> Add(AddBarberFormModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (this.data.Barbers.Any(b => b.UserId == userId))
            {
                this.ModelState.AddModelError(nameof(model.FullName), "User is already a barber");

                return View(model);
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var barber = new Barber
            {
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Email = userEmail,
                UserId = userId,
            };

            await data.Barbers.AddAsync(barber);
            
            await data.SaveChangesAsync();

            return RedirectToAction("All", "BarberShop");
        }

    }
}
