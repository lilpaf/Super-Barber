using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static SuperBarber.Data.DataConstraints;

namespace SuperBarber.Data.Models
{
    public class Barber
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(FirstNameMaxLength)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(LastNameMaxLength)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(PhoneNumberMaxLength)]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(EmailMaxLength)]
        public string Email { get; set; }

        public int? BarberShopId { get; set; }

        [ForeignKey(nameof(BarberShopId))]
        public BarberShop? BarberShop { get; set; }

        [Required]
        public string UserId { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
