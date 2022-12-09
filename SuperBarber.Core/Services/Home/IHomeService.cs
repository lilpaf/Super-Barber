using SuperBarber.Core.Models.BarberShop;

namespace SuperBarber.Core.Services.Home
{
    public interface IHomeService
    {
        Task<IEnumerable<BarberShopListingViewModel>> SearchAvalibleBarbershopsAsync(string city, string district, string date, string time, string userId);

        Task<IEnumerable<string>> GetCitiesAsync();

        Task<IEnumerable<string>> GetDistrictsAsync();
    }
}
