using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBarber.Core.Services.Account;
using SuperBarber.Core.Services.Barbers;
using SuperBarber.Core.Services.BarberShops;
using SuperBarber.Core.Services.Cart;
using SuperBarber.Core.Services.Home;
using SuperBarber.Core.Services.Mail;
using SuperBarber.Core.Services.Order;
using SuperBarber.Core.Services.Service;
using SuperBarber.Extensions;
using SuperBarber.Infrastructure.Data;
using SuperBarber.Infrastructure.Data.Models;
using SuperBarber.Services.Service;
using static SuperBarber.Extensions.ApplicationBuilderExtensions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<SuperBarberDbContext>(options => options
    .UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession();

builder.Services.AddDefaultIdentity<User>(options =>
    {
        //Uncomment this when you want to use the mailing service
        options.SignIn.RequireConfirmedAccount = true;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
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
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.PrepareDataBase();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
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