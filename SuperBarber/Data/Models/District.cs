using System.ComponentModel.DataAnnotations;
using static SuperBarber.Data.DataConstraints;

namespace SuperBarber.Data.Models
{
    public class District
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(AddressMaxLength)]
        public string Name { get; set; }
    }
}
