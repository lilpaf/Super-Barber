using Microsoft.AspNetCore.Mvc;
using SuperBarber.Models.Service;

namespace SuperBarber.Services.Service
{
    public interface IServiceService
    {
        Task<IEnumerable<ServiceCategoryViewModel>> GetServiceCategoriesAsync();

        Task AddServiceAsync(AddServiceFormModel model);
    }
}
