namespace SuperBarber.Models.Barbers
{
    public class BarberOrdersListingViewModel
    {
        public string OrderId { get; init; }
        
        public int BarberId { get; init; }

        public string ClientFirstName { get; init; }

        public string ClientLastName { get; init; }

        public string ServiceName { get; init; }

        public decimal Price { get; init; }

        public DateTime Date { get; init; }
    }
}
