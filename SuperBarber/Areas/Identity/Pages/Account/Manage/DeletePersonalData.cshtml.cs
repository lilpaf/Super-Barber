// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

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
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly IAccountService _accountService;

        public DeletePersonalDataModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IAccountService accountService,
            ILogger<DeletePersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _accountService = accountService;
            _logger = logger;
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

                if (User.IsBarber())
                {
                    await _accountService.DeleteBarberAsync(user, true);
                }

                await _accountService.DeleteUserAsync(user);

                /*if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Unexpected error occurred deleting user.");
                }*/

                await _signInManager.SignOutAsync();

                var userId = await _userManager.GetUserIdAsync(user);

                _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

                return RedirectToAction("Index", "Home", new { area = "" });
            }
            catch (ModelStateCustomException ex)
            {
                ModelState.AddModelError(ex.Key, ex.Message);

                return Page();
            }
        } 
    }
}
