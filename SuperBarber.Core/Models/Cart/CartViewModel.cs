using SuperBarber.Core.Models.Service;

namespace SuperBarber.Core.Models.Cart
{
    public class CartViewModel
    {
        public decimal TotalPrice { get; init; }

        public IEnumerable<ServiceListingViewModel> Services { get; init; }
    }
}
