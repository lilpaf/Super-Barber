using Microsoft.EntityFrameworkCore;
using SuperBarber.Data.Models;
using SuperBarber.Data;
using Microsoft.AspNetCore.Identity;
using Moq;
using static SuperBarber.Tests.Mocks.SignInManegerMock;
using static SuperBarber.Tests.Mocks.UserManegerMock;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SuperBarber.Tests
{
    public class CreateTestDb
    {
        public IEnumerable<BarberShop> BarberShops => this.barberShops;
        public IEnumerable<District> Districts => this.districts;
        public City City => this.city;
        public IEnumerable<Barber> Barbers => this.barbers;
        public IEnumerable<User> Users => this.users;
        public UserManager<User> UserManager => this.userManager.Object;
        public SignInManager<User> SignInManager => this.signInManager.Object;

        public static readonly Guid GuestUserId = Guid.Parse("8f7fb633-be1d-49de-b7f8-8e927b498027");
        public static readonly Guid BarberShopOwnerUserId = Guid.Parse("ede8797e-cee5-4efe-918f-dd528b09a663");
        public static readonly Guid BarberUserId = Guid.Parse("ddba946c-b7eb-47d2-89d3-6e8448f2c059");
        public static readonly Guid OrderId = Guid.Parse("f20166b4-76a4-428b-b40f-8f1c474744df");

        private IEnumerable<BarberShop> barberShops;
        private IEnumerable<District> districts;
        private City city;
        private IEnumerable<Barber> barbers;
        private IEnumerable<User> users;
        private Mock<UserManager<User>> userManager;
        private Mock<SignInManager<User>> signInManager;

        private SuperBarberDbContext dbContext;

        public SuperBarberDbContext SeedDataInDb()
        {
            const string ImageUrl = "https://sortis.com/wp-content/uploads/2020/05/05222020_rudys1_124244-1560x968-1-1024x635.jpg";

            this.users = new List<User>()
            {
                new User(){ Id = GuestUserId.ToString(), FirstName = "", LastName = "", Email = "", PhoneNumber = "", IsDeleted = false, DeleteDate = null },
                 new User(){ Id = BarberShopOwnerUserId.ToString(), FirstName = "", LastName = "", Email = "", PhoneNumber = "", IsDeleted = false, DeleteDate = null },
                 new User(){ Id = BarberUserId.ToString(), FirstName = "", LastName = "", Email = "", PhoneNumber = "", IsDeleted = false, DeleteDate = null }
            };

            this.barbers = new List<Barber>()
            {
                new Barber() { Id = 1, UserId = BarberShopOwnerUserId.ToString(), FirstName = "", LastName = "", Email = "", PhoneNumber = "", IsDeleted = false, DeleteDate = null },
                new Barber() { Id = 2, UserId = BarberUserId.ToString(), FirstName = "", LastName = "", Email = "", PhoneNumber = "", IsDeleted = false, DeleteDate = null } 
            };

            this.districts = new List<District>()
            {
            new District(){Id = 1, Name = "Lozenets"},
            new District(){Id = 2, Name = "Serdica"}
            };

            this.city = new City() { Id = 1, Name = "Sofia" };

            this.barberShops = new List<BarberShop>()
            {
                 new BarberShop()
                 {
                     Id = 1,
                     Name = "TestBarberShopOne",
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
                    Orders = new HashSet<Order>(){new Order() { Id = OrderId, BarberId = 1, Date = new DateTime(2022,12,03,11,0,0).ToUniversalTime(), ServiceId = 1, UserId = GuestUserId.ToString() } },
                    Services = new HashSet<BarberShopServices>(),
                    IsDeleted = false,
                    DeleteDate = null
                },
            };


            var options = new DbContextOptionsBuilder<SuperBarberDbContext>()
                    .UseInMemoryDatabase(databaseName: "SuperBarberInMemoryDb")
                    .Options;
            this.dbContext = new SuperBarberDbContext(options);
            this.dbContext.AddRange(this.users);
            this.dbContext.AddRange(this.barbers);
            this.dbContext.Add(this.city);
            this.dbContext.AddRange(this.districts);
            this.dbContext.AddRange(this.barberShops);
            
            this.signInManager = MockSignInManager();
            this.userManager = MockUserManager(this.users);

            if (!dbContext.Barbers.Any() && !dbContext.BarberShops.Any() && !dbContext.Districts.Any()
                    && !dbContext.Users.Any() && !dbContext.Cities.Any())
            {
                this.dbContext.SaveChanges();
            }

            return this.dbContext;
        }

        public void DisposeTestDb()
        {
            this.dbContext.Dispose();
        }
    }
}
