﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Infrastructure;
using System.Threading;
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

        public async Task AddBarberAsync(string userId)
        {
            if (this.data.Barbers.Any(b => b.UserId == userId))
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
            };

            await userManager.AddToRoleAsync(user, BarberRoleName);

            await data.Barbers.AddAsync(barber);

            await data.SaveChangesAsync();

            await signInManager.RefreshSignInAsync(user);
        }

        public async Task AsignBarberToBarberShopAsync(int barberShopId, string userId)
        {
            var barberShop = await this.data.BarberShops.FindAsync(barberShopId);

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

        public async Task UnasignBarberFromBarberShopAsync(int barberShopId, int barberId, string userId)
        {
            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .ThenInclude(b => b.Barber)
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId);

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
            .FirstOrDefaultAsync(b => b.Id == barberId && b.BarberShops.Any(bs => bs.BarberShopId == barberShop.Id));

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
                data.Orders.RemoveRange(orders);
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

        public async Task<bool> CheckIfUserIsTheBabrerToFire(string userId, int barberId)
        {
            var barber = await this.data.Barbers
            .FirstOrDefaultAsync(b => b.Id == barberId);

            if (barber == null)
            {
                return false;
            }

            return barber.UserId.Equals(userId);
        }

        public async Task AddOwnerToBarberShop(int barberShopId, int barberId, string userId)
        {
            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .ThenInclude(b => b.Barber)
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId);

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
                .FirstOrDefaultAsync(b => b.Id == barberId && b.BarberShops.Any(bs => bs.BarberShopId == barberShop.Id));

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

        public async Task RemoveOwnerFromBarberShop(int barberShopId, int barberId, string userId)
        {
            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .ThenInclude(b => b.Barber)
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId);

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
                .FirstOrDefaultAsync(b => b.Id == barberId && b.BarberShops.Any(bs => bs.BarberShopId == barberShop.Id));

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

        public async Task MakeBarberUnavailableFromBarberShop(int barberShopId, int barberId, string userId)
        {
            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .ThenInclude(b => b.Barber)
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            var barber = await this.data.Barbers
                .FirstOrDefaultAsync(b => b.Id == barberId && b.BarberShops.Any(bs => bs.BarberShopId == barberShop.Id));

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

        public async Task MakeBarberAvailableFromBarberShop(int barberShopId, int barberId, string userId)
        {
            var barberShop = await this.data.BarberShops
                .Include(bs => bs.Barbers)
                .ThenInclude(b => b.Barber)
                .FirstOrDefaultAsync(bs => bs.Id == barberShopId);

            if (barberShop == null)
            {
                throw new ModelStateCustomException("", "This barbershop does not exist");
            }

            var barber = await this.data.Barbers
                .FirstOrDefaultAsync(b => b.Id == barberId && b.BarberShops.Any(bs => bs.BarberShopId == barberShop.Id));

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
    }
}
