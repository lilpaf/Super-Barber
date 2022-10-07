using System.ComponentModel.DataAnnotations;
using static SuperBarber.Data.DataConstraints;

namespace SuperBarber.Models
{
    public class AddBarberShopFormModel
    {
        [Required]
        [StringLength(
            ShopNameMaxLength, 
            MinimumLength = DefaultMinLength,
            ErrorMessage = "{0} should have a length between {2} and {1}"
            )]
        public string Name { get; init; }
        
        [Required]
        [StringLength(
             AddressMaxLength,
             MinimumLength = DefaultMinLength,
             ErrorMessage = "{0} should have a length between {2} and {1}"
             )]
        public string City { get; init; }

        [Required]
        [StringLength(
            AddressMaxLength,
            MinimumLength = DefaultMinLength,
            ErrorMessage = "{0} should have a length between {2} and {1}"
            )]
        public string District { get; init; }

        [Required]
        [Display(Name = "Street name and number")]
        [StringLength(
            AddressMaxLength,
            MinimumLength = DefaultMinLength,
            ErrorMessage = "{0} should have a length between {2} and {1}"
            )]
        public string Street { get; init; }

        [Required]
        [Display(Name = "Image URL")]
        [Url]
        public string? ImageUrl { get; init; }
    }
}
