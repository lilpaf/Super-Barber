namespace SuperBarber.Core.Models.BarberShop
{
    public class MineBarberShopViewModel
    {
        public int CurrentPage { get; init; } = 1;

        public int TotalBarberShops { get; set; }

        public IEnumerable<BarberShopListingViewModel> BarberShops { get; init; }
    }
}
