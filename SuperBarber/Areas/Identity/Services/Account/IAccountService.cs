using SuperBarber.Data.Models;

namespace SuperBarber.Areas.Identity.Services.Account
{
    public interface IAccountService
    {
        Task SetFirstAndLastNameAsync(User user, string firstName, string lastName);

        Task DeleteBarberAsync(User user);
    }
}
