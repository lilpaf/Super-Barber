using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static SuperBarber.Infrastructure.Data.DataConstraints;

namespace SuperBarber.Infrastructure.Data.Models
{
    public class Barber
    {
        public int Id { get; set; }
        
        [MaxLength(FirstNameMaxLength)]
        public string FirstName { get; set; }
        
        [MaxLength(LastNameMaxLength)]
        public string LastName { get; set; }

        [MaxLength(PhoneNumberMaxLength)]
        public string PhoneNumber { get; set; }

        [MaxLength(EmailMaxLength)]
        public string Email { get; set; }

        [Required]
        public string UserId { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeleteDate { get; set; }

        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();

        public ICollection<BarberShopBarbers> BarberShops { get; set; } = new HashSet<BarberShopBarbers>();
    }
}
