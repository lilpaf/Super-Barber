using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBarber.Data.Models
{
    public class Order
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public int BarberShopId { get; set; }

        [ForeignKey(nameof(BarberShopId))]
        public BarberShop BarberShop { get; set; }

        public int BarberId { get; set; }

        [ForeignKey(nameof(BarberId))]
        public Barber Barber { get; set; }

        public DateTime Date { get; set; }

        public int ServiceId { get; set; }
        
        [ForeignKey(nameof(ServiceId))]
        public Service Service { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeleteDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}