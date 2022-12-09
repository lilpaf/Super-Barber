using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SuperBarber.Core.Extensions;
using SuperBarber.Core.Services.BarberShops;
using SuperBarber.Infrastructure.Data;
using SuperBarber.Infrastructure.Data.Models;
using static SuperBarber.Core.Extensions.CustomRoles;

namespace SuperBarber.Core.Services.Account
{
    public class AccountService : IAccountService
    {
        private readonly SuperBarberDbContext _data;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IBarberShopService _barberShopService;


        public AccountService(SuperBarberDbContext data, UserManager<User> userManager, SignInManager<User> signInManager, IBarberShopService barberShopService)
        {
            _data = data;
            _userManager = userManager;
            _signInManager = signInManager;
            _barberShopService = barberShopService;
        }

        public async Task SetFirstAndLastNameAsync(User user, string firstName, string lastName)
        {
            var barber = await _data.Barbers.FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (firstName != user.FirstName)
            {
                user.FirstName = firstName;

                _data.Users.Update(user);

                if (barber != null)
                {
                    barber.FirstName = firstName;
                }

                await _data.SaveChangesAsync();
            }
            if (lastName != user.LastName)
            {
                user.LastName = lastName;

                _data.Users.Update(user);

                if (barber != null)
                {
                    barber.LastName = lastName;
                }

                await _data.SaveChangesAsync();
            }
        }

        public async Task DeleteBarberAsync(User user, bool userIsDeleted)
        {
            var barber = await _data.Barbers.FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (barber == null)
            {
                throw new ModelStateCustomException("", "This barber does not exist!");
            }

            var orders = await _data.Orders.Where(o => o.BarberId == barber.Id).ToListAsync();

            if (orders.Any())
            {
                orders.ForEach(o => o.IsDeleted = true);
                orders.ForEach(o => o.DeleteDate = DateTime.UtcNow);
            }

            var barberShopsOwned = await _data.BarberShops
                .Where(bs => bs.Barbers.Any(b => b.BarberId == barber.Id && b.IsOwner))
                .ToListAsync();

            // First check if the barber is owner of any barbershops and try to delete them.
            if (barberShopsOwned.Any())
            {
                foreach (var barberShop in barberShopsOwned)
                {
                    try
                    {
                        await _barberShopService.DeleteBarberShopAsync(barberShop.Id, user.Id, false);
                    }
                    catch (ModelStateCustomException ex)
                    {
                        throw ex;
                    }
                }
            }

            //Letter check if the barber is only employee at any barbershop
            if (_data.BarberShops.Any(bs => bs.Barbers.Any(b => b.BarberId == barber.Id)))
            {
                barber.BarberShops.Clear();
            }

            await _userManager.RemoveFromRoleAsync(user, BarberRoleName);

            if (userIsDeleted)
            {
                barber.FirstName = null;

                barber.LastName = null;

                barber.PhoneNumber = null;

                barber.Email = null;
            }

            barber.IsDeleted = true;

            barber.DeleteDate = DateTime.UtcNow;

            await _data.SaveChangesAsync();

            await _signInManager.RefreshSignInAsync(user);
        }

        public async Task DeleteUserAsync(User user)
        {
            var orders = await _data.Orders.Where(o => o.UserId == user.Id).ToListAsync();

            if (orders.Any())
            {
                orders.ForEach(o => o.IsDeleted = true);
                orders.ForEach(o => o.DeleteDate = DateTime.UtcNow);
            }

            user.FirstName = null;

            user.LastName = null;

            user.UserName = null;

            user.NormalizedUserName = null;

            user.Email = null;

            user.NormalizedEmail = null;

            user.PhoneNumber = null;

            user.PasswordHash = null;

            user.IsDeleted = true;

            user.DeleteDate = DateTime.UtcNow;

            await _data.SaveChangesAsync();
        }

        public async Task UpdateBarberEmailAsync(User user, string email)
        {
            var barber = await _data.Barbers.FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (barber != null)
            {
                barber.Email = email;

                await _data.SaveChangesAsync();
            }
        }

        public async Task UpdateBarberPhoneNumberAsync(User user, string phoneNumber)
        {
            var barber = await _data.Barbers.FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (barber != null)
            {
                barber.PhoneNumber = phoneNumber;

                await _data.SaveChangesAsync();
            }
        }
    }
}
