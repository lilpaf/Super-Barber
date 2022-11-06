using Microsoft.AspNetCore.Identity;
using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Services.CutomException;
using static SuperBarber.CustomRoles;

namespace SuperBarber.Services.Barbers
{
    public class BarberService : IBarberService
    {
        private readonly SuperBarberDbContext data;
        private readonly UserManager<User> userManager;

        public BarberService(SuperBarberDbContext data, UserManager<User> userManager)
        {
            this.data = data;
            this.userManager = userManager;
        }

        public async Task AddBarberAsync(string userId)
        {
            if (this.data.Barbers.Any(b => b.UserId == userId))
            {
                throw new ModelStateCustomException("", "User is already a barber");
            }

            var user = await this.data.Users.FindAsync(userId);

            var barber = new Barber
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                UserId = userId,
            };

            await userManager.AddToRoleAsync(user, BarberRoleName);

            await data.Barbers.AddAsync(barber);

            await data.SaveChangesAsync();
        }
    }
}
