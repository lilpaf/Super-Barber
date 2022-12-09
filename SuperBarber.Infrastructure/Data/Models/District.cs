using System.ComponentModel.DataAnnotations;
using static SuperBarber.Infrastructure.Data.DataConstraints;

namespace SuperBarber.Infrastructure.Data.Models
{
    public class District
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(AddressMaxLength)]
        public string Name { get; set; }
    }
}
