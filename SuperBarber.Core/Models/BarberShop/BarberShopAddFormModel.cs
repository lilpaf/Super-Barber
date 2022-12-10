using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using static SuperBarber.Infrastructure.Data.DataConstraints;

namespace SuperBarber.Core.Models.BarberShop
{
    public class BarberShopAddFormModel
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
        public string StartHour { get; init; }

        [Required]
        public string FinishHour { get; init; }

        [Display(Name = "Image")]
        public IFormFile ImageFile { get; init; }
    }
}
