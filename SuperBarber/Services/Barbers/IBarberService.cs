using SuperBarber.Models.Barber;

namespace SuperBarber.Services.Barbers
{
    public interface IBarberService
    {
        Task AddBarber(string userId, string userEmail, AddBarberFormModel model);
    }
}
