using System.ComponentModel.DataAnnotations;
using static SuperBarber.Infrastructure.Data.DataConstraints;

namespace SuperBarber.Core.Models.User
{
    public class UserRegisterFormModel
    {
        [Required]
        [Display(Name = "First name")]
        [StringLength(FirstNameMaxLength, MinimumLength = NameMinLength)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        [StringLength(LastNameMaxLength, MinimumLength = NameMinLength)]
        public string LastName { get; set; }

        [Required]
        [StringLength(EmailMaxLength, MinimumLength = EmailMinLength)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Phone number")]
        [RegularExpression(@"^\+?\d+[ |-]?\d+[ |-]?\d{3}[ |-]?\d{4}$")]
        [StringLength(PhoneNumberMaxLength, MinimumLength = PhoneNumberMinLength)]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(PasswordMaxLength, MinimumLength = EmailMinLength)]
        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password))]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}
