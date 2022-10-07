using System.ComponentModel.DataAnnotations;
using static SuperBarber.Data.DataConstraints;

namespace SuperBarber.Data.Models
{
    public class Barber
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(FullNameMaxLength)]
        public string FullName { get; set; }

        [Required]
        public string Password { get; set; }

        public int PhoneNumber { get; set; }

        [Required]
        [MaxLength(EmailMaxLength)]
        public string Email { get; set; }

        public Guid BarberShopId { get; set; }
        public BarberShop BarberShop { get; set; }

        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
