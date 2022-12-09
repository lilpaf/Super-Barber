#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SuperBarber.Infrastructure.Data.Models;

namespace SuperBarber.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<User> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        //public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
            
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    ModelState.AddModelError(string.Empty, "Your account is locked out. Kindly wait for 5 minutes and try again.");
                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
