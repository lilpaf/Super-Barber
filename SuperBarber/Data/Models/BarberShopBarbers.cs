using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBarber.Data.Models
{
    public class BarberShopBarbers
    {
        public int BarberShopId { get; set; }

        [ForeignKey(nameof(BarberShopId))]
        public BarberShop BarberShop { get; set; }

        public int BarberId { get; set; }

        [ForeignKey(nameof(BarberId))]
        public Barber Barber { get; set; }

        public bool IsOwner { get; set; }
    }
}
