using Microsoft.EntityFrameworkCore;
using SuperBarber.Infrastructure.Data;
using SuperBarber.Infrastructure.Data.Models;
using SuperBarber.Core.Models.Cart;
using SuperBarber.Core.Models.Service;
using static SuperBarber.Core.Services.Common.TimeCheck;
using SuperBarber.Core.Extensions;

namespace SuperBarber.Core.Services.Cart
{
    public class CartService : ICartService
    {
        private readonly SuperBarberDbContext data;

        public CartService(SuperBarberDbContext data)
            => this.data = data;

        public async Task<Infrastructure.Data.Models.Service> GetServiceAsync(int serviceId)
                => await this.data.Services
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == serviceId && !s.IsDeleted);

        private bool BarberShopHasService(int barberShopId, int serviceId)
                => this.data.BarberShops
                   .Any(b => b.Id == barberShopId && !b.IsDeleted
                   && b.Services.Any(s => s.ServiceId == serviceId && !s.Service.IsDeleted));

        public async Task<decimal> BarberShopServicePriceAsync(int barberShopId, int serviceId)
        {
            if (!BarberShopHasService(barberShopId, serviceId))
            {
                throw new ModelStateCustomException("", "This barbershop dose not contain this service");
            }

            var servicePrice = await this.data.BarberShops.Where(b => b.Id == barberShopId)
                .Select(b => b.Services.First(s => s.ServiceId == serviceId).Price).FirstAsync();

            return servicePrice;
        }


        public async Task AddOrderAsync(BookServiceFormModel model, List<ServiceListingViewModel> cartList, string userId)
        {
            var newCartList = new List<ServiceListingViewModel>(cartList);

            foreach (var item in cartList)
            {
                DateTime dateParsed;

                var dateParsedResult = DateTime.TryParse(model.Date, out dateParsed);

                if (!dateParsedResult)
                {
                    throw new ModelStateCustomException("", "Date input is incorect.", newCartList);
                }

                if (!CheckIfTimeIsCorrect(model.Time))
                {
                    throw new ModelStateCustomException("", "Hour input is incorect.", newCartList);
                }

                var timeArr = model.Time.Split(':');

                var ts = new TimeSpan(int.Parse(timeArr[0]), int.Parse(timeArr[1]), 0);

                dateParsed = dateParsed.Date + ts;

                var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .ThenInclude(b => b.Barber)
                .ThenInclude(o => o.Orders)
                .FirstOrDefaultAsync(bs => bs.Id == item.BarberShopId
                && bs.IsPublic
                && !bs.IsDeleted
                && bs.StartHour <= ts
                && bs.FinishHour >= ts);

                if (barberShop == null)
                {
                    throw new ModelStateCustomException("",
                        $"Your desired book hour for {item.ServiceName} at {item.BarberShopName} is out of the working time of {item.BarberShopName}. If you had any previous orders in your cart and they are gone they are already proccesed you can find all the orders that you have made in the menu 'My orders'. Remove {item.ServiceName} at {item.BarberShopName} from the cart to continue.",
                        newCartList);
                }

                if (!BarberShopHasService(barberShop.Id, item.ServiceId))
                {
                    throw new ModelStateCustomException("",
                        $"{item.BarberShopName} dose not contain service {item.ServiceName}. If you had any previous orders in your cart and they are gone they are already proccesed you can find all the orders that you have made in the menu 'My orders'.",
                        newCartList);
                }

                var minimumBookDate = DateTime.UtcNow.AddMinutes(15);

                if (dateParsed.ToUniversalTime() < minimumBookDate)
                {
                    throw new ModelStateCustomException("",
                        $"Your desired book hour must be at least at {minimumBookDate.ToLocalTime().Hour}:{minimumBookDate.ToLocalTime().Minute}.  If you had any previous orders in your cart and they are gone they are already proccesed you can find all the orders that you have made in the menu 'My orders'.",
                        newCartList);
                }

                var barber = barberShop.Barbers
                    .Where(b => b.Barber.Orders.All(o => o.Date != dateParsed.ToUniversalTime())
                            && b.IsAvailable
                            && !b.Barber.IsDeleted)
                    .FirstOrDefault();

                if (barber == null)
                {
                    throw new ModelStateCustomException("",
                        $"There are no avalible barbers at {barberShop.Name} for the desired time right now. If you had any previous orders in your cart and they are gone they are already proccesed you can find all the orders that you have made in the menu 'My orders'. Remove {item.ServiceName} at {item.BarberShopName} from the cart to continue.",
                        newCartList);
                }

                var order = new SuperBarber.Infrastructure.Data.Models.Order
                {
                    BarberShopId = barberShop.Id,
                    BarberId = barber.BarberId,
                    Date = dateParsed.ToUniversalTime(),
                    ServiceId = item.ServiceId,
                    UserId = userId,
                    IsDeleted = false,
                    DeleteDate = null,
                    Price = item.Price
                };

                await this.data.Orders.AddAsync(order);

                newCartList.Remove(item);

                await this.data.SaveChangesAsync();
            }
        }

        public async Task<string> GetBarberShopNameAsync(int id)
            => await this.data.BarberShops.Where(bs => bs.Id == id && !bs.IsDeleted).Select(bs => bs.Name).FirstOrDefaultAsync();

        public string GetBarberShopNameToFriendlyUrl(string name)
            => name.Replace(' ', '-');
    }
}
