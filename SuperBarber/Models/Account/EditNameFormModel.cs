using System.ComponentModel.DataAnnotations;
using static SuperBarber.Data.DataConstraints;

namespace SuperBarber.Models.Account
{
    public class EditNameFormModel
    {
        [Required]
        [Display(Name = "First name")]
        [StringLength(FirstNameMaxLength, MinimumLength = NameMinLength)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        [StringLength(LastNameMaxLength, MinimumLength = NameMinLength)]
        public string LastName { get; set; }
    }
}
