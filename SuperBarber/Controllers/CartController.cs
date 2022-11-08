using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperBarber.Infrastructure;
using SuperBarber.Models.Cart;
using SuperBarber.Models.Service;
using SuperBarber.Services.Cart;
using SuperBarber.Services.CutomException;
using static SuperBarber.Infrastructure.SessionExtensions;

namespace SuperBarber.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService cartService;

        public CartController(ICartService cartService)
            => this.cartService = cartService;

        public IActionResult All()
        {
            if (HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName) != null)
            {
                var cartList = HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName).ToList();

                return View(new CartViewModel
                {
                    Services = cartList,
                    TotalPrice = cartList.Sum(s => s.Price)
                });
            }

            return View(new CartViewModel { Services = new List<ServiceListingViewModel>() });
        }

        public async Task<IActionResult> Add(int BarberShopId, int ServiceId)
        {
            List<ServiceListingViewModel> cartList;

            var service = await cartService.GetServiceAsync(ServiceId);

            var barberShop = cartService.CheckBarberShopId(BarberShopId);

            if (service == null || !barberShop)
            {
                this.ModelState.AddModelError("", "Invalid service or barbershop");

                return RedirectToAction("All", "Service", new { id = BarberShopId });
            }

            if (HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName) != null
            && HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName).ToList().Any())
            {
                cartList = HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName).ToList();

                cartList.Add(new ServiceListingViewModel
                {
                    ServiceId = service.Id,
                    BarberShopId = BarberShopId,
                    Price = service.Price,
                    Name = service.Name,
                    Category = service.Category.Name
                });

                HttpContext.Session.Set<List<ServiceListingViewModel>>(SessionName, cartList);

                return RedirectToAction("All", "Service", new { id = BarberShopId });
            }

            cartList = new List<ServiceListingViewModel>();

            cartList.Add(new ServiceListingViewModel
            {
                ServiceId = service.Id,
                BarberShopId = BarberShopId,
                Price = service.Price,
                Name = service.Name,
                Category = service.Category.Name
            });

            HttpContext.Session.Set<List<ServiceListingViewModel>>(SessionName, cartList);

            return RedirectToAction("All", "Service", new { id = BarberShopId });
        }

        public IActionResult Remove(int Id)
        {
            if (HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName) != null
                && HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName).ToList().Any())
            {

                var cartList = HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName).ToList();

                var service = cartList.FirstOrDefault(s => s.ServiceId == Id);

                if (service == null)
                {
                    this.ModelState.AddModelError("", "Invalid service");

                    return RedirectToAction("All", "Cart");
                }

                cartList.Remove(service);

                HttpContext.Session.Set<List<ServiceListingViewModel>>(SessionName, cartList);
            }

            return RedirectToAction("All", "Cart");
        }

        public IActionResult Book() => View();

        [HttpPost]
        public async Task<IActionResult> Book(BookServiceFormModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName) != null
                    && HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName).ToList().Any())
                {

                    var cartList = HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName).ToList();

                    var userId = User.Id();

                    await cartService.AddOrderAsync(model, cartList, userId);

                    cartList.RemoveRange(0, cartList.Count);

                    HttpContext.Session.Set<List<ServiceListingViewModel>>(SessionName, cartList);

                }

                return RedirectToAction("All", "BarberShop");
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                return View(model);
            }
        }
    }
}
