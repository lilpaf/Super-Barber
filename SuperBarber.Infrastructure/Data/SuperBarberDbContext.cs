using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuperBarber.Infrastructure.Data.Models;

namespace SuperBarber.Infrastructure.Data
{
    public class SuperBarberDbContext : IdentityDbContext<User>
    {
        public SuperBarberDbContext(DbContextOptions<SuperBarberDbContext> options) : base(options)
        {
        }

        public DbSet<Barber> Barbers { get; set; }
        public DbSet<BarberShop> BarberShops { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<District> Districts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BarberShopServices>()
                .HasKey(x => new { x.ServiceId, x.BarberShopId });
            
            modelBuilder.Entity<BarberShopBarbers>()
                .HasKey(x => new { x.BarberId, x.BarberShopId });

            modelBuilder.Entity<Order>()
                .HasOne(o => o.BarberShop)
                .WithMany(b => b.Orders)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Barber)
                .WithMany(b => b.Orders)
                .OnDelete(DeleteBehavior.Restrict);
             
            base.OnModelCreating(modelBuilder);
        }
    }
}
