using SuperBarber.Models.Service;

namespace SuperBarber.Models.Cart
{
    public class CartViewModel
    {
        public decimal TotalPrice { get; init; }

        public IEnumerable<ServiceListingViewModel> Services { get; init; }
    }
}
