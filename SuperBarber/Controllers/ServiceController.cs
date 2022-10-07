using Microsoft.AspNetCore.Mvc;
using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Models;

namespace SuperBarber.Controllers
{
    public class ServiceController : Controller
    {
        private readonly SuperBarberDbContext data;

        public ServiceController(SuperBarberDbContext data)
        => this.data = data;

        public IActionResult Add() => View(new AddServiceFormModel
        {
            Categories = this.GetServiceCategories()
        });

        [HttpPost]
        public IActionResult Add(AddServiceFormModel service)
        {
            if (!this.data.Categories.Any(c => c.Id == service.CategoryId))
            {
                this.ModelState.AddModelError(nameof(service.CategoryId), "Category does not exist");
            } 
            
            if (this.data.Services.Any(s => s.Name == service.Name && s.CategoryId == service.CategoryId))
            {
                this.ModelState.AddModelError(nameof(service.Name), "This service already exists");
            }

            if(!ModelState.IsValid)
            {
                service.Categories = this.GetServiceCategories();

                return View(service);
            }

            var serviceData = new Service
            {
                Name = service.Name,
                Price = service.Price,
                CategoryId = service.CategoryId
            };

            this.data.Services.Add(serviceData);

            this.data.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        private IEnumerable<ServiceCategoryViewModel> GetServiceCategories()
            => this.data
            .Categories
            .Select(c => new ServiceCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToList();
    }
}
