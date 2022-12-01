using SuperBarber.Models.BarberShop;
using SuperBarber.Models.Interfaces;
using SuperBarber.Models.Service;

namespace SuperBarber.Infrastructure
{
    public static class ModelExtensions
    { 
        public static string ToFriendlyUrl(this IBarberShopModel barberShop)
            => barberShop.BarberShopName.Replace(' ', '-');
    }
}
