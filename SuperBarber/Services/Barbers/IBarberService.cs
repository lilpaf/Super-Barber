namespace SuperBarber.Services.Barbers
{
    public interface IBarberService
    {
        Task AddBarberAsync(string userId);

        Task AsignBarberToBarberShopAsync(int barberShopId, string userId);

        Task UnasignBarberFromBarberShopAsync(int barberShopId, string? userId, int? barberId);

        Task<string> GetBarberShopNameToFriendlyUrlAsync(int id);

        Task AddOwnerToBarberShop(int barberShopId, int barberId);
        
        Task RemoveOwnerFromBarberShop(int barberShopId, int barberId);
    }
}
