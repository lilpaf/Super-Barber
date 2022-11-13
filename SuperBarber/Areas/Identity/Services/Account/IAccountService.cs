using SuperBarber.Data.Models;
using SuperBarber.Models.Account;

namespace SuperBarber.Areas.Identity.Services.Account
{
    public interface IAccountService
    {
        Task SetFirstAndLastNameAsync(User user, string firstName, string lastName);

        Task DeleteBarberAsync(User user);

        Task<IEnumerable<BarberOrdersListingViewModel>> GetBarberOrdersAsync(User user);
    }
}
