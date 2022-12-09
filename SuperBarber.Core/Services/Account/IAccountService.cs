using SuperBarber.Infrastructure.Data.Models;

namespace SuperBarber.Core.Services.Account
{
    public interface IAccountService
    {
        Task SetFirstAndLastNameAsync(User user, string firstName, string lastName);

        Task DeleteBarberAsync(User user, bool userIsDeleted, string wwwRootPath);

        Task DeleteUserAsync(User user);

        Task UpdateBarberEmailAsync(User user, string email);

        Task UpdateBarberPhoneNumberAsync(User user, string phoneNumber);
    }
}
