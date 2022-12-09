using SuperBarber.Core.Models.Barber;
using SuperBarber.Core.Models.Interfaces;

namespace SuperBarber.Core.Models.BarberShop
{
    public class ManageBarberShopViewModel : IBarberShopModel
    {
        public int BarberShopId { get; init; }

        public string BarberShopName { get; init; }

        public IEnumerable<BarberViewModel> Barbers { get; init; }
    }
}
