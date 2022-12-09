using SuperBarber.Core.Models.BarberShop;

namespace SuperBarber.Core.Services.BarberShops
{
    public interface IBarberShopService
    {
        Task<AllBarberShopQueryModel> AllBarberShopsAsync(AllBarberShopQueryModel query, string userId, List<BarberShopListingViewModel>? barberShops = null, bool publicOnly = true);

        Task AddBarberShopAsync(BarberShopAddFormModel model, string userId, string wwwRootPath);

        Task<MineBarberShopViewModel> MineBarberShopsAsync(string userId, int currentPage);

        Task EditBarberShopAsync(BarberShopEditFormModel model, int barberShopId, string userId, bool userIsAdmin, string wwwRootPath);

        Task DeleteBarberShopAsync(int barberShopId, string userId, bool userIsAdmin, string wwwRootPath);

        Task<BarberShopEditFormModel> DisplayBarberShopInfoAsync(int barberShopId);

        Task<ManageBarberShopViewModel> BarberShopInformationAsync(string userId, int barberShopId);

        Task<string> GetBarberShopNameToFriendlyUrlAsync(int id);

        Task MakeBarberShopPrivateAsync(int barberShopId);

        Task MakeBarberShopPublicAsync(int barberShopId);
    }
}
