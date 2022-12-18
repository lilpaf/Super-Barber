using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperBarber.Core.Extensions;
using SuperBarber.Core.Models.Cart;
using SuperBarber.Core.Models.Service;
using SuperBarber.Core.Services.Cart;
using SuperBarber.Extensions;
using static SuperBarber.Extensions.SessionExtensions;
using static SuperBarber.Extensions.WebConstants;
using static SuperBarber.Extensions.WebConstants.CartControllerConstants;

namespace SuperBarber.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService cartService;

        public CartController(ICartService cartService)
            => this.cartService = cartService;

        [RestoreModelStateFromTempData]
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

        [HttpPost]
        public async Task<IActionResult> Add(int barbershopId, int serviceId)
        {
            List<ServiceListingViewModel> cartList;

            var service = await cartService.GetServiceAsync(serviceId);

            var barberShopName = await cartService.GetBarberShopNameAsync(barbershopId);

            if (service == null || barberShopName == null)
            {
                SetTempDataModelStateExtension.SetTempData(this, "", InvalidServiceOrBarbershop);

                return RedirectToAction("All", "BarberShop");
            }

            decimal servicePrice;

            try
            {
                servicePrice = await cartService.BarberShopServicePriceAsync(barbershopId, serviceId);
            }
            catch (ModelStateCustomException ex)
            {
                SetTempDataModelStateExtension.SetTempData(this, ex.Key, ex.Message);

                return RedirectToAction("All", "BarberShop");
            }

            if (HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName) != null
            && HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName).ToList().Any())
            {
                cartList = HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName).ToList();

                cartList.Add(new ServiceListingViewModel
                {
                    ServiceId = service.Id,
                    BarberShopId = barbershopId,
                    BarberShopName = barberShopName,
                    Price = servicePrice,
                    ServiceName = service.Name,
                    Category = service.Category.Name
                });

                HttpContext.Session.Set<List<ServiceListingViewModel>>(SessionName, cartList);

                TempData[GlobalMessageKey] = ItemAddedSuccsessMessege;

                return RedirectToAction("All", "Service", new { barbershopId, information = cartService.GetBarberShopNameToFriendlyUrl(barberShopName) });
            }

            cartList = new List<ServiceListingViewModel>
            {
                new ServiceListingViewModel
                {
                    ServiceId = service.Id,
                    BarberShopId = barbershopId,
                    BarberShopName = barberShopName,
                    Price = servicePrice,
                    ServiceName = service.Name,
                    Category = service.Category.Name
                }
            };

            HttpContext.Session.Set(SessionName, cartList);

            TempData[GlobalMessageKey] = ItemAddedSuccsessMessege;

            return RedirectToAction("All", "Service", new { barbershopId, information = cartService.GetBarberShopNameToFriendlyUrl(barberShopName) });
        }

        [HttpPost]
        public IActionResult Remove(int serviceid)
        {
            if (HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName) != null
                && HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName).ToList().Any())
            {

                var cartList = HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName).ToList();

                var service = cartList.FirstOrDefault(s => s.ServiceId == serviceid);

                if (service == null)
                {
                    SetTempDataModelStateExtension.SetTempData(this, "", InvalidService);

                    return RedirectToAction("All", "Cart");
                }

                cartList.Remove(service);

                HttpContext.Session.Set<List<ServiceListingViewModel>>(SessionName, cartList);

                TempData[GlobalMessageKey] = ItemRemovedSuccsessMessege;
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

                    TempData[GlobalMessageKey] = OrderBookedSuccsessMessege;
                }

                return RedirectToAction("All", "BarberShop");
            }
            catch (ModelStateCustomException ex)
            {
                this.ModelState.AddModelError(ex.Key, ex.Message);

                HttpContext.Session.Set<List<ServiceListingViewModel>>(SessionName, ex.CartList);

                return View(model);
            }
        }
    }
}
