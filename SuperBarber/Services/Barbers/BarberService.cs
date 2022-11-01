using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Models.Barber;
using SuperBarber.Services.CutomException;

namespace SuperBarber.Services.Barbers
{
    public class BarberService : IBarberService
    {
        private readonly SuperBarberDbContext data;

        public BarberService(SuperBarberDbContext data)
            => this.data = data;

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

            await data.Barbers.AddAsync(barber);

            await data.SaveChangesAsync();
        }
    }
}
