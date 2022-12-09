using Microsoft.AspNetCore.Mvc;
using SuperBarber.Core.Extensions;
using SuperBarber.Core.Models.BarberShop;
using SuperBarber.Core.Services.BarberShops;
using SuperBarber.Extensions;
using static SuperBarber.Extensions.WebConstants;

namespace SuperBarber.Areas.Admin.Controllers
{
    public class BarberShopController : AdminController
    {
        private readonly IBarberShopService barberShopService;

        public BarberShopController(IBarberShopService barberShopService)
            => this.barberShopService = barberShopService;

        [RestoreModelStateFromTempData]
        public async Task<IActionResult> All([FromQuery] AllBarberShopQueryModel query)
        {
            var userId = User.Id();

            return View(await barberShopService.AllBarberShopsAsync(query, userId, null, false));
        }
        
        public async Task<IActionResult> MakePublic(int barberShopId, string information)
        {
            try
            {
                if (barberShopId == 0 || information == null || information != await this.barberShopService.GetBarberShopNameToFriendlyUrlAsync(barberShopId))
                {
                    return BadRequest();
                }

                await barberShopService.MakeBarberShopPublicAsync(barberShopId);

                TempData[GlobalMessageKey] = $"{information.Replace('-', ' ')} is set to public!";

                return RedirectToAction(nameof(All));
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);
                
                return RedirectToAction(nameof(All));
            }
        }
        
        public async Task<IActionResult> MakePrivate(int barberShopId, string information)
        {
            try
            {
                if (barberShopId == 0 || information == null || information != await this.barberShopService.GetBarberShopNameToFriendlyUrlAsync(barberShopId))
                {
                    return BadRequest();
                }

                await barberShopService.MakeBarberShopPrivateAsync(barberShopId);

                TempData[GlobalMessageKey] = $"{information.Replace('-', ' ')} is set to private!";

                return RedirectToAction(nameof(All));
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("All", "BarberShop", new { area = "" });
            }
        }
    }
}
