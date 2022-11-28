using SuperBarber.Models.Barbers;

namespace SuperBarber.Services.Barbers
{
    public interface IBarberService
    {
        Task AddBarberAsync(string userId);

        Task AsignBarberToBarberShopAsync(int barberShopId, string userId);

        Task UnasignBarberFromBarberShopAsync(int barberShopId, int barberId, string userId);

        Task AddOwnerToBarberShop(int barberShopId, int barberId, string userId);

        Task RemoveOwnerFromBarberShop(int barberShopId, int barberId, string userId);

        Task MakeBarberAvailableFromBarberShop(int barberShopId, int barberId, string userId);

        Task MakeBarberUnavailableFromBarberShop(int barberShopId, int barberId, string userId);

        Task<bool> CheckIfUserIsTheBabrerToFire(string userId, int barberId);

        Task<IEnumerable<BarberOrdersListingViewModel>> GetBarberOrdersAsync(string userId);
    }
}
