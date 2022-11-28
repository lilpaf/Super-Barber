using SuperBarber.Models.BarberShop;

namespace SuperBarber.Services.Home
{
    public interface IHomeService
    {
        Task<IEnumerable<BarberShopListingViewModel>> SearchAvalibleBarbershopAsync(string city, string district, string date, string time, string userId);

        Task<IEnumerable<string>> GetCitiesAsync();

        Task<IEnumerable<string>> GetDistrictsAsync();
    }
}
