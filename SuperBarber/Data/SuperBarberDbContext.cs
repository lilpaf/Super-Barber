using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuperBarber.Data.Models;

namespace SuperBarber.Data
{
    public class SuperBarberDbContext : IdentityDbContext
    {
        public SuperBarberDbContext(DbContextOptions<SuperBarberDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Barber> Barbers { get; set; }
        public DbSet<BarberShop> BarberShops { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<UserOrder> UserOrders { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<District> Districts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserOrder>()
                .HasKey(x => new { x.UserId, x.OrderId });

            modelBuilder.Entity<BarberShopServices>()
                .HasKey(x => new { x.ServiceId, x.BarberShopId });

            modelBuilder.Entity<Order>()
                .HasOne<BarberShop>()
                .WithMany(x => x.Orders)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
