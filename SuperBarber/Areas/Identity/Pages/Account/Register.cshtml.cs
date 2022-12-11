#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SuperBarber.Core.Services.Mail;
using SuperBarber.Infrastructure.Data.Models;
using static SuperBarber.Infrastructure.Data.DataConstraints;
using static SuperBarber.Extensions.WebConstants;

namespace SuperBarber.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IMailService _emailSender;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IMailService emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
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

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm Your Email Address",
                        $"<h1>Confirm Your Email Address</h1>Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        TempData[GlobalMessageKey] = $"Welcome to SuperBarber, {Input.FirstName}! Before we get started, please check your email to confirm your account.";

                        return RedirectToAction("Index", "Home", new { area = "" });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        return RedirectToAction("Index", "Home", new { area = "" });
                    }
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
