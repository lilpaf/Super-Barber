using SuperBarber.Models.Barber;

namespace SuperBarber.Services.Barbers
{
    public interface IBarberService
    {
        Task AddBarberAsync(string userId);
    }
}
