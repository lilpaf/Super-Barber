using SuperBarber.Core.Models.Order;

namespace SuperBarber.Core.Services.Barbers
{
    public interface IBarberService
    {
        Task AddBarberAsync(string userId);

        Task AsignBarberToBarberShopAsync(int barberShopId, string userId);

        Task UnasignBarberFromBarberShopAsync(int barberShopId, int barberId, string userId);

        Task AddOwnerToBarberShopAsync(int barberShopId, int barberId, string userId);

        Task RemoveOwnerFromBarberShopAsync(int barberShopId, int barberId, string userId);

        Task MakeBarberAvailableAtBarberShopAsync(int barberShopId, int barberId, string userId);

        Task MakeBarberUnavailableAtBarberShopAsync(int barberShopId, int barberId, string userId);

        Task<bool> CheckIfUserIsTheBabrerToFire(string userId, int barberId);

        Task<OrderViewModel> GetBarberOrdersAsync(string userId, int currentPage);

        Task<string> GetBarberShopNameToFriendlyUrlAsync(int id);

        Task<string> GetBarberNameAsync(int id);
    }
}
