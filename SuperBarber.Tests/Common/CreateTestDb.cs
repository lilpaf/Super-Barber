using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Globalization;
using SuperBarber.Infrastructure.Data.Models;
using SuperBarber.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using static SuperBarber.Tests.Mocks.SignInManegerMock;
using static SuperBarber.Tests.Mocks.UserManegerMock;
using static SuperBarber.Tests.Mocks.IWebHostEnviromentMock;

namespace SuperBarber.Tests.Common
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
        public IWebHostEnvironment WebHostEnvironment => webHostEnvironment;

        public static readonly Guid GuestUserId = Guid.Parse("8f7fb633-be1d-49de-b7f8-8e927b498027");
        public static readonly Guid BarberShopOwnerUserId = Guid.Parse("ede8797e-cee5-4efe-918f-dd528b09a663");
        public static readonly Guid BarberUserId = Guid.Parse("ddba946c-b7eb-47d2-89d3-6e8448f2c059");
        public static readonly Guid OrderId = Guid.Parse("f20166b4-76a4-428b-b40f-8f1c474744df");
        public static readonly string NewTestImage = "new-testistockphototest-new-639607852-1024x1024-202112100453234.jpg";

        private IEnumerable<BarberShop> barberShops;
        private IEnumerable<District> districts;
        private City city;
        private Category category;
        private IEnumerable<Barber> barbers;
        private IEnumerable<User> users;
        private Mock<UserManager<User>> userManager;
        private Mock<SignInManager<User>> signInManager;
        private IWebHostEnvironment webHostEnvironment;
        private List<string> images;

        private SuperBarberDbContext dbContext;

        public SuperBarberDbContext SeedDataInDb()
        {
            webHostEnvironment = MockIWebHostEnvironment().Object;

            var wwwRootPath = webHostEnvironment.WebRootPath;

            const string firstImageName = "testistockphototest-639607852-1024x1024-202112100453234.jpg";
            const string secondImageName = "test2istockphototest2-639607852-1024x1024-202112100453234.jpg";

            images = new List<string>()
            {
                firstImageName,
                secondImageName
            };

            foreach (var image in images)
            {
                string path = Path.Combine(wwwRootPath + "/Image/", image);

                using (FileStream fs = File.Create(path))
                {
                    for (byte i = 0; i < 100; i++)
                    {
                        fs.WriteByte(i);
                    }
                }
            }

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
                     ImageName = firstImageName,
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
                    ImageName = secondImageName,
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
                    ImageName = null,
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
            var wwwRootPath = webHostEnvironment.WebRootPath;

            foreach (var image in images)
            {
                var path = Path.Combine(wwwRootPath, "image", image);

                if (File.Exists(path))
                {
                    File.Delete(path);
                    continue;
                }

                //Deletes the added images when you add delete or edit the barbershop
                var rootFolderPath = Path.Combine(wwwRootPath, "image");

                string filesToDelete = $@"*{NewTestImage.Replace(".jpg", "")}*.jpg";
                string[] fileList = Directory.GetFiles(rootFolderPath, filesToDelete);

                foreach (var file in fileList)
                {
                    File.Delete(file);
                }
            }
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
