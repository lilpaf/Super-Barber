using Microsoft.AspNetCore.Mvc;
using SuperBarber.Models.BarberShop;

namespace SuperBarber.Services.BarberShops
{
    public interface IBarberShopService
    {
        Task<AllBarberShopQueryModel> AllBarberShopsAsync([FromQuery] AllBarberShopQueryModel query, List<BarberShopListingViewModel> barberShops = null);

        Task AddBarberShopAsync(BarberShopFormModel model, string userId);

        Task<IEnumerable<BarberShopListingViewModel>> MineBarberShopsAsync(string userId);

        Task EditBarberShopAsync(BarberShopFormModel model, int barberShopId, string userId, bool userIsAdmin);

        Task DeleteBarberShopAsync(int barberShopId, string userId, bool userIsAdmin);

        Task<BarberShopFormModel> DisplayBarberShopInfoAsync(int barberShopId);

        Task<ManageBarberShopViewModel> BarberShopInformationAsync(string userId, int barberShopId);
    }
}
