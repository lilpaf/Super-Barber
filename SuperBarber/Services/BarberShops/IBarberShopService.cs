using Microsoft.AspNetCore.Mvc;
using SuperBarber.Models.BarberShop;

namespace SuperBarber.Services.BarberShops
{
    public interface IBarberShopService
    {
        Task<AllBarberShopQueryModel> AllBarberShopsAsync([FromQuery] AllBarberShopQueryModel query, string userId, List<BarberShopListingViewModel>? barberShops = null, bool publicOnly = true);

        Task AddBarberShopAsync(BarberShopFormModel model, string userId);

        Task<MineBarberShopViewModel> MineBarberShopsAsync(string userId, int currentPage);

        Task EditBarberShopAsync(BarberShopFormModel model, int barberShopId, string userId, bool userIsAdmin);

        Task DeleteBarberShopAsync(int barberShopId, string userId, bool userIsAdmin);

        Task<BarberShopFormModel> DisplayBarberShopInfoAsync(int barberShopId);

        Task<ManageBarberShopViewModel> BarberShopInformationAsync(string userId, int barberShopId);

        Task<string> GetBarberShopNameToFriendlyUrlAsync(int id);

        Task MakeBarberShopPrivateAsync(int barberShopId);

        Task MakeBarberShopPublicAsync(int barberShopId);
    }
}
