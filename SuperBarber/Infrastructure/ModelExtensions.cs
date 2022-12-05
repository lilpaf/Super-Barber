using SuperBarber.Models.Interfaces;

namespace SuperBarber.Infrastructure
{
    public static class ModelExtensions
    { 
        public static string ToFriendlyUrl(this IBarberShopModel barberShop)
            => barberShop.BarberShopName.Replace(' ', '-');
    }
}
