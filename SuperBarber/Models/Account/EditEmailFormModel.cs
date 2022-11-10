using System.ComponentModel.DataAnnotations;
using static SuperBarber.Data.DataConstraints;

namespace SuperBarber.Models.Account
{
    public class EditEmailFormModel
    {
        [Required]
        [StringLength(EmailMaxLength, MinimumLength = EmailMinLength)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Compare(nameof(Email))]
        [Display(Name = "Confirm email")]
        public string ConfirmEmail { get; set; }
    }
}
