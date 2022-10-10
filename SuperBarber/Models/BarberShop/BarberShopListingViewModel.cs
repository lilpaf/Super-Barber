using System.ComponentModel.DataAnnotations;

namespace SuperBarber.Models.BarberShop
{
    public class BarberShopListingViewModel
    {
        public Guid Id { get; init; }

        public string Name { get; init; }

        public string City { get; init; }
        
        public string District { get; init; }

        public string Street { get; init; }

        public string StartHour { get; init; }

        public string FinishHour { get; init; }

        public string ImageUrl { get; set; }
    }
}
