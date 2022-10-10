using System.ComponentModel.DataAnnotations;
using static SuperBarber.Data.DataConstraints;

namespace SuperBarber.Data.Models
{
    public class BarberShop
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(ShopNameMaxLength)]
        public string Name { get; set; }
        
        public int CityId { get; set; }
        public City City { get; set; }

        public int DistrictId { get; set; }
        public District District { get; set; }

        [Required]
        [MaxLength(AddressMaxLength)]
        public string Street { get; set; }

        public TimeSpan StartHour { get; set; }

        public TimeSpan FinishHour { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public ICollection<BarberShopServices> Services { get; set; } = new HashSet<BarberShopServices>();
        
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
        
        public ICollection<Barber> Barbers { get; set; } = new HashSet<Barber>();
    }
}