using System.ComponentModel.DataAnnotations;
using static SuperBarber.Data.DataConstraints;

namespace SuperBarber.Models.Service
{
    public class AddServiceFormModel
    {
        [Required]
        [StringLength(
            ServiceNameMaxLength,
            MinimumLength = DefaultMinLength,
            ErrorMessage = "The name of the service should have a length between {2} and {1}"
            )]
        public string Name { get; init; }

        [Range(
            PriceMinRange,
            PriceMaxRange,
            ErrorMessage = "Price should be in the range of {1} to {2} and can have only 2 nubmers after the decimal point")]
        public decimal Price { get; init; }

        [Display(Name = "Category")]
        public int CategoryId { get; init; }

        public IEnumerable<ServiceCategoryViewModel> Categories { get; set; }
    }
}
