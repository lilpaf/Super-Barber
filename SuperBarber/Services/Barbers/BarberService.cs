using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Infrastructure;
using SuperBarber.Models.Order;
using static SuperBarber.Infrastructure.CustomRoles;

namespace SuperBarber.Services.Barbers
{
    public class BarberService : IBarberService
    {
        private readonly SuperBarberDbContext data;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public BarberService(SuperBarberDbContext data,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            this.data = data;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        /// <summary>
        /// This method allows the user to become a barber and adds him to the role of barber
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ModelStateCustomException"></exception>

        public async Task AddBarberAsync(string userId)
        {
            if (this.data.Barbers.Any(b => b.UserId == userId && !b.IsDeleted))
            {
                throw new ModelStateCustomException("", "User is already a barber");
            }

            var user = await this.data.Users.FindAsync(userId);

            var barber = new Barber
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                UserId = userId,
                IsDeleted = false,
                DeleteDate = null
            };

            await userManager.AddToRoleAsync(user, BarberRoleName);

            var deletedBarber = await this.data.Barbers
                .FirstOrDefaultAsync(b => barber.UserId == b.UserId);

            if (deletedBarber != null)
            {
                deletedBarber.FirstName = barber.FirstName;
                deletedBarber.LastName = barber.LastName;
                deletedBarber.PhoneNumber = barber.PhoneNumber;
                deletedBarber.Email = barber.Email;
                deletedBarber.IsDeleted = barber.IsDeleted;
                deletedBarber.DeleteDate = barber.DeleteDate;
            }
            else
            {
                await data.Barbers.AddAsync(barber);
            }
            
            await data.SaveChangesAsync();

            await signInManager.RefreshSignInAsync(user);
        }

        /// <summary>
        /// This method allows the barber to assign himself as employee of a barbershop, 
        /// but he will not be takeing orders untill the shop owner makes him available.
        /// </summary>
        /// <param name="barberShopId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ModelStateCustomException"></exception>
        public async Task AsignBarberToBarberShopAsync(int barberShopId, string userId)
        {
            var barberShop = await this.data.BarberShops
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId && bs.IsPublic && !bs.IsDeleted);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            if (this.data.Barbers.Any(b => b.UserId == userId && b.BarberShops.Any(bs => bs.BarberShopId == barberShopId)))
            {
                throw new ModelStateCustomException("", "User is already asigned to this barbershop");
            }

            var barber = await this.data.Barbers.FirstOrDefaultAsync(b => b.UserId == userId);

            if (barber != null)
            {
                barber.BarberShops.Add(new BarberShopBarbers
                {
                    Barber = barber,
                    BarberShop = barberShop,
                    IsAvailable = false
                });

                await data.SaveChangesAsync();
            }
        }

        /// <summary>
        /// This method allows the shop owner to fire barbers from his shop and to resign as owner and barber from his shop.
        /// </summary>
        /// <param name="barberShopId"></param>
        /// <param name="barberId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ModelStateCustomException"></exception>
        public async Task UnasignBarberFromBarberShopAsync(int barberShopId, int barberId, string userId)
        {
            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .ThenInclude(b => b.Barber)
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId && !bs.IsDeleted);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            if (barberShop.Barbers.Any(b => b.Barber.UserId == userId && !b.IsOwner) ||
                !barberShop.Barbers.Any(b => b.Barber.UserId == userId))
            {
                throw new ModelStateCustomException("", $"You are not owner of {barberShop.Name}");
            }

            var barber = await this.data.Barbers
            .Include(b => b.BarberShops)
            .FirstOrDefaultAsync(b => b.Id == barberId && !b.IsDeleted && b.BarberShops.Any(bs => bs.BarberShopId == barberShop.Id));

            if (barber == null)
            {
                throw new ModelStateCustomException("", "This barber does not exist");
            }

            if (barberShop.Barbers.Count(b => b.IsOwner) == 1 && barberShop.Barbers.Any(b => b.BarberId == barber.Id && b.IsOwner))
            {
                throw new ModelStateCustomException("", "You are the only owner of this barbershop. If you want to unassign yoursef as barber from this barbershop you have to transfer the ownership to someone else from the barbershop manegment or delete the barbershop from the button 'Delete'.");
            }

            barber.BarberShops
               .Remove(barber.BarberShops
                       .Where(bs => bs.BarberShopId == barberShop.Id)
                       .First());

            var orders = await data.Orders.Where(o => o.BarberId == barber.Id).ToListAsync();

            if (orders.Any())
            {
                orders.ForEach(o => o.IsDeleted = true);
                orders.ForEach(o => o.DeleteDate = DateTime.UtcNow);
            }

            await data.SaveChangesAsync();

            var user = await this.data.Users.FirstAsync(u => u.Id == barber.UserId);

            if (!barber.BarberShops.Any(bs => bs.IsOwner))
            {
                await userManager.RemoveFromRoleAsync(user, BarberShopOwnerRoleName);

                if (userId == user.Id)
                {
                    await signInManager.RefreshSignInAsync(user);
                }
            }
        }

        /// <summary>
        /// Checks if the user is fireing a barber from his shop or he is resigning as owner and barber from his shop.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="barberId"></param>
        /// <returns></returns>
        public async Task<bool> CheckIfUserIsTheBabrerToFire(string userId, int barberId)
        {
            var barber = await this.data.Barbers
            .FirstOrDefaultAsync(b => b.Id == barberId && !b.IsDeleted);

            if (barber == null)
            {
                return false;
            }

            return barber.UserId.Equals(userId);
        }

        /// <summary>
        /// This method allows the owners of a barbershop to add employees barbers as owners of their barbershop.
        /// </summary>
        /// <param name="barberShopId"></param>
        /// <param name="barberId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ModelStateCustomException"></exception>
        public async Task AddOwnerToBarberShop(int barberShopId, int barberId, string userId)
        {
            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .ThenInclude(b => b.Barber)
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId && !bs.IsDeleted);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            if (barberShop.Barbers.Any(b => b.Barber.UserId == userId && !b.IsOwner) ||
                !barberShop.Barbers.Any(b => b.Barber.UserId == userId))
            {
                throw new ModelStateCustomException("", $"You are not owner of {barberShop.Name}");
            }

            var barber = await this.data.Barbers
                .Include(b => b.BarberShops)
                .FirstOrDefaultAsync(b => b.Id == barberId && !b.IsDeleted && b.BarberShops.Any(bs => bs.BarberShopId == barberShop.Id));

            if (barber == null)
            {
                throw new ModelStateCustomException("", "This barber does not exist");
            }

            if (barberShop.Barbers.Any(b => b.BarberId == barberId && b.IsOwner))
            {
                throw new ModelStateCustomException("", $"This barber is already owner of {barberShop.Name}");
            }

            var user = await this.data.Users.FirstAsync(u => u.Id == barber.UserId);

            if (!barber.BarberShops.Any(bs => bs.IsOwner))
            {
                await userManager.AddToRoleAsync(user, BarberShopOwnerRoleName);
            }

            var newOwner = barberShop.Barbers.Where(b => b.BarberId == barber.Id).First();

            newOwner.IsOwner = true;

            await this.data.SaveChangesAsync();
        }

        /// <summary>
        /// This method allows the owners to revoke the ownership of other owners. 
        /// </summary>
        /// <param name="barberShopId"></param>
        /// <param name="barberId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ModelStateCustomException"></exception>
        public async Task RemoveOwnerFromBarberShop(int barberShopId, int barberId, string userId)
        {
            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .ThenInclude(b => b.Barber)
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId && !bs.IsDeleted);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            if (barberShop.Barbers.Any(b => b.Barber.UserId == userId && !b.IsOwner) ||
                !barberShop.Barbers.Any(b => b.Barber.UserId == userId))
            {
                throw new ModelStateCustomException("", $"You are not owner of {barberShop.Name}");
            }

            if (barberShop.Barbers.Count(b => b.IsOwner) == 1)
            {
                throw new ModelStateCustomException("", "Every babrbershop has to have at least one owner.");
            }

            var barber = await this.data.Barbers
                .Include(b => b.BarberShops)
                .FirstOrDefaultAsync(b => b.Id == barberId && !b.IsDeleted && b.BarberShops.Any(bs => bs.BarberShopId == barberShop.Id));

            if (barber == null)
            {
                throw new ModelStateCustomException("", "This barber does not exist");
            }

            if (barber.UserId == userId)
            {
                throw new ModelStateCustomException("", $"Use the resign function next to your name in the {barberShop.Name} manegment menu");
            }

            if (barberShop.Barbers.Any(b => b.BarberId == barberId && !b.IsOwner))
            {
                throw new ModelStateCustomException("", $"This barber is not an owner of {barberShop.Name}");
            }

            var oldOwner = barberShop.Barbers.Where(b => b.BarberId == barber.Id).First();

            oldOwner.IsOwner = false;

            await this.data.SaveChangesAsync();

            var user = await this.data.Users.FirstAsync(u => u.Id == barber.UserId);

            if (!barber.BarberShops.Any(bs => bs.IsOwner))
            {
                await userManager.RemoveFromRoleAsync(user, BarberShopOwnerRoleName);
            }
        }

        /// <summary>
        /// This method allows the barbershop owners to make themself or other employees barbers unavailable. 
        /// </summary>
        /// <param name="barberShopId"></param>
        /// <param name="barberId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ModelStateCustomException"></exception>
        public async Task MakeBarberUnavailableAtBarberShop(int barberShopId, int barberId, string userId)
        {
            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .ThenInclude(b => b.Barber)
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId && !bs.IsDeleted);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            var barber = await this.data.Barbers
                .FirstOrDefaultAsync(b => b.Id == barberId && !b.IsDeleted && b.BarberShops.Any(bs => bs.BarberShopId == barberShop.Id));

            if (barber == null)
            {
                throw new ModelStateCustomException("", "This barber does not exist");
            }

            if (barberShop.Barbers.Any(b => b.Barber.UserId == userId && !b.IsOwner) ||
                !barberShop.Barbers.Any(b => b.Barber.UserId == userId))
            {
                throw new ModelStateCustomException("", $"You are not owner of {barberShop.Name}");
            }

            var unavailableBarber = barberShop.Barbers.Where(b => b.BarberId == barber.Id).First();

            unavailableBarber.IsAvailable = false;

            await this.data.SaveChangesAsync();
        }

        /// <summary>
        /// This method allows owners to make themself or other employees barbers avalible at their barbershop.
        /// </summary>
        /// <param name="barberShopId"></param>
        /// <param name="barberId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ModelStateCustomException"></exception>
        public async Task MakeBarberAvailableAtBarberShop(int barberShopId, int barberId, string userId)
        {
            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .ThenInclude(b => b.Barber)
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId && !bs.IsDeleted);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            var barber = await this.data.Barbers
                .FirstOrDefaultAsync(b => b.Id == barberId && !b.IsDeleted && b.BarberShops.Any(bs => bs.BarberShopId == barberShop.Id));

            if (barber == null)
            {
                throw new ModelStateCustomException("", "This barber does not exist");
            }

            if (barberShop.Barbers.Any(b => b.Barber.UserId == userId && !b.IsOwner) ||
                !barberShop.Barbers.Any(b => b.Barber.UserId == userId))
            {
                throw new ModelStateCustomException("", $"You are not owner of {barberShop.Name}");
            }

            var availableBarber = barberShop.Barbers.Where(b => b.BarberId == barber.Id).First();

            availableBarber.IsAvailable = true;

            await this.data.SaveChangesAsync();
        }

        /// <summary>
        /// This method gets all the orders that the user barber has.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<OrderViewModel> GetBarberOrdersAsync(string userId, int currentPage)
        {
            var barberOrders = await this.data.Orders
               .Where(o => o.Barber.UserId == userId)
               .OrderByDescending(o => o.Date)
               .Select(o => new OrdersListingViewModel
               {
                   OrderId = o.Id.ToString(),
                   BarberShop = o.BarberShop.Name,
                   BarberId = o.BarberId,
                   ClientFirstName = o.User.FirstName,
                   ClientLastName = o.User.LastName,
                   ClientPhoneNumber = o.User.PhoneNumber,
                   ClientEmail = o.User.Email,
                   ServiceName = o.Service.Name,
                   Price = o.Price,
                   Date = o.Date,
                   IsDeleted = o.IsDeleted
               })
               .ToListAsync();

            var totalBarberOrders = barberOrders.Count;

            var barberOrdersPaged = barberOrders
                    .Skip((currentPage - 1) * OrderViewModel.OrdersPerPage)
                    .Take(OrderViewModel.OrdersPerPage)
                    .ToList();

            return new OrderViewModel
            {
                Orders = barberOrdersPaged,
                CurrentPage = currentPage,
                TotalOrders = totalBarberOrders
            };
        }

        public async Task<string> GetBarberShopNameToFriendlyUrlAsync(int id)
            => await this.data.BarberShops.Where(bs => bs.Id == id && !bs.IsDeleted).Select(bs => bs.Name.Replace(' ', '-')).FirstOrDefaultAsync();

        public async Task<string> GetBarberNameAsync(int id)
            => await this.data.Barbers.Where(b => b.Id == id && !b.IsDeleted).Select(b => b.FirstName + ' ' + b.LastName).FirstOrDefaultAsync();
    }
}
