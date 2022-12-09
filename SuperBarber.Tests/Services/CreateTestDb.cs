using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Moq;
using static SuperBarber.Tests.Mocks.SignInManegerMock;
using static SuperBarber.Tests.Mocks.UserManegerMock;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Globalization;
using System.Threading;
using SuperBarber.Infrastructure.Data.Models;
using SuperBarber.Infrastructure.Data;

namespace SuperBarber.Tests.Services
{
    public class CreateTestDb
    {
        public IEnumerable<BarberShop> BarberShops => barberShops;
        public IEnumerable<District> Districts => districts;
        public City City => city;
        public IEnumerable<Barber> Barbers => barbers;
        public IEnumerable<User> Users => users;
        public UserManager<User> UserManager => userManager.Object;
        public SignInManager<User> SignInManager => signInManager.Object;

        public static readonly Guid GuestUserId = Guid.Parse("8f7fb633-be1d-49de-b7f8-8e927b498027");
        public static readonly Guid BarberShopOwnerUserId = Guid.Parse("ede8797e-cee5-4efe-918f-dd528b09a663");
        public static readonly Guid BarberUserId = Guid.Parse("ddba946c-b7eb-47d2-89d3-6e8448f2c059");
        public static readonly Guid OrderId = Guid.Parse("f20166b4-76a4-428b-b40f-8f1c474744df");

        private IEnumerable<BarberShop> barberShops;
        private IEnumerable<District> districts;
        private City city;
        private Category category;
        private IEnumerable<Barber> barbers;
        private IEnumerable<User> users;
        private Mock<UserManager<User>> userManager;
        private Mock<SignInManager<User>> signInManager;

        private SuperBarberDbContext dbContext;

        public SuperBarberDbContext SeedDataInDb()
        {
            const string ImageUrl = "https://sortis.com/wp-content/uploads/2020/05/05222020_rudys1_124244-1560x968-1-1024x635.jpg";

            users = new List<User>()
            {
                new User(){ Id = GuestUserId.ToString(), FirstName = "", LastName = "", Email = "", PhoneNumber = "", IsDeleted = false, DeleteDate = null },
                 new User(){ Id = BarberShopOwnerUserId.ToString(), FirstName = "", LastName = "", Email = "", PhoneNumber = "", IsDeleted = false, DeleteDate = null },
                 new User(){ Id = BarberUserId.ToString(), FirstName = "", LastName = "", Email = "", PhoneNumber = "", IsDeleted = false, DeleteDate = null }
            };

            barbers = new List<Barber>()
            {
                new Barber() { Id = 1, UserId = BarberShopOwnerUserId.ToString(), FirstName = "", LastName = "", Email = "", PhoneNumber = "", IsDeleted = false, DeleteDate = null },
                new Barber() { Id = 2, UserId = BarberUserId.ToString(), FirstName = "", LastName = "", Email = "", PhoneNumber = "", IsDeleted = false, DeleteDate = null }
            };

            districts = new List<District>()
            {
            new District(){Id = 1, Name = "Lozenets"},
            new District(){Id = 2, Name = "Serdica"}
            };

            city = new City() { Id = 1, Name = "Sofia" };

            category = new Category() { Id = 1, Name = "Hair" };

            var dateParsed = DateTime.Parse(DateTime.UtcNow.AddDays(1).ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture));

            var ts = new TimeSpan(12, 00, 0);

            dateParsed = dateParsed.Date + ts;

            barberShops = new List<BarberShop>()
            {
                 new BarberShop()
                 {
                     Id = 1,
                     Name = "TestBarberShop One",
                     CityId = 1,
                     DistrictId = 1,
                     Street= "st. Test 1",
                     StartHour = new TimeSpan(9,0,0),
                     FinishHour = new TimeSpan(18,0,0),
                     ImageUrl = ImageUrl,
                     IsPublic = true,
                     Barbers = new HashSet<BarberShopBarbers>(){new BarberShopBarbers(){BarberId = 1, IsOwner = true, IsAvailable = true } },
                     Orders = new HashSet<Order>(),
                     Services = new HashSet<BarberShopServices>(),
                     IsDeleted = false,
                     DeleteDate = null
                 },
                new BarberShop()
                {
                    Id = 2,
                    Name = "TestBarberShopTwo",
                    CityId = 1,
                    DistrictId = 2,
                    Street= "st. Test 2",
                    StartHour = new TimeSpan(9,0,0),
                    FinishHour = new TimeSpan(18,0,0),
                    ImageUrl = ImageUrl,
                    IsPublic = true,
                    Barbers = new HashSet<BarberShopBarbers>(){new BarberShopBarbers(){BarberId = 1, IsOwner = true, IsAvailable = true } },
                    Orders = new HashSet<Order>()
                    {
                        new Order()
                        {
                            Id = OrderId,
                            BarberId = 1,
                            Date = dateParsed.ToUniversalTime(),
                            ServiceId = 1,
                            UserId = GuestUserId.ToString(),
                            IsDeleted = false,
                            DeleteDate = null
                        }
                    },
                    Services = new HashSet<BarberShopServices>()
                    {
                        new BarberShopServices()
                        {
                            Service = new Service()
                            {
                                Id = 1,
                                Name = "Hair cut",
                                Category = category,
                                CategoryId = 1,
                                IsDeleted = false
                            },
                            Price = 20
                        }
                    },
                    IsDeleted = false,
                    DeleteDate = null
                },
                new BarberShop
                {
                    Id = 3,
                    Name = "DeletedBarberShopTest",
                    CityId = 1,
                    DistrictId = 2,
                    Street = "st. Deleted 2",
                    StartHour = new TimeSpan(9, 0, 0),
                    FinishHour = new TimeSpan(18, 0, 0),
                    ImageUrl = ImageUrl,
                    IsPublic = false,
                    Barbers = new HashSet<BarberShopBarbers>(),
                    Orders = new HashSet<Order>(),
                    Services = new HashSet<BarberShopServices>(),
                    IsDeleted = true,
                    DeleteDate = DateTime.UtcNow
                }
            };

            var options = new DbContextOptionsBuilder<SuperBarberDbContext>()
                    .UseInMemoryDatabase(databaseName: "SuperBarberInMemoryDb")
                    .Options;
            dbContext = new SuperBarberDbContext(options);
            dbContext.AddRange(users);
            dbContext.AddRange(barbers);
            dbContext.Add(city);
            dbContext.AddRange(districts);
            dbContext.AddRange(barberShops);

            signInManager = MockSignInManager();
            userManager = MockUserManager(users);

            dbContext.SaveChanges();

            return dbContext;
        }

        public void DisposeTestDb()
        {
            dbContext.Orders.RemoveRange(dbContext.Orders.ToList());
            dbContext.Services.RemoveRange(dbContext.Services.ToList());
            dbContext.Categories.RemoveRange(dbContext.Categories.ToList());
            dbContext.BarberShops.RemoveRange(dbContext.BarberShops.ToList());
            dbContext.Cities.RemoveRange(dbContext.Cities.ToList());
            dbContext.Districts.RemoveRange(dbContext.Districts.ToList());
            dbContext.Barbers.RemoveRange(dbContext.Barbers.ToList());
            dbContext.Users.RemoveRange(dbContext.Users.ToList());
            dbContext.SaveChanges();
            dbContext.Dispose();
        }
    }
}
