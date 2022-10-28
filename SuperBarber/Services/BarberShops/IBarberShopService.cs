using Microsoft.AspNetCore.Mvc;
using SuperBarber.Models.BarberShop;

namespace SuperBarber.Services.BarberShops
{
    public interface IBarberShopService
    {
        Task<AllBarberShopQueryModel> AllBarberShops([FromQuery] AllBarberShopQueryModel query, List<BarberShopListingViewModel> barberShops = null);

        Task AddBarberShop(AddBarberShopFormModel model);
    }
}
