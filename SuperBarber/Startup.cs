using CarRentingSystem.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBarber.Areas.Identity.Services.Account;
using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Services.Barbers;
using SuperBarber.Services.BarberShops;
using SuperBarber.Services.Cart;
using SuperBarber.Services.Home;
using SuperBarber.Services.Service;
using static SuperBarber.Infrastructure.ApplicationBuilderExtensions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<SuperBarberDbContext>(options => options
    .UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession();

builder.Services.AddDefaultIdentity<User>(options =>
    {
        //options.SignIn.RequireConfirmedAccount = true;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<SuperBarberDbContext>();

builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
});

builder.Services.AddTransient<IBarberShopService, BarberShopService>();
builder.Services.AddTransient<IBarberService, BarberService>();
builder.Services.AddTransient<IServiceService, ServiceService>();
builder.Services.AddTransient<IHomeService, HomeService>();
builder.Services.AddTransient<ICartService, CartService>();
builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.PrepareDataBase();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection()
   .UseStaticFiles()
   .UseRouting()
   .UseAuthentication()
   .UseAuthorization()
   .UseSession()
   .UseEndpoints(endpoints =>
   {
       endpoints.MapDefaultAreaRoute();
       
       /*endpoints.MapControllerRoute(
           name: "Admin",
           pattern: "{area:exists}/{controller=Home}/{action=Idex}/{id?}");*/
       
       endpoints.MapControllerRoute(
           name: "Custom Route",
           pattern: "/{controller}/{action}/{barbershopid}/{information}",
           defaults: new {controller = "Service", action = "All" });
       
       endpoints.MapDefaultControllerRoute();

       endpoints.MapRazorPages();
   });

app.Run();