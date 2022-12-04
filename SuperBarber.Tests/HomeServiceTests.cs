using Microsoft.EntityFrameworkCore;
using SuperBarber.Data;
using SuperBarber.Data.Models;
using SuperBarber.Infrastructure;
using SuperBarber.Services.Home;
using System.Runtime.CompilerServices;

namespace SuperBarber.Tests
{
    [TestFixture]
    public class HomeServiceTests
    {
        private IEnumerable<BarberShop> barberShops;
        private IEnumerable<District> districts;
        private City city;
        private Barber barber;
        private User user;

        private SuperBarberDbContext dbContext;

        [OneTimeSetUp]
        public void TestInitialize()
        {
            const string ImageUrl = "https://sortis.com/wp-content/uploads/2020/05/05222020_rudys1_124244-1560x968-1-1024x635.jpg";

            this.user= new User() { Id = "UserId"};

            this.barber = new Barber() { Id = 1, UserId = "UserId"};

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
                    ImageUrl = ImageUrl, IsPublic = true, 
                    Barbers = new HashSet<BarberShopBarbers>(){new BarberShopBarbers(){BarberId = 1, IsOwner = true, IsAvailable = true } }, 
                    Orders = new HashSet<Order>(),
                    Services = new HashSet<BarberShopServices>()
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
                    Orders = new HashSet<Order>(){new Order() { BarberId = 1, Date = new DateTime(2022,12,03,11,0,0).ToUniversalTime(), ServiceId = 1, UserId= "UserId" } },
                    Services = new HashSet<BarberShopServices>()
                },
            };

            var options = new DbContextOptionsBuilder<SuperBarberDbContext>()
                    .UseInMemoryDatabase(databaseName: "SuperBarberInMemoryDb")
                    .Options;
            this.dbContext = new SuperBarberDbContext(options);
            this.dbContext.Add(user);
            this.dbContext.Add(barber);
            this.dbContext.Add(city);
            this.dbContext.AddRange(districts);
            this.dbContext.AddRange(this.barberShops);
            this.dbContext.SaveChanges();
        }

        [Test]
        public async Task Test_GetOneBarberShopMatchingTheCriteria()
        { 
            IHomeService service = new HomeService(this.dbContext);
            
            var barberShops = await service.SearchAvalibleBarbershopsAsync("Sofia", "Lozenets", "2022-12-03", "12:00", "UserId");

            var dbBarberShops = this.barberShops.ToList();
                
            Assert.True(barberShops.Count() == 1);

            foreach (var barberShop in barberShops)
            {
                var dbBarberShop = dbBarberShops.First(bs => bs.Id == barberShop.Id);

                Assert.True(dbBarberShop.Name == barberShop.BarberShopName);
                Assert.True(dbBarberShop.StartHour.ToString(@"hh\:mm") == barberShop.StartHour);
                Assert.True(dbBarberShop.FinishHour.ToString(@"hh\:mm") == barberShop.FinishHour);
                Assert.True(dbBarberShop.Street == barberShop.Street);
                Assert.True(dbBarberShop.ImageUrl == barberShop.ImageUrl);
                Assert.True(dbBarberShop.ImageUrl == barberShop.ImageUrl);
                Assert.True(dbBarberShop.City.Name == barberShop.City);
                Assert.True(dbBarberShop.District.Name == barberShop.District);
            }
        }

        [Test]
        public async Task Test_GetTwoBarberShopsMatchingTheCriteria()
        { 
            IHomeService service = new HomeService(this.dbContext);
            
            var barberShops = await service.SearchAvalibleBarbershopsAsync("Sofia", "All", "2022-12-03", "12:00", "UserId");

            var dbBarberShops = this.barberShops.ToList();
                
            Assert.True(barberShops.Count() == 2);

            foreach (var barberShop in barberShops)
            {
                var dbBarberShop = dbBarberShops.First(bs => bs.Id == barberShop.Id);

                Assert.True(dbBarberShop.Name == barberShop.BarberShopName);
                Assert.True(dbBarberShop.StartHour.ToString(@"hh\:mm") == barberShop.StartHour);
                Assert.True(dbBarberShop.FinishHour.ToString(@"hh\:mm") == barberShop.FinishHour);
                Assert.True(dbBarberShop.Street == barberShop.Street);
                Assert.True(dbBarberShop.ImageUrl == barberShop.ImageUrl);
                Assert.True(dbBarberShop.ImageUrl == barberShop.ImageUrl);
                Assert.True(dbBarberShop.City.Name == barberShop.City);
                Assert.True(dbBarberShop.District.Name == barberShop.District);
            }
        }
        
        [Test]
        public void Test_ThrowModelStateCustomExceptionWhenThereIsNoMatchingBarberShops()
        { 
            IHomeService service = new HomeService(this.dbContext);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.SearchAvalibleBarbershopsAsync("Sofia", "All", "2022-12-03", "8:00", "UserId"), "Right now we do not have any avalible barbershops matching your criteria.");
        }
        
        [Test]
        public void Test_ThrowModelStateCustomExceptionWhenThereIsNoCityFound()
        { 
            IHomeService service = new HomeService(this.dbContext);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.SearchAvalibleBarbershopsAsync("Varna", "All", "2022-12-03", "12:00", "UserId"), "Invalid city.");
        }
        
        [Test]
        public void Test_ThrowModelStateCustomExceptionWhenThereIsNoDistrictFound()
        { 
            IHomeService service = new HomeService(this.dbContext);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.SearchAvalibleBarbershopsAsync("Sofia", "Mladost", "2022-12-03", "12:00", "UserId"), "Invalid district.");
        }
        
        [Test]
        public void Test_ThrowModelStateCustomExceptionWhenTheTimeInpitIsInvalid()
        { 
            IHomeService service = new HomeService(this.dbContext);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.SearchAvalibleBarbershopsAsync("Sofia", "All", "2022-12-03", "12-00", "UserId"), "Invalid time input.");
        }
        
        [Test]
        public void Test_ThrowModelStateCustomExceptionWhenTheDateInpitIsInvalid()
        { 
            IHomeService service = new HomeService(this.dbContext);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.SearchAvalibleBarbershopsAsync("Sofia", "All", "20221203", "12:00", "UserId"), "Invalid date input.");
        }
    }
}