using SuperBarber.Models.Service;

namespace SuperBarber.Services.Service
{
    public interface IServiceService
    {
        Task<IEnumerable<ServiceCategoryViewModel>> GetServiceCategoriesAsync();

        Task AddServiceAsync(AddServiceFormModel model, string userId, int barberShopId);

        Task<IEnumerable<ServiceListingViewModel>> ShowServicesAsync(int barberShopId);

        Task RemoveServiceAsync(int barberShopId, int serviceId, string userId);
    }
}
