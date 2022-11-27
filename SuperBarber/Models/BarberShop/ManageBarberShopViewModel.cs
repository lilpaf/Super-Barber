using SuperBarber.Models.Barbers;

namespace SuperBarber.Models.BarberShop
{
    public class ManageBarberShopViewModel
    {
        public int BarberShopId { get; set; }

        public string BarberShopName { get; set; }

        public IEnumerable<BarberViewModel> Barbers { get; set; }
    }
}
