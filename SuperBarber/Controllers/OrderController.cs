using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperBarber.Infrastructure;
using static SuperBarber.Infrastructure.WebConstants;
using SuperBarber.Services.Order;

namespace SuperBarber.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService orderService;

        public OrderController(IOrderService orderService)
            => this.orderService = orderService;

        [HttpPost]
        public async Task<IActionResult> Remove(string orderId, int? barberId)
        {
            if (orderId == null)
            {
                return BadRequest();
            }

            if (barberId != null && User.IsBarber())
            {
                try
                {
                    if (barberId == 0)
                    {
                        return BadRequest();
                    }

                    var userId = User.Id();

                    await this.orderService.RemoveOrder(orderId, (int)barberId, userId);

                    TempData[GlobalMessageKey] = "Order was canceled!";

                    return RedirectToAction("OrdersInfo", "Barber");
                }
                catch (ModelStateCustomException ex)
                {
                    SetTempDataModelStateExtension.SetTempData(this, ex);

                    return RedirectToAction("OrdersInfo", "Barber");
                }
            }
            else if (barberId == null)
            {
                try
                {
                    var userId = User.Id();

                    await this.orderService.RemoveYourOrder(orderId, userId);

                    TempData[GlobalMessageKey] = "Order was canceled!";

                    return RedirectToAction(nameof(Mine));
                }
                catch (ModelStateCustomException ex)
                {
                    SetTempDataModelStateExtension.SetTempData(this, ex);

                    return RedirectToAction(nameof(Mine));
                }
            }

            return BadRequest();
        }

        [RestoreModelStateFromTempData]
        public async Task<IActionResult> Mine()
        {
            var userId = User.Id();

            return View(await orderService.GetMyOrdersAsync(userId));
        }
    }
}
