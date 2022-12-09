using SuperBarber.Core.Models.Cart;
using SuperBarber.Core.Models.Service;

namespace SuperBarber.Core.Services.Cart
{
    public interface ICartService
    {
        Task<Infrastructure.Data.Models.Service> GetServiceAsync(int serviceId);

        Task<decimal> BarberShopServicePriceAsync(int barberShopId, int serviceId);

        Task AddOrderAsync(BookServiceFormModel model, List<ServiceListingViewModel> cartList, string userId);

        Task<string> GetBarberShopNameAsync(int id);

        string GetBarberShopNameToFriendlyUrl(string name);
    }
}
