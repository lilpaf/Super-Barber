using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SuperBarber.Infrastructure.Data;
using SuperBarber.Infrastructure.Data.Models;
using static SuperBarber.Core.Extensions.CustomRoles;

namespace SuperBarber.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder PrepareDataBase(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            
            var services = serviceScope.ServiceProvider;
            
            var data = services.GetRequiredService<SuperBarberDbContext>();

            data.Database.Migrate();

            SeedCategories(data);
            SeedAdministrotor(services);
            SeedBarberRole(services);
            SeedBarberShopOwnerRole(services);

            return app;
        }

        private static void SeedCategories(SuperBarberDbContext data)
        {
            if (data.Categories.Any())
            {
                return;
            }

            data.Categories.AddRange(new[]
            {
                new Category{ Name = "Hair" },
                new Category{ Name = "Face" },
                new Category{ Name = "Epilation" },
                new Category{ Name = "Mix" }
            });

            data.SaveChanges();
        }

        private static void SeedAdministrotor(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            Task.Run(async () =>
            {
                if (await roleManager.RoleExistsAsync(AdministratorRoleName))
                {
                    return;
                }

                var role = new IdentityRole { Name = AdministratorRoleName };

                await roleManager.CreateAsync(role);

                const string adminEmail = "admin@barbers.com";
                const string adminPassword = "admin!23";
                const string adminFirstName = "AdminFirstName";
                const string adminLastName = "AdminLastName";

                var user = new User
                {
                    Email = adminEmail,
                    UserName = adminEmail,
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                };

                await userManager.CreateAsync(user, adminPassword);

                await userManager.AddToRoleAsync(user, role.Name);
            })
            .GetAwaiter()
            .GetResult();
        }
        
        private static void SeedBarberRole(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            Task.Run(async () =>
            {
                if (await roleManager.RoleExistsAsync(BarberRoleName))
                {
                    return;
                }

                var role = new IdentityRole { Name = BarberRoleName };

                await roleManager.CreateAsync(role);
            })
            .GetAwaiter()
            .GetResult();
        }
        
        private static void SeedBarberShopOwnerRole(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            Task.Run(async () =>
            {
                if (await roleManager.RoleExistsAsync(BarberShopOwnerRoleName))
                {
                    return;
                }

                var role = new IdentityRole { Name = BarberShopOwnerRoleName };

                await roleManager.CreateAsync(role);
            })
            .GetAwaiter()
            .GetResult();
        }
    }
}
