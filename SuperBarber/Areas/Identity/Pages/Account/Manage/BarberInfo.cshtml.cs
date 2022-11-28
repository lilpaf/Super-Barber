using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SuperBarber.Areas.Identity.Services.Account;
using SuperBarber.Data.Models;
using SuperBarber.Infrastructure;
using SuperBarber.Models.Account;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace SuperBarber.Areas.Identity.Pages.Account.Manage
{
    public class BarberInfoModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IAccountService _accountService;

        public BarberInfoModel(
            UserManager<User> userManager,
            IAccountService accountService)
        {
            _userManager = userManager;
            _accountService = accountService;
        }

        public List<BarberOrdersListingViewModel> BarberOrders { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user.");
            }
            if (User.IsInRole(CustomRoles.BarberRoleName))
            {
                BarberOrders = (List<BarberOrdersListingViewModel>)await _accountService.GetBarberOrdersAsync(user);

                return Page();
            }

            ModelState.AddModelError(string.Empty, "User in not a barber.");

            return RedirectToPage("PersonalData");
        }
    }
}
