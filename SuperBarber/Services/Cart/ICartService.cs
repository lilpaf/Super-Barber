﻿using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Models.Cart;
using SuperBarber.Models.Service;

namespace SuperBarber.Services.Cart
{
    public interface ICartService
    {
        Task<Data.Models.Service> GetServiceAsync(int serviceId);

        bool CheckBarberShopId(int barberShopId);

        Task AddOrderAsync(BookServiceFormModel model, List<ServiceListingViewModel> cartList, string userId);
    }
}