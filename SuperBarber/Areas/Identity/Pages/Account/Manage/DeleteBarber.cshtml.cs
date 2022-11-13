using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SuperBarber.Areas.Identity.Services.Account;
using SuperBarber.Data.Models;

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
            
            if (User.IsInRole(CustomRoles.BarberRoleName))
            {
                ModelState.AddModelError(string.Empty, "User in not a barber.");

                return RedirectToPage("PersonalData");
            }

            await _accountService.DeleteBarberAsync(user);

            return RedirectToPage("PersonalData");
        }
    }
}
