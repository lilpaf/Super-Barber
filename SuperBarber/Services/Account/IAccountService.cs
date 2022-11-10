using SuperBarber.Models.Account;

namespace SuperBarber.Services.Account
{
    public interface IAccountService
    {
        Task EditUserNamesAsync(string userId, EditNameFormModel model);
        
        Task<Tuple<string, string>> GetUserNamesAsync(string userId);

        Task<string> GetUserEmailAsync(string userId);

        Task EditEmailAsync(string userId, EditEmailFormModel model);

        Task EditPasswordAsync(string userId, EditPasswordFormModel model);

        Task<IEnumerable<BarberOrdersListingViewModel>> GetBarberOrdersAsync(string userId);

        Task DeleteAccountAsync(string userId);

        Task DeleteBarberAsync(string userId);
    }
}
