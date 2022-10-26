using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Models.Service;

namespace SuperBarber.Controllers
{
    [Authorize]
    public class ServiceController : Controller
    {
        private readonly SuperBarberDbContext data;

        public ServiceController(SuperBarberDbContext data)
        => this.data = data;

        public async Task<IActionResult> Add() => View(new AddServiceFormModel
        {
            Categories = await this.GetServiceCategoriesAsync()
        });

        [HttpPost]
        public async Task<IActionResult> Add(AddServiceFormModel model)
        {
            if (!this.data.Categories.Any(c => c.Id == model.CategoryId))
            {
                this.ModelState.AddModelError(nameof(model.CategoryId), "Category does not exist");
            } 
            
            if (this.data.Services.Any(s => s.Name.ToLower().Trim() == model.Name.ToLower().Trim() && s.CategoryId == model.CategoryId))
            {
                this.ModelState.AddModelError(nameof(model.Name), "This service already exists");
            }

            if(!ModelState.IsValid)
            {
                model.Categories = await this.GetServiceCategoriesAsync();

                return View(model);
            }

            var service = new Service
            {
                Name = model.Name,
                Price = model.Price,
                CategoryId = model.CategoryId
            };

            await this.data.Services.AddAsync(service);

            await this.data.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        private async Task<IEnumerable<ServiceCategoryViewModel>> GetServiceCategoriesAsync()
            => await this.data
            .Categories
            .Select(c => new ServiceCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name
            })
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
