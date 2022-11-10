using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperBarber.Infrastructure;
using SuperBarber.Models.Account;
using SuperBarber.Services.Account;
using static SuperBarber.CustomRoles;
using SuperBarber.Services.CutomException;

namespace SuperBarber.Controllers
{
    [Authorize]
    public class Account : Controller
    {
        private readonly IAccountService accountService;

        public Account(IAccountService accountService) 
            => this.accountService = accountService;

        public async Task<IActionResult> Manage()
        {
            var userId = User.Id();

            var names = await accountService.GetUserNamesAsync(userId);

            var firstName = names.Item1;
            var lastName = names.Item2;

            return View(new EditNameFormModel
            {
                FirstName = firstName,
                LastName = lastName
            });
        }

        [HttpPost]
        public async Task<IActionResult> Manage(EditNameFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Id();

            await accountService.EditUserNamesAsync(userId, model);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ChangeEmail()
        {
            var userId = User.Id();

            var email = await accountService.GetUserEmailAsync(userId);

            return View(new EditEmailFormModel
            {
                Email = email,
                ConfirmEmail = email
            });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeEmail(EditEmailFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Id();

            await accountService.EditEmailAsync(userId, model);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult ChangePassword() => View();

        [HttpPost]
        public async Task<IActionResult> ChangePassword(EditPasswordFormModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                var userId = User.Id();

                await accountService.EditPasswordAsync(userId, model);

                return RedirectToAction("Index", "Home");
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                return View();
            }
        }

        [Authorize(Roles = BarberRoleName)]
        public async Task<IActionResult> Barber()
        {
            var userId = User.Id();

            return View(await accountService.GetBarberOrdersAsync(userId));
        } 
        
        public IActionResult PersonalData()
            => View();
        
        public async Task<IActionResult> Delete()
        {
            var userId = User.Id();

            await accountService.DeleteAccountAsync(userId);
            
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = BarberRoleName)]
        public async Task<IActionResult> DeleteBarber()
        {
            var userId = User.Id();

            await accountService.DeleteBarberAsync(userId);

            return RedirectToAction(nameof(PersonalData));
        } 
    }
}
