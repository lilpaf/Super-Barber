using Microsoft.EntityFrameworkCore.Design;
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

        public async Task AddBarber(string userId, string userEmail, AddBarberFormModel model)
        {
            if (this.data.Barbers.Any(b => b.UserId == userId))
            {
                throw new ModelStateCustomException(nameof(model.FullName), "User is already a barber");
            }

            var barber = new Barber
            {
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Email = userEmail,
                UserId = userId,
            };

            await data.Barbers.AddAsync(barber);

            await data.SaveChangesAsync();
        }
    }
}
