using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Data.Models;

namespace SuperBarber.Infrastructure
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder PrepareDataBase(this IApplicationBuilder app)
        {
            using var scopedServices = app.ApplicationServices.CreateScope();

            var data = scopedServices.ServiceProvider.GetService<SuperBarberDbContext>();

            data.Database.Migrate();

            SeedCategories(data);

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
            });

            data.SaveChanges();
        }
    }
}
