using SuperBarber.Models.Home;

namespace SuperBarber.Models.BarberShop
{
    public class AllBarberShopQueryModel
    {
        public string City { get; init; }

        public IEnumerable<string> Cities { get; init; }

        //public string District { get; init; }

        //public IEnumerable<string> Districts { get; init; }

        public string SearchTerm { get; init; }

        public BatberShopSorting Sorting { get; init; }

        public IEnumerable<BarberShopListingViewModel> BarberShops { get; init; }
    }
}
