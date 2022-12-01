using SuperBarber.Models.Barber;
using SuperBarber.Models.Interfaces;

namespace SuperBarber.Models.BarberShop
{
    public class ManageBarberShopViewModel : IBarberShopModel
    {
        public int BarberShopId { get; init; }

        public string BarberShopName { get; init; }

        public IEnumerable<BarberViewModel> Barbers { get; init; }
    }
}
