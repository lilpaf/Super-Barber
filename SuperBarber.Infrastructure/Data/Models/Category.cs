using System.ComponentModel.DataAnnotations;
using static SuperBarber.Infrastructure.Data.DataConstraints;

namespace SuperBarber.Infrastructure.Data.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(CategoryNameMaxLength)]
        public string Name { get; set; }
    }
}
