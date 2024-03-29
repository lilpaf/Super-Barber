﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperBarber.Core.Extensions;
using SuperBarber.Core.Models.Order;
using SuperBarber.Core.Services.Order;
using SuperBarber.Extensions;
using static SuperBarber.Extensions.WebConstants;
using static SuperBarber.Extensions.WebConstants.OrderControllerConstants;

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

                    await this.orderService.RemoveOrderAsync(orderId, (int)barberId, userId);

                    TempData[GlobalMessageKey] = RemoveSuccsessMessege;

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

                    await this.orderService.RemoveYourOrderAsync(orderId, userId);

                    TempData[GlobalMessageKey] = RemoveSuccsessMessege;

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
        public async Task<IActionResult> Mine([FromQuery] OrderViewModel model)
        {
            var userId = User.Id();

            return View(await orderService.GetMyOrdersAsync(userId, model.CurrentPage));
        }
    }
}
