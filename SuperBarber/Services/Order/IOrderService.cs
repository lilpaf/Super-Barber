﻿using SuperBarber.Models.Order;

namespace SuperBarber.Services.Order
{
    public interface IOrderService
    {
        Task RemoveOrderAsync(string orderId, int barberId, string userId);

        Task<OrderViewModel> GetMyOrdersAsync(string userId, int currentPage);

        Task RemoveYourOrderAsync(string orderId, string userId);
    }
}
