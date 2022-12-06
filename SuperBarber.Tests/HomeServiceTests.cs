using SuperBarber.Data;
using SuperBarber.Infrastructure;
using SuperBarber.Services.Home;
using static SuperBarber.Tests.CreateTestDb;


namespace SuperBarber.Tests
{
    [TestFixture]
    public class HomeServiceTests : BaseTest
    {
        private IHomeService service;

        [SetUp]
        public void TestInitialize()
        {
            service = new HomeService(base.dbContextWithSeededData);
        }

        [Test]
        public async Task Test_GetOneBarberShopMatchingTheCriteria()
        {
            var dbBarberShops = testDb.BarberShops.ToList();
            
            var testBarberShop = dbBarberShops.First(b => !b.IsDeleted);
            var testUser = testDb.Users.First(u => u.Id == GuestUserId.ToString());

            var barberShops = await service.SearchAvalibleBarbershopsAsync(testBarberShop.City.Name, testBarberShop.District.Name, "2022-12-03", "12:00", testUser.Id);
    
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
                Assert.True(barberShop.UserIsEmployee == false);
                Assert.True(barberShop.UserIsOwner == false);
            }
        }

        [Test]
        public async Task Test_GetTwoBarberShopsMatchingTheCriteria()
        {
            var dbBarberShops = testDb.BarberShops.ToList();

            var testBarberShop = dbBarberShops.First(b => !b.IsDeleted);
            var testUser = testDb.Users.First(u => u.Id == BarberShopOwnerUserId.ToString());

            var barberShops = await service.SearchAvalibleBarbershopsAsync(testBarberShop.City.Name, "All", "2022-12-03", "12:00", testUser.Id);
     
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
                Assert.True(barberShop.UserIsEmployee == true);
                Assert.True(barberShop.UserIsOwner == true);
            }
        }
        
        [Test]
        public void Test_ThrowModelStateCustomExceptionWhenThereIsNoMatchingBarberShops()
        {
            var testUser = testDb.Users.First(u => u.Id == GuestUserId.ToString());

            var dbBarberShop = testDb.BarberShops.First(b => !b.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.SearchAvalibleBarbershopsAsync(dbBarberShop.City.Name, "All", "2022-12-03", "8:00", testUser.Id), "Right now we do not have any avalible barbershops matching your criteria.");
        }
        
        [Test]
        public void Test_ThrowModelStateCustomExceptionWhenThereIsNoCityFound()
        {
            var testUser = testDb.Users.First(u => u.Id == GuestUserId.ToString());

            var dbBarberShop = testDb.BarberShops.First(b => !b.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.SearchAvalibleBarbershopsAsync("Varna", "All", "2022-12-03", "12:00", testUser.Id), "Invalid city.");
        }
        
        [Test]
        public void Test_ThrowModelStateCustomExceptionWhenThereIsNoDistrictFound()
        {
            var testUser = testDb.Users.First(u => u.Id == GuestUserId.ToString());

            var dbBarberShop = testDb.BarberShops.First(b => !b.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.SearchAvalibleBarbershopsAsync(dbBarberShop.City.Name, "Mladost", "2022-12-03", "12:00", testUser.Id), "Invalid district.");
        }
        
        [Test]
        public void Test_ThrowModelStateCustomExceptionWhenTheTimeInpitIsInvalid()
        {
            var testUser = testDb.Users.First(u => u.Id == GuestUserId.ToString());

            var dbBarberShop = testDb.BarberShops.First(b => !b.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.SearchAvalibleBarbershopsAsync(dbBarberShop.City.Name, "All", "2022-12-03", "12-00", testUser.Id), "Invalid time input.");
        }
        
        [Test]
        public void Test_ThrowModelStateCustomExceptionWhenTheDateInpitIsInvalid()
        {
            var testUser = testDb.Users.First(u => u.Id == GuestUserId.ToString());

            var dbBarberShop = testDb.BarberShops.First(b => !b.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.SearchAvalibleBarbershopsAsync(dbBarberShop.City.Name, "All", "20221203", "12:00", testUser.Id), "Invalid date input.");
        }
        
        [Test]
        public async Task Test_GetListOfCitiesNamesFromTheDb()
        {
            var testCities = await service.GetCitiesAsync();

            var testCity = testDb.City;

            Assert.True(testCities.Any());
            foreach (var cityName in testCities)
            {
                Assert.True(cityName == testCity.Name);
            }
        }
        
        [Test]
        public async Task Test_GetListOfDistrictsNamesFromTheDb()
        {
            var testDistricts = await service.GetDistrictsAsync();

            var testDbDistricts = testDb.Districts;

            Assert.True(testDistricts.Count() == 2);
            foreach (var districtName in testDistricts)
            {
                Assert.True(testDbDistricts.Any(d => d.Name == districtName));
            }
        }
    }
}