﻿using SuperBarber.Data.Models;
using SuperBarber.Models.Cart;
using SuperBarber.Models.Service;

namespace SuperBarber.Services.Cart
{
    public interface ICartService
    {
        Task<Data.Models.Service> GetServiceAsync(int serviceId);

        Task<decimal> BarberShopServicePrice(int barberShopId, int serviceId);

        Task AddOrderAsync(BookServiceFormModel model, List<ServiceListingViewModel> cartList, string userId);

        Task<string> GetBarberShopNameAsync(int id);

        string GetBarberShopNameToFriendlyUrl(string name);
    }
}
