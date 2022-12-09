using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBarber.Infrastructure.Data.Models
{
    public class BarberShopServices
    {
        public int BarberShopId { get; set; }

        [ForeignKey(nameof(BarberShopId))]
        public BarberShop BarberShop { get; set; }

        public int ServiceId { get; set; }

        [ForeignKey(nameof(ServiceId))]
        public Service Service { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
