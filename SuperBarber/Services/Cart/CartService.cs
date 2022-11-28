using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Infrastructure;
using SuperBarber.Models.Cart;
using SuperBarber.Models.Service;

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
                .FirstOrDefaultAsync(s => s.Id == serviceId);

        public bool CheckBarberShopId(int barberShopId)
                => this.data.BarberShops
                   .Any(b => b.Id == barberShopId);


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
                && bs.StartHour < ts
                && bs.FinishHour > ts);


                if (barberShop == null)
                {
                    throw new ModelStateCustomException("", $"Your desired book hour for {item.ServiceName} in {item.BarberShopName} is out of the working time of the barbershop. Remove the item from the cart to continue.", newCartList);
                }

                var barber = barberShop.Barbers
                    .Where(b => b.Barber.Orders.All(o => o.Date != dateParsed) && b.IsAvailable)
                    .FirstOrDefault();

                if (barber == null)
                {
                    throw new ModelStateCustomException("", $"There are no avalible barbers at {barberShop.Name} for the desired time right now.", newCartList);
                }

                var order = new Order
                {
                    BarberShopId = barberShop.Id,
                    BarberId = barber.BarberId,
                    Date = dateParsed,
                    ServiceId = item.ServiceId,
                    UserId = userId
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

        private bool CheckIfTimeIsCorrect(string time)
        {
            var correctTimes = new string[]
            {
                "08:00", "08:30", "09:00", "09:30", "10:00", "10:30", "11:00", "11:30",
                "12:00", "12:30", "13:00", "13:30", "14:00", "14:30", "15:00", "15:30",
                "16:00", "16:30", "17:00", "17:30", "18:00", "18:30", "19:00", "19:30",
                "20:00", "20:30", "21:00", "21:30", "22:00", "22:30", "23:00"
            };

            return correctTimes.Any(t => t.Equals(time));
        }
    }
}
