using Microsoft.AspNetCore.Identity;
using SuperBarber.Data.Models;
using SuperBarber.Data;
using SuperBarber.Models.Account;
using SuperBarber.Services.CutomException;
using Microsoft.EntityFrameworkCore;
using static SuperBarber.CustomRoles;

namespace SuperBarber.Services.Account
{
    public class AccountService : IAccountService
    {
        private readonly SuperBarberDbContext data;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public AccountService(SuperBarberDbContext data, 
            UserManager<User> userManager, 
            SignInManager<User> signInManager)
        {
            this.data = data;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task<Tuple<string, string>> GetUserNamesAsync (string userId)
        { 
            var user = await userManager.FindByIdAsync(userId);

            var firstName = user.FirstName;
            var lastName = user.LastName;

            return Tuple.Create(firstName, lastName);
        }
        
        public async Task EditUserNamesAsync (string userId, EditNameFormModel model)
        { 
            var user = await userManager.FindByIdAsync(userId);

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            this.data.Users.Update(user);

            await this.data.SaveChangesAsync();
        }

        public async Task<string> GetUserEmailAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            return user.Email;
        }

        public async Task EditEmailAsync(string userId, EditEmailFormModel model)
        {
            var user = await userManager.FindByIdAsync(userId);

            user.Email = model.Email;
            
            this.data.Users.Update(user);

            await this.data.SaveChangesAsync();
        }
        
        public async Task EditPasswordAsync(string userId, EditPasswordFormModel model)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (await userManager.CheckPasswordAsync(user, model.CurrentPassword))
            {
                await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                this.data.Users.Update(user);

                await this.data.SaveChangesAsync();
            }
            else
            {
                throw new ModelStateCustomException(nameof(model.CurrentPassword), "Wrong password");
            }
        }

        public async Task<IEnumerable<BarberOrdersListingViewModel>> GetBarberOrdersAsync(string userId)
            => await this.data.Orders
                .Where(o => o.Barber.UserId == userId)
                .OrderByDescending(o => o.Date)
                .Select(o => new BarberOrdersListingViewModel
                {
                    ClientFirstName = o.User.FirstName,
                    ClientLastName = o.User.LastName,
                    ServiceName = o.Service.Name,
                    Price = o.Service.Price,
                    Date = o.Date.ToString(@"MM/dd/yy H:mm")
                })
                .ToListAsync();

        public async Task DeleteAccountAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            await userManager.DeleteAsync(user);

            await this.data.SaveChangesAsync();

            await signInManager.SignOutAsync();
        }
        
        public async Task DeleteBarberAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            var barber = await this.data.Barbers.FirstAsync(b => b.UserId == user.Id);

            var orders = await this.data.Orders.Where(o => o.BarberId== barber.Id).ToListAsync();

            if (orders.Any())
            {
                this.data.Orders.RemoveRange(orders);
            }

            var barberShop = await this.data.BarberShops
                .FirstOrDefaultAsync(bs => bs.Barbers.Any(b => b.Id == barber.Id));

            if (barberShop != null && barberShop.Barbers.Count == 1)
            {
                await userManager.RemoveFromRoleAsync(user, BarberShopOwnerRoleName);

                this.data.BarberShops.Remove(barberShop);
            }

            await userManager.RemoveFromRoleAsync(user, BarberRoleName);

            this.data.Barbers.Remove(barber);

            await this.data.SaveChangesAsync();

            await signInManager.SignInAsync(user, isPersistent: false);
        }
    }
}
