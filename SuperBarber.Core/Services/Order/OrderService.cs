using Microsoft.EntityFrameworkCore;
using SuperBarber.Core.Extensions;
using SuperBarber.Core.Models.Order;
using SuperBarber.Infrastructure.Data;
using static SuperBarber.Core.Extensions.ExeptionErrors;
using static SuperBarber.Core.Extensions.ExeptionErrors.OrderServiceErrors;

namespace SuperBarber.Core.Services.Order
{
    public class OrderService : IOrderService
    {
        private readonly SuperBarberDbContext data;

        public OrderService(SuperBarberDbContext data)
            => this.data = data;

        public async Task RemoveOrderAsync(string orderId, int barberId, string userId)
        {
            var order = await this.data.Orders
                .Include(o => o.Barber)
                .FirstOrDefaultAsync(o => o.Id.ToString() == orderId);

            if (order == null)
            {
                throw new ModelStateCustomException("", OrderNonExistent);
            }

            if (order.BarberId != barberId || order.Barber.UserId != userId)
            {
                throw new ModelStateCustomException("", UserIsNotTheAssignedBarber);
            }

            if (DateTime.UtcNow > order.Date || order.IsDeleted)
            {
                throw new ModelStateCustomException("", CancelOrderCanNoLongerBeDone);
            }

            order.IsDeleted = true;

            order.DeleteDate = DateTime.UtcNow;

            await this.data.SaveChangesAsync();
        }
        
        public async Task RemoveYourOrderAsync(string orderId, string userId)
        {
            var order = await this.data.Orders
                .Include(o => o.Barber)
                .FirstOrDefaultAsync(o => o.Id.ToString() == orderId);

            if (order == null)
            {
                throw new ModelStateCustomException("", OrderNonExistent);
            }

            if (order.UserId != userId)
            {
                throw new ModelStateCustomException("", UserDidNotMakeTheOrder);
            }

            if (DateTime.UtcNow.AddMinutes(30) > order.Date || order.IsDeleted)
            {
                throw new ModelStateCustomException("", CancelOrderCanNoLongerBeDone);
            }

            order.IsDeleted = true;

            order.DeleteDate = DateTime.UtcNow;

            await this.data.SaveChangesAsync();
        }

        public async Task<OrderViewModel> GetMyOrdersAsync(string userId, int currentPage)
        {
            var userOrders = await this.data.Orders
               .Where(o => o.UserId == userId)
               .OrderByDescending(o => o.Date)
               .Select(o => new OrdersListingViewModel
               {
                   OrderId = o.Id.ToString(),
                   BarberShop = o.BarberShop.Name,
                   BarberFirstName = o.Barber.FirstName,
                   BarberLastName = o.Barber.LastName,
                   BarberPhoneNumber = o.Barber.PhoneNumber,
                   BarberEmail = o.Barber.Email,
                   ServiceName = o.Service.Name,
                   Price = o.Price,
                   Date = o.Date,
                   IsDeleted = o.IsDeleted
               })
               .ToListAsync();

            var totalUserOrders = userOrders.Count;

            var userOrdersPaged = userOrders
                    .Skip((currentPage - 1) * OrderViewModel.OrdersPerPage)
                    .Take(OrderViewModel.OrdersPerPage)
                    .ToList();

            return new OrderViewModel
            {
                Orders = userOrdersPaged,
                CurrentPage = currentPage,
                TotalOrders = totalUserOrders
            };
        }
    }
}
