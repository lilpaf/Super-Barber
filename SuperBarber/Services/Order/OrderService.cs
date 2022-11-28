using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Infrastructure;

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
    }
}
