using SuperBarber.Core.Models.Interfaces;

namespace SuperBarber.Extensions
{
    public static class ModelExtensions
    { 
        public static string ToFriendlyUrl(this IBarberShopModel barberShop)
            => barberShop.BarberShopName.Replace(' ', '-');
    }
}
