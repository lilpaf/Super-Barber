using Microsoft.AspNetCore.Mvc;
using SuperBarber.Infrastructure;
using SuperBarber.Services.Order;

namespace SuperBarber.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService orderService;

        public OrderController(IOrderService orderService)
            => this.orderService = orderService;

        public async Task<IActionResult> Remove(string orderId, int barberId)
        {
            try
            {
                if (barberId == 0 || orderId == null)
                {
                    return BadRequest();
                }

                var userId = User.Id();

                await this.orderService.RemoveOrder(orderId, barberId, userId);

                return RedirectToAction("OrdersInfo", "Barber");
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex);

                return RedirectToAction("OrdersInfo", "Barber"); ;
            }
        }
    }
}
