using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static SuperBarber.Data.DataConstraints;

namespace SuperBarber.Data.Models
{
    public class BarberShop
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(ShopNameMaxLength)]
        public string Name { get; set; }
        
        public int CityId { get; set; }

        [ForeignKey(nameof(CityId))]
        public City City { get; set; }

        public int DistrictId { get; set; }

        [ForeignKey(nameof(DistrictId))]
        public District District { get; set; }

        [Required]
        [MaxLength(AddressMaxLength)]
        public string Street { get; set; }

        public TimeSpan StartHour { get; set; }

        public TimeSpan FinishHour { get; set; }

        [Required]
        public string ImageUrl { get; set; }
        
        public bool IsPublic { get; set; }
        
        public bool IsDeleted { get; set; }

        public DateTime? DeleteDate { get; set; }

        public ICollection<BarberShopServices> Services { get; set; } = new HashSet<BarberShopServices>();
        
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
        
        public ICollection<BarberShopBarbers> Barbers { get; set; } = new HashSet<BarberShopBarbers>(); 
    }
}