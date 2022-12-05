using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Infrastructure;
using SuperBarber.Models.Cart;
using SuperBarber.Models.Service;
using static SuperBarber.Services.Common.TimeCheck;

namespace SuperBarber.Services.Cart
{
    public class CartService : ICartService
    {
        private readonly SuperBarberDbContext data;

        public CartService(SuperBarberDbContext data)
            => this.data = data;

        public async Task<Data.Models.Service> GetServiceAsync(int serviceId)
                => await this.data.Services
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == serviceId && !s.IsDeleted);

        private bool BarberShopHasService(int barberShopId, int serviceId)
                => this.data.BarberShops
                   .Any(b => b.Id == barberShopId && !b.IsDeleted 
                   && b.Services.Any(s => s.ServiceId == serviceId && !s.Service.IsDeleted));

        public async Task<decimal> BarberShopServicePrice(int barberShopId, int serviceId)
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
                var dateParsed = DateTime.Parse(model.Date);

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
                    throw new ModelStateCustomException("", $"Your desired book hour for {item.ServiceName} at {item.BarberShopName} is out of the working time of {item.BarberShopName}. Remove {item.ServiceName} at {item.BarberShopName} from the cart to continue.", newCartList);
                }

                var minimumBookDate = DateTime.UtcNow.AddMinutes(15);

                if (dateParsed.ToUniversalTime() < minimumBookDate)
                {
                    throw new ModelStateCustomException("", $"Your desired book hour must be at least at {minimumBookDate.ToLocalTime().Hour}:{minimumBookDate.ToLocalTime().Minute}", newCartList);
                }

                var barber = barberShop.Barbers
                    .Where(b => b.Barber.Orders.All(o => o.Date != dateParsed.ToUniversalTime()) && b.IsAvailable)
                    .FirstOrDefault();

                if (barber == null)
                {
                    throw new ModelStateCustomException("", $"There are no avalible barbers at {barberShop.Name} for the desired time right now. Remove {item.ServiceName} at {item.BarberShopName} from the cart to continue.", newCartList);
                }

                var order = new Data.Models.Order
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
            => await this.data.BarberShops.Where(bs => bs.Id == id).Select(bs => bs.Name).FirstOrDefaultAsync(); 
        
        public string GetBarberShopNameToFriendlyUrl(string name)
            => name.Replace(' ', '-');
    }
}
