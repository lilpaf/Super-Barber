using System.ComponentModel.DataAnnotations;

namespace SuperBarber.Data.Models
{
    public class CartItem
    {
        [Key]
        public string ItemId { get; set; }

        public string CartId { get; set; }

        public int Quantity { get; set; }

        public DateTime DateCreated { get; set; }

        public int ServiceId { get; set; }

        public virtual Service Service { get; set; }
    }
}
