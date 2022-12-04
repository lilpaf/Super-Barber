using SuperBarber.Models.Order;

namespace SuperBarber.Services.Order
{
    public interface IOrderService
    {
        Task RemoveOrder(string orderId, int barberId, string userId);

        Task<OrderViewModel> GetMyOrdersAsync(string userId, int currentPage);

        Task RemoveYourOrder(string orderId, string userId);
    }
}
