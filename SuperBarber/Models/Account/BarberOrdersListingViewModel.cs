namespace SuperBarber.Models.Account
{
    public class BarberOrdersListingViewModel
    {
        public string ClientFirstName { get; init; }
        
        public string ClientLastName { get; init; }

        public string ServiceName { get; init; }

        public decimal Price { get; init; }

        public string Date { get; init; }
    }
}
