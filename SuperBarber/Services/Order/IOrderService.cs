using SuperBarber.Models.Order;

namespace SuperBarber.Services.Order
{
    public interface IOrderService
    {
        Task RemoveOrder(string orderId, int barberId, string userId);

        Task<IEnumerable<OrdersListingViewModel>> GetMyOrdersAsync(string userId);

        Task RemoveYourOrder(string orderId, string userId);
    }
}
