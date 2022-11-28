namespace SuperBarber.Services.Order
{
    public interface IOrderService
    {
        Task RemoveOrder(string orderId, int barberId, string userId);
    }
}
