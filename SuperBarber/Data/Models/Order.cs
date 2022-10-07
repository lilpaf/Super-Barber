using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBarber.Data.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BarberShopId { get; set; }

        public Guid BarberId { get; set; }

        public DateTime Date { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; }
    }
}