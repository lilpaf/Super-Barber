using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Infrastructure;
using SuperBarber.Models.Cart;
using SuperBarber.Models.Service;
using static SuperBarber.Infrastructure.SessionExtensions;

namespace SuperBarber.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly SuperBarberDbContext data;

        public CartController(SuperBarberDbContext data)
        {
            this.data = data;
        }

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

        public IActionResult Add(int BarberShopId, int ServiceId)
        {
            List<ServiceListingViewModel> cartList;

            var service = this.data.Services
                .Include(s => s.Category)
                .FirstOrDefault(s => s.Id == ServiceId);

            if (service == null)
            {
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

                if (service != null)
                {
                    cartList.Remove(service);

                    HttpContext.Session.Set<List<ServiceListingViewModel>>(SessionName, cartList);
                }
            }

            return RedirectToAction("All", "Cart");

        }

        public IActionResult Book() => View();

        [HttpPost]
        public IActionResult Book(BookServiceFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName) != null
                && HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName).ToList().Any())
            {

                var cartList = HttpContext.Session.Get<List<ServiceListingViewModel>>(SessionName).ToList();

                var dateParsed = DateTime.Parse(model.Date);

                var timeArr = model.Time.Split(':');

                var ts = new TimeSpan(int.Parse(timeArr[0]), int.Parse(timeArr[1]), 0);

                dateParsed = dateParsed.Date + ts;

                var barberShop = this.data.BarberShops
                    .Include(bs => bs.Barbers)
                    .FirstOrDefault(bs => bs.Id == cartList.First().BarberShopId
                    && bs.StartHour < ts
                    && bs.FinishHour > ts
                    && bs.Barbers.Any(b => !b.Orders.Any(o => o.Date == dateParsed)));

                if (barberShop == null)
                {
                    return RedirectToAction("All", "Cart"); // break
                }


                var barber = barberShop.Barbers
                    .Where(b => !b.Orders.Any(o => o.Date == dateParsed))
                    .First();

                List<Order> orders = new List<Order>();

                foreach (var item in cartList)
                {
                    var order = new Order
                    {
                        BarberShopId = barberShop.Id,
                        BarberId = barber.Id,
                        Date = dateParsed,
                        ServiceId = item.ServiceId,
                        UserId = User.Id()
                    };

                    orders.Add(order);
                }

                this.data.Orders.AddRange(orders);

                this.data.SaveChanges();

                cartList.RemoveRange(0, cartList.Count);

                HttpContext.Session.Set<List<ServiceListingViewModel>>(SessionName, cartList);

            }

            return RedirectToAction("All", "BarberShop");

        }
    }
}
