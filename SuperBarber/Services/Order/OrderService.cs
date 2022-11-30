using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Infrastructure;
using SuperBarber.Models.Order;

namespace SuperBarber.Services.Order
{
    public class OrderService : IOrderService
    {
        private readonly SuperBarberDbContext data;

        public OrderService(SuperBarberDbContext data)
            => this.data = data;

        public async Task RemoveOrder(string orderId, int barberId, string userId)
        {
            var order = await this.data.Orders
                .Include(o => o.Barber)
                .FirstOrDefaultAsync(o => o.Id.ToString() == orderId);

            if (order == null)
            {
                throw new ModelStateCustomException("", "Invalid order");
            }

            if (order.BarberId != barberId || order.Barber.UserId != userId)
            {
                throw new ModelStateCustomException("", "You are not authorized to preform this action");
            }

            if (DateTime.UtcNow > order.Date)
            {
                throw new ModelStateCustomException("", "You can no longer cancel this order");
            }

            this.data.Orders.Remove(order);

            await this.data.SaveChangesAsync();
        }
        
        public async Task RemoveYourOrder(string orderId, string userId)
        {
            var order = await this.data.Orders
                .Include(o => o.Barber)
                .FirstOrDefaultAsync(o => o.Id.ToString() == orderId);

            if (order == null)
            {
                throw new ModelStateCustomException("", "Invalid order");
            }

            if (order.UserId != userId)
            {
                throw new ModelStateCustomException("", "You are not authorized to preform this action");
            }

            if (DateTime.UtcNow > order.Date)
            {
                throw new ModelStateCustomException("", "You can no longer cancel this order");
            }

            this.data.Orders.Remove(order);

            await this.data.SaveChangesAsync();
        }

        public async Task<IEnumerable<OrdersListingViewModel>> GetMyOrdersAsync(string userId)
           => await this.data.Orders
               .Where(o => o.Barber.UserId == userId)
               .OrderByDescending(o => o.Date)
               .Select(o => new OrdersListingViewModel
               {
                   OrderId = o.Id.ToString(),
                   BarberFirstName = o.User.FirstName,
                   BarberLastName = o.User.LastName,
                   ServiceName = o.Service.Name,
                   Price = o.Service.Price,
                   Date = o.Date
               })
               .ToListAsync();
    }
}
