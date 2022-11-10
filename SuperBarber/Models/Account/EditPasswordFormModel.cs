using System.ComponentModel.DataAnnotations;
using static SuperBarber.Data.DataConstraints;

namespace SuperBarber.Models.Account
{
    public class EditPasswordFormModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        [StringLength(PasswordMaxLength, MinimumLength = EmailMinLength)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required]
        [Compare(nameof(NewPassword))]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmNewPassword { get; set; }
    }
}
