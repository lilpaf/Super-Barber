using SuperBarber.Models.Service;

namespace SuperBarber.Services.Service
{
    public interface IServiceService
    {
        Task<IEnumerable<ServiceCategoryViewModel>> GetServiceCategoriesAsync();

        Task AddServiceAsync(AddServiceFormModel model, string userId);

        Task<IEnumerable<ServiceListingViewModel>> ShowServicesAsync(int barberShopId);
    }
}
