namespace SuperBarber.Models.BarberShop
{
    public class AllBarberShopQueryModel
    {
        public const int BarberShopsPerPage = 3;

        public string City { get; init; }

        public IEnumerable<string> Cities { get; init; }

        //public string District { get; init; }

        //public IEnumerable<string> Districts { get; init; }

        public string SearchTerm { get; init; }

        public int CurrentPage { get; init; } = 1;

        public int TotalBarberShops { get; set; }

        public BarberShopSorting Sorting { get; init; }
        
        public IEnumerable<BarberShopListingViewModel> BarberShops { get; init; }
    }
}
