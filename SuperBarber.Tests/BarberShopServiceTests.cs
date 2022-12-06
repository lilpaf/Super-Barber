using Microsoft.AspNetCore.Identity;
using SuperBarber.Data.Models;
using SuperBarber.Infrastructure;
using SuperBarber.Models.BarberShop;
using SuperBarber.Services.BarberShops;
using static SuperBarber.Tests.CreateTestDb;

namespace SuperBarber.Tests
{
    public class BarberShopServiceTests : BaseTest
    {
        private IBarberShopService service;
        private const int FakeId = 100;

        [SetUp]
        public void TestInitialize()
        {
            service = new BarberShopService(dbContextWithSeededData, userManager, signInManager);
        }

        [Test]
        public async Task TestAllBarberShops_ShouldReturnOnlyBarberShopWithOneInName()
        {
            var searchModel = new AllBarberShopQueryModel() { SearchTerm = "one" };

            var userId = GuestUserId.ToString();

            var model = await service.AllBarberShopsAsync(searchModel, userId);

            Assert.True(model.SearchTerm == searchModel.SearchTerm);
            Assert.True(model.BarberShops.Count() == model.TotalBarberShops);
            Assert.True(model.BarberShops.All(b => b.Id == 1));
        }
        
        [Test]
        public async Task TestAllBarberShops_ShouldThrowModelStateCustomExceptionWhenCityIsNonExistent()
        {
            var searchModel = new AllBarberShopQueryModel() { City = "Fake" };
            var userId = GuestUserId.ToString();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AllBarberShopsAsync(searchModel, userId), "Invalid city.");
        }
        
        [Test]
        public async Task TestAllBarberShops_ShouldReturnAllTheBarbershopsWithTheCorrectCityAndDistrict()
        {
            var city = new City() { Id = FakeId, Name = "Varna"};

            var newBarberShop = new BarberShop()
            {
                Id = FakeId,
                Name = "TestBarberShopNew",
                CityId = 2,
                DistrictId = 1,
                Street = "st. Test 1",
                StartHour = new TimeSpan(9, 0, 0),
                FinishHour = new TimeSpan(18, 0, 0),
                ImageUrl = "",
                IsPublic = true,
                Barbers = new HashSet<BarberShopBarbers>(),
                Orders = new HashSet<Order>(),
                Services = new HashSet<BarberShopServices>(),
                IsDeleted = false,
                DeleteDate = null
            };

            dbContextWithSeededData.Cities.Add(city);
            dbContextWithSeededData.BarberShops.Add(newBarberShop);
            dbContextWithSeededData.SaveChanges();

            var searchModel = new AllBarberShopQueryModel() { City = "Sofia", District = "Lozenets" };
            var userId = GuestUserId.ToString();

            var model = await service.AllBarberShopsAsync(searchModel, userId);

            Assert.True(model.BarberShops.All(bs => bs.City == "Sofia" && bs.District == "Lozenets"));
            Assert.True(model.TotalBarberShops == 1);
        }
        
        [Test]
        public async Task TestAllBarberShops_ShouldThrowModelStateCustomExceptionWhenDistrictIsNonExistent()
        {
            var searchModel = new AllBarberShopQueryModel() { District = "Fake" };
            var userId = GuestUserId.ToString();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AllBarberShopsAsync(searchModel, userId), "Invalid district.");
        }
        
        [Test]
        public async Task TestAllBarberShops_ShouldOrderBarberShopsByNameDefault()
        {
            var searchModel = new AllBarberShopQueryModel();
            var userId = GuestUserId.ToString();

            var model = await service.AllBarberShopsAsync(searchModel, userId);

            var dbBarberShopsOrdered = dbContextWithSeededData.BarberShops.Where(b => !b.IsDeleted).OrderBy(b => b.Name).ToList();

            Assert.True(dbBarberShopsOrdered.First().Id == model.BarberShops.First().Id);
        }
        
        [Test]
        public async Task TestAllBarberShops_ShouldNotTakeDeltedOrPrivateBarberShopsByDefault()
        {
            var searchModel = new AllBarberShopQueryModel();
            var userId = GuestUserId.ToString();

            var dbBarberShopDeleted = dbContextWithSeededData.BarberShops.First(b => b.IsDeleted);

            var privateBarberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            privateBarberShop.IsPublic = false;

            dbContextWithSeededData.SaveChanges();

            var model = await service.AllBarberShopsAsync(searchModel, userId);

            Assert.False(model.BarberShops.Any(b => b.Id == dbBarberShopDeleted.Id));
            Assert.False(model.BarberShops.Any(b => b.Id == privateBarberShop.Id));
        }
        
        [Test]
        public async Task TestAllBarberShops_ShouldOrderBarberShopsByDistrict()
        {
            var searchModel = new AllBarberShopQueryModel() { Sorting = BarberShopSorting.District};
            var userId = GuestUserId.ToString();

            var model = await service.AllBarberShopsAsync(searchModel, userId);

            var dbBarberShopsOrdered = dbContextWithSeededData.BarberShops.Where(b => !b.IsDeleted).OrderBy(b => b.District.Name).ToList();

            Assert.True(dbBarberShopsOrdered.First().Id == model.BarberShops.First().Id);
        }
        
        [Test]
        public async Task TestAllBarberShops_ShouldOrderBarberShopsByCity()
        {
            var city = new City() { Id = 10, Name = "Varna" };

            var newBarberShop = new BarberShop()
            {
                Id = FakeId,
                Name = "TestBarberShopNew",
                CityId = 2,
                DistrictId = 1,
                Street = "st. Test 1",
                StartHour = new TimeSpan(9, 0, 0),
                FinishHour = new TimeSpan(18, 0, 0),
                ImageUrl = "",
                IsPublic = true,
                Barbers = new HashSet<BarberShopBarbers>(),
                Orders = new HashSet<Order>(),
                Services = new HashSet<BarberShopServices>(),
                IsDeleted = false,
                DeleteDate = null
            };

            dbContextWithSeededData.Cities.Add(city);
            dbContextWithSeededData.BarberShops.Add(newBarberShop);
            dbContextWithSeededData.SaveChanges();

            var searchModel = new AllBarberShopQueryModel() { Sorting = BarberShopSorting.City};
            var userId = GuestUserId.ToString();

            var model = await service.AllBarberShopsAsync(searchModel, userId);

            var dbBarberShopsOrdered = dbContextWithSeededData.BarberShops.Where(b => !b.IsDeleted).OrderBy(b => b.City.Name).ToList();

            Assert.True(dbBarberShopsOrdered.First().Id == model.BarberShops.First().Id);
        }
        
        [Test]
        public async Task TestAllBarberShops_ModelShouldReturnCorrectInformation()
        {
            var searchModel = new AllBarberShopQueryModel();
            var ownerUserId = BarberShopOwnerUserId.ToString();

            var model = await service.AllBarberShopsAsync(searchModel, ownerUserId);

            Assert.True(model.BarberShops.First().UserIsEmployee);
            Assert.True(model.BarberShops.First().UserIsOwner);

            var city = dbContextWithSeededData.Cities.First();

            // Return the 1 city we have in the db
            Assert.True(model.Cities.All(c => c == city.Name));
            
            Assert.True(model.Districts.Count() == 2);

            foreach (var district in dbContextWithSeededData.Districts.ToList())
            {
                Assert.True(model.Districts.Any(d => d == district.Name));
            }
            
            var userId = GuestUserId.ToString();

            model = await service.AllBarberShopsAsync(searchModel, userId);

            Assert.False(model.BarberShops.First().UserIsEmployee);
            Assert.False(model.BarberShops.First().UserIsOwner);
        }
        
        [Test]
        public async Task TestAllBarberShops_PageingOfBarberShopsWork()
        {
            var searchModel = new AllBarberShopQueryModel() { CurrentPage = 0};
            var userId = GuestUserId.ToString();

            var model = await service.AllBarberShopsAsync(searchModel, userId);

            Assert.True(model.BarberShops.Count() == 2);

            searchModel = new AllBarberShopQueryModel() { CurrentPage = 2 };

            model = await service.AllBarberShopsAsync(searchModel, userId);

            Assert.False(model.BarberShops.Any());
        }
        
        [Test]
        public async Task TestAddBarberShop_ShouldThrowModelStateCustomExceptionWhenHourInputIsIncorrect()
        {
            var userId = BarberUserId.ToString();

            var formModel = new BarberShopFormModel()
            {
                Name = "NewTestBarberShop",
                City = "Varna",
                District = "Chaika",
                Street = "Test Street 1",
                StartHour = "10-30",
                FinishHour = "19:30",
                ImageUrl = "URL"
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberShopAsync(formModel, userId), "Start hour input is incorrect.");
            
            formModel = new BarberShopFormModel()
            {
                Name = "NewTestBarberShop",
                City = "Varna",
                District = "Chaika",
                Street = "Test Street 1",
                StartHour = "10:30",
                FinishHour = "19-30",
                ImageUrl = "URL"
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberShopAsync(formModel, userId), "Finish hour input is incorrect.");
        }
        
        [Test]
        public async Task TestAddBarberShop_ShouldThrowModelStateCustomExceptionWhenStartHourIsBiggerThanFinishHour()
        {
            var userId = BarberUserId.ToString();

            var formModel = new BarberShopFormModel()
            {
                Name = "NewTestBarberShop",
                City = "Varna",
                District = "Chaika",
                Street = "Test Street 1",
                StartHour = "11:30",
                FinishHour = "11:00",
                ImageUrl = "URL"
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberShopAsync(formModel, userId), "Opening hour must be smaller that the closing hour. And can not be the same as the closing hour.");
        }
        
        [Test]
        public async Task TestAddBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberShopAlreadyExist()
        {
            var userId = BarberUserId.ToString();

            var formModel = new BarberShopFormModel()
            {
                Name = "TestBarberShop One",
                City = "Sofia",
                District = "Lozenets",
                Street = "st. Test 1",
                StartHour = "10:30",
                FinishHour = "19:00",
                ImageUrl = "URL"
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberShopAsync(formModel, userId), "This barbershop already exist.");
        }
        
        [Test]
        public async Task TestAddBarberShop_ShouldThrowModelStateCustomExceptionWhenUserIsNotABarberOrIsDeletedOrNonExistent()
        {
            var userId = GuestUserId.ToString();

            var formModel = new BarberShopFormModel()
            {
                Name = "TestBarberShop One",
                City = "Sofia",
                District = "Lozenets",
                Street = "st. Test 1",
                StartHour = "10:30",
                FinishHour = "19:00",
                ImageUrl = "URL"
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberShopAsync(formModel, userId), "You need to be a barber in order to do this action.");

            //User not existing shouldn't be possible by default in order to become a barber you should be registered user
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberShopAsync(formModel, FakeId.ToString()), "User does not exist.");

            var deletedUser = new User() { Id = "DeletedUser", FirstName = "", LastName = "", Email = "", PhoneNumber = "", IsDeleted = true, DeleteDate = DateTime.UtcNow };

            dbContextWithSeededData.Users.Add(deletedUser);
            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberShopAsync(formModel, deletedUser.Id), "User does not exist.");
        }
        
        [Test]
        public async Task TestAddBarberShop_DeletedBarberShopShouldBeRestoredWhenTheAddedOneIsTheSame()
        {
            var userId = BarberUserId.ToString();

            var deletedBarberShop = dbContextWithSeededData.BarberShops.First(b => b.IsDeleted);

            var formModel = new BarberShopFormModel()
            {
                Name = deletedBarberShop.Name,
                City = deletedBarberShop.City.Name,
                District = deletedBarberShop.District.Name,
                Street = deletedBarberShop.Street,
                StartHour = "10:30",
                FinishHour = "19:00",
                ImageUrl = "URL"
            };

            await service.AddBarberShopAsync(formModel, userId);

            Assert.True(dbContextWithSeededData.BarberShops.Any(b => b.Id == deletedBarberShop.Id && !b.IsDeleted && b.DeleteDate == null));
            Assert.False(dbContextWithSeededData.BarberShops.Any(b => b.IsDeleted && b.DeleteDate == null));
            Assert.True(dbContextWithSeededData.BarberShops.Count() == 3);
        }
        
        [Test]
        public async Task TestAddBarberShop_ShouldAddBarberShopWithNewCityAndDistrict()
        {
            var userId = BarberUserId.ToString();

            var formModel = new BarberShopFormModel()
            {
                Name = "NewTestBarberShop",
                City = "varna",
                District = "Chaika",
                Street = "st. Test 1",
                StartHour = "10:30",
                FinishHour = "19:00",
                ImageUrl = "URL"
            };

            await service.AddBarberShopAsync(formModel, userId);

            Assert.True(dbContextWithSeededData.BarberShops.Any(b => b.Name == formModel.Name && b.District.Name == formModel.District && b.Street == formModel.Street));
            Assert.True(dbContextWithSeededData.BarberShops.Any(b => b.StartHour.ToString(@"hh\:mm") == formModel.StartHour && b.FinishHour.ToString(@"hh\:mm") == formModel.FinishHour));
            // varna from form input should be saved as Varna in db
            Assert.True(dbContextWithSeededData.Cities.Any(c => c.Name == "Varna"));
            Assert.True(dbContextWithSeededData.BarberShops.Count() == 4);
        }
    }
}
