using SuperBarber.Models.BarberShop;
using SuperBarber.Models.Service;

namespace SuperBarber.Infrastructure
{
    public static class ModelExtensions
    {
        public static string ToFriendlyUrl(this BarberShopListingViewModel barberShop)
            => barberShop.Name.Replace(' ', '-');
        
        public static string ToFriendlyUrl(this ManageBarberShopViewModel barberShop)
            => barberShop.BarberShopName.Replace(' ', '-');
        
        public static string ToFriendlyUrl(this ServiceListingViewModel service)
            => service.BarberShopName.Replace(' ', '-');
    }
}
