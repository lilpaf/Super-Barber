using Microsoft.AspNetCore.Mvc;
using SuperBarber.Models.BarberShop;

namespace SuperBarber.Services.BarberShops
{
    public interface IBarberShopService
    {
        Task<AllBarberShopQueryModel> AllBarberShopsAsync([FromQuery] AllBarberShopQueryModel query, List<BarberShopListingViewModel> barberShops = null);

        Task AddBarberShopAsync(AddBarberShopFormModel model);
    }
}
