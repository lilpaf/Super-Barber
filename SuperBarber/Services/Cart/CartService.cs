﻿using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Models.Cart;
using SuperBarber.Models.Service;
using SuperBarber.Services.CutomException;

namespace SuperBarber.Services.Cart
{
    public class CartService : ICartService
    {
        private readonly SuperBarberDbContext data;

        public CartService(SuperBarberDbContext data)
            => this.data = data;

        public async Task<Data.Models.Service> GetServiceAsync(int serviceId)
                => await this.data.Services
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

        public bool CheckBarberShopId(int barberShopId)
                => this.data.BarberShops
                   .Any(b => b.Id == barberShopId);


        public async Task AddOrderAsync(BookServiceFormModel model, List<ServiceListingViewModel> cartList, string userId)
        {
            var dateParsed = DateTime.Parse(model.Date);

            var timeArr = model.Time.Split(':');

            var ts = new TimeSpan(int.Parse(timeArr[0]), int.Parse(timeArr[1]), 0);

            dateParsed = dateParsed.Date + ts;

            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .FirstOrDefaultAsync(bs => bs.Id == cartList.First().BarberShopId
                && bs.StartHour < ts
                && bs.FinishHour > ts
                && bs.Barbers.Any(b => !b.Orders.Any(o => o.Date == dateParsed)));

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "There are no avalible barbers for the desired time right now.");
            }


            var barber = barberShop.Barbers
                .Where(b => !b.Orders.Any(o => o.Date == dateParsed))
                .FirstOrDefault();

            if (barber == null)
            {
                throw new ModelStateCustomException("", "There are no avalible barbers for the desired time right now.");
            }

            List<Order> orders = new List<Order>();

            foreach (var item in cartList)
            {
                var order = new Order
                {
                    BarberShopId = barberShop.Id,
                    BarberId = barber.Id,
                    Date = dateParsed,
                    ServiceId = item.ServiceId,
                    UserId = userId
                };

                orders.Add(order);
            }

            await this.data.Orders.AddRangeAsync(orders);

            await this.data.SaveChangesAsync();
        }

    }
}