namespace SuperBarber.Models.BarberShop
{
    public class AllBarberShopQueryModel
    {
        //public const int BarberShopPerPage = 3;

        public string City { get; init; }

        public IEnumerable<string> Cities { get; init; }

        //public string District { get; init; }

        //public IEnumerable<string> Districts { get; init; }

        public string SearchTerm { get; init; }

        public BarberShopSorting Sorting { get; init; }
        
        public IEnumerable<BarberShopListingViewModel> BarberShops { get; init; }
    }
}
