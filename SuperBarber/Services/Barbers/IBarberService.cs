using SuperBarber.Models.Order;

namespace SuperBarber.Services.Barbers
{
    public interface IBarberService
    {
        Task AddBarberAsync(string userId);

        Task AsignBarberToBarberShopAsync(int barberShopId, string userId);

        Task UnasignBarberFromBarberShopAsync(int barberShopId, int barberId, string userId);

        Task AddOwnerToBarberShop(int barberShopId, int barberId, string userId);

        Task RemoveOwnerFromBarberShop(int barberShopId, int barberId, string userId);

        Task MakeBarberAvailableAtBarberShop(int barberShopId, int barberId, string userId);

        Task MakeBarberUnavailableAtBarberShop(int barberShopId, int barberId, string userId);

        Task<bool> CheckIfUserIsTheBabrerToFire(string userId, int barberId);

        Task<OrderViewModel> GetBarberOrdersAsync(string userId, int currentPage);

        Task<string> GetBarberShopNameToFriendlyUrlAsync(int id);

        Task<string> GetBarberNameAsync(int id);
    }
}
