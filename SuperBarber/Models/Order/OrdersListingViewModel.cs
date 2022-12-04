namespace SuperBarber.Models.Order
{
    public class OrdersListingViewModel
    {
        public string OrderId { get; init; }
        
        public string BarberShop { get; init; }

        public int? BarberId { get; init; }

        public string? ClientFirstName { get; init; }
        
        public string? BarberFirstName { get; init; }

        public string? ClientLastName { get; init; }
        
        public string? BarberLastName { get; init; }
        
        public string? ClientPhoneNumber { get; init; }
        
        public string? BarberPhoneNumber { get; init; }
        
        public string? ClientEmail { get; init; }
        
        public string? BarberEmail { get; init; }

        public string ServiceName { get; init; }

        public decimal Price { get; init; }

        public DateTime Date { get; init; }

        public bool IsDeleted { get; init; }
    }
}
