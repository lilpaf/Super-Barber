using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SuperBarber.Core.Extensions;
using SuperBarber.Core.Services.Account;
using SuperBarber.Extensions;
using SuperBarber.Infrastructure.Data.Models;

namespace SuperBarber.Areas.Identity.Pages.Account.Manage
{
    public class DeleteBarberModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IAccountService _accountService;

        public DeleteBarberModel(
            UserManager<User> userManager,
            IAccountService accountService)
        {
            _userManager = userManager;
            _accountService = accountService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound("Unable to load user.");
                }

                RequirePassword = await _userManager.HasPasswordAsync(user);
                if (RequirePassword)
                {
                    if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                    {
                        ModelState.AddModelError(string.Empty, "Incorrect password.");
                        return Page();
                    }
                }

                if (!User.IsBarber())
                {
                    return NotFound("User in not a barber.");
                }

                await _accountService.DeleteBarberAsync(user, false);

                return RedirectToPage("PersonalData");
            }
            catch (ModelStateCustomException ex)
            {
                ModelState.AddModelError(ex.Key, ex.Message);

                return Page();
            }
            
        }
    }
}
