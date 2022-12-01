using SuperBarber.Models.Interfaces;

namespace SuperBarber.Models.Service
{
    public class ServiceListingViewModel : IBarberShopModel
    {
        public int ServiceId { get; init; }
        
        public int BarberShopId { get; init; }
        
        public string BarberShopName { get; init; }

        public string ServiceName { get; init; }
        
        public decimal Price { get; init; }

        public string Category { get; init; }
    }
}
