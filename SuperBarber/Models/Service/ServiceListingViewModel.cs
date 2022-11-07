namespace SuperBarber.Models.Service
{
    public class ServiceListingViewModel
    {
        public int ServiceId { get; init; }
        
        public int BarberShopId { get; init; }

        public string Name { get; init; }
        
        public decimal Price { get; init; }

        public string Category { get; init; }
    }
}
