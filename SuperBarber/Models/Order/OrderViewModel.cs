namespace SuperBarber.Models.Order
{
    public class OrderViewModel
    {
        public const int OrdersPerPage = 15;

        public int CurrentPage { get; init; } = 1;

        public int TotalOrders { get; set; }

        public List<OrdersListingViewModel> Orders { get; init; }
    }
}
