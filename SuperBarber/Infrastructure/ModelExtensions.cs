using SuperBarber.Models.BarberShop;

namespace SuperBarber.Infrastructure
{
    public static class ModelExtensions
    {
        public static string ToFriendlyUrl(this BarberShopListingViewModel barberShop)
            => barberShop.Name.Replace(' ', '-');
    }
}
