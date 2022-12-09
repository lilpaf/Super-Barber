#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SuperBarber.Infrastructure.Data.Models;
using static SuperBarber.Infrastructure.Data.DataConstraints;

namespace SuperBarber.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        //private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
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
            [RegularExpression(@"^\+(\d{3})+[ |-]?(\d{1,})+[ |-]?(\d{3})+[ |-]?(\d{4})+$")]
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


        public IActionResult OnGet()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Email = Input.Email,
                    UserName = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    PhoneNumber = Input.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
