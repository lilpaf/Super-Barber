using Microsoft.AspNetCore.Identity;
using SuperBarber.Data.Models;
using SuperBarber.Data;
using Microsoft.EntityFrameworkCore;
using static SuperBarber.Infrastructure.CustomRoles;
using SuperBarber.Infrastructure;

namespace SuperBarber.Areas.Identity.Services.Account
{
    public class AccountService : IAccountService
    {
        private readonly SuperBarberDbContext _data;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;


        public AccountService(SuperBarberDbContext data, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _data = data;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task SetFirstAndLastNameAsync(User user, string firstName, string lastName)
        {
            if (firstName != user.FirstName)
            {
                user.FirstName = firstName;

                _data.Users.Update(user);

                await _data.SaveChangesAsync();
            }
            else if (lastName != user.LastName)
            {
                user.LastName = lastName;

                _data.Users.Update(user);

                await _data.SaveChangesAsync();
            }
        }

        public async Task DeleteBarberAsync(User user)
        {
            var barber = await _data.Barbers.FirstAsync(b => b.UserId == user.Id);

            var orders = await _data.Orders.Where(o => o.BarberId == barber.Id).ToListAsync();

            if (orders.Any())
            {
                _data.Orders.RemoveRange(orders);
            }

            var barberShop = await _data.BarberShops
                .FirstOrDefaultAsync(bs => bs.Barbers.Any(b => b.BarberId == barber.Id));

            if (barberShop != null && barberShop.Barbers.Where(b => b.IsOwner).Count() > 1)
            {
                throw new ModelStateCustomException("", "You have to transfer first the ownership of the barbershop that you own or delete it");
            }
            
            if (barberShop != null && barberShop.Barbers.Count == 1)
            {
                await _userManager.RemoveFromRoleAsync(user, BarberShopOwnerRoleName);

                _data.BarberShops.Remove(barberShop);
            }

            await _userManager.RemoveFromRoleAsync(user, BarberRoleName);

            _data.Barbers.Remove(barber);

            await _data.SaveChangesAsync();

            await _signInManager.RefreshSignInAsync(user);
        }
    }
}
