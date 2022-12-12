using Microsoft.AspNetCore.Http;
using NUnit.Framework.Internal;
using SuperBarber.Core.Extensions;
using SuperBarber.Core.Models.BarberShop;
using SuperBarber.Core.Services.BarberShops;
using SuperBarber.Infrastructure.Data.Models;
using SuperBarber.UnitTests.Common;
using static SuperBarber.UnitTests.Common.CreateTestDb;

namespace SuperBarber.UnitTests.Services
{
    public class BarberShopServiceTests : BaseTest
    {
        private IBarberShopService service;
        private const int FakeId = 100;
        private IFormFile file;
        private string wwwRootPath;

        [SetUp]
        public void TestInitialize()
        {
            service = new BarberShopService(dbContextWithSeededData, userManager, signInManager);
            wwwRootPath = webHostEnvironment.WebRootPath;
            var stream = new MemoryStream();
            file = new FormFile(new MemoryStream(), 0, stream.Length, NewTestImage, NewTestImage);
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
        public void TestAllBarberShops_ShouldThrowModelStateCustomExceptionWhenCityIsNonExistent()
        {
            var searchModel = new AllBarberShopQueryModel() { City = "Fake" };
            var userId = GuestUserId.ToString();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AllBarberShopsAsync(searchModel, userId), "Invalid city.");
        }

        [Test]
        public async Task TestAllBarberShops_ShouldReturnAllTheBarbershopsWithTheCorrectCityAndDistrict()
        {
            var city = new City() { Id = FakeId, Name = "Varna" };

            var newBarberShop = new BarberShop()
            {
                Id = FakeId,
                Name = "TestBarberShopNew",
                CityId = FakeId,
                DistrictId = 1,
                Street = "st. Test 1",
                StartHour = new TimeSpan(9, 0, 0),
                FinishHour = new TimeSpan(18, 0, 0),
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
        public void TestAllBarberShops_ShouldThrowModelStateCustomExceptionWhenDistrictIsNonExistent()
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
            var searchModel = new AllBarberShopQueryModel() { Sorting = BarberShopSorting.District };
            var userId = GuestUserId.ToString();

            var model = await service.AllBarberShopsAsync(searchModel, userId);

            var dbBarberShopsOrdered = dbContextWithSeededData.BarberShops.Where(b => !b.IsDeleted).OrderBy(b => b.District.Name).ToList();

            Assert.True(dbBarberShopsOrdered.First().Id == model.BarberShops.First().Id);
        }

        [Test]
        public async Task TestAllBarberShops_ShouldOrderBarberShopsByCity()
        {
            var city = new City() { Id = FakeId, Name = "Varna" };

            var newBarberShop = new BarberShop()
            {
                Id = FakeId,
                Name = "TestBarberShopNew",
                CityId = FakeId,
                DistrictId = 1,
                Street = "st. Test 1",
                StartHour = new TimeSpan(9, 0, 0),
                FinishHour = new TimeSpan(18, 0, 0),
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

            var searchModel = new AllBarberShopQueryModel() { Sorting = BarberShopSorting.City };
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
            var searchModel = new AllBarberShopQueryModel() { CurrentPage = 0 };
            var userId = GuestUserId.ToString();

            var model = await service.AllBarberShopsAsync(searchModel, userId);

            Assert.True(model.BarberShops.Count() == 2);

            searchModel = new AllBarberShopQueryModel() { CurrentPage = 2 };

            model = await service.AllBarberShopsAsync(searchModel, userId);

            Assert.False(model.BarberShops.Any());
        }

        [Test]
        public void TestAddBarberShop_ShouldThrowModelStateCustomExceptionWhenHourInputIsIncorrect()
        {
            var userId = BarberUserId.ToString();

            var formModel = new BarberShopAddFormModel()
            {
                Name = "NewTestBarberShop",
                City = "Varna",
                District = "Chaika",
                Street = "Test Street 1",
                StartHour = "10-30",
                FinishHour = "19:30",
                ImageFile = file
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberShopAsync(formModel, userId, wwwRootPath), "Start hour input is incorrect.");

            formModel = new BarberShopAddFormModel()
            {
                Name = "NewTestBarberShop",
                City = "Varna",
                District = "Chaika",
                Street = "Test Street 1",
                StartHour = "10:30",
                FinishHour = "19-30",
                ImageFile = file
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberShopAsync(formModel, userId, wwwRootPath), "Finish hour input is incorrect.");
        }

        [Test]
        public void TestAddBarberShop_ShouldThrowModelStateCustomExceptionWhenStartHourIsBiggerThanFinishHourOrEqual()
        {
            var userId = BarberUserId.ToString();

            var formModel = new BarberShopAddFormModel()
            {
                Name = "NewTestBarberShop",
                City = "Varna",
                District = "Chaika",
                Street = "Test Street 1",
                StartHour = "11:30",
                FinishHour = "11:00",
                ImageFile = file
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberShopAsync(formModel, userId, wwwRootPath), "Opening hour must be smaller that the closing hour. And can not be the same as the closing hour.");

            formModel = new BarberShopAddFormModel()
            {
                Name = "NewTestBarberShop",
                City = "Varna",
                District = "Chaika",
                Street = "Test Street 1",
                StartHour = "10:30",
                FinishHour = "10:30",
                ImageFile = file
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberShopAsync(formModel, userId, wwwRootPath), "Opening hour must be smaller that the closing hour. And can not be the same as the closing hour.");
        }

        [Test]
        public void TestAddBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberShopAlreadyExist()
        {
            var userId = BarberUserId.ToString();

            var formModel = new BarberShopAddFormModel()
            {
                Name = "TestBarberShop One",
                City = "Sofia",
                District = "Lozenets",
                Street = "st. Test 1",
                StartHour = "10:30",
                FinishHour = "19:00",
                ImageFile = file
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberShopAsync(formModel, userId, wwwRootPath), "This barbershop already exist.");
        }

        [Test]
        public void TestAddBarberShop_ShouldThrowModelStateCustomExceptionWhenUserIsNotABarberOrIsDeletedOrNonExistent()
        {
            var userId = GuestUserId.ToString();

            var formModel = new BarberShopAddFormModel()
            {
                Name = "TestBarberShop One",
                City = "Sofia",
                District = "Lozenets",
                Street = "st. Test 1",
                StartHour = "10:30",
                FinishHour = "19:00",
                ImageFile = file
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberShopAsync(formModel, userId, wwwRootPath), "You need to be a barber in order to do this action.");

            //User not existing shouldn't be possible by default in order to become a barber you should be registered user
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberShopAsync(formModel, FakeId.ToString(), wwwRootPath), "User does not exist.");

            var deletedUser = new User() { Id = "DeletedUser", FirstName = "", LastName = "", Email = "", PhoneNumber = "", IsDeleted = true, DeleteDate = DateTime.UtcNow };

            dbContextWithSeededData.Users.Add(deletedUser);
            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberShopAsync(formModel, deletedUser.Id, wwwRootPath), "User does not exist.");
        }

        [Test]
        public async Task TestAddBarberShop_DeletedBarberShopShouldBeRestoredWhenTheAddedOneIsTheSame()
        {
            var userId = BarberUserId.ToString();

            var deletedBarberShop = dbContextWithSeededData.BarberShops.First(b => b.IsDeleted);

            var formModel = new BarberShopAddFormModel()
            {
                Name = deletedBarberShop.Name,
                City = deletedBarberShop.City.Name,
                District = deletedBarberShop.District.Name,
                Street = "Deleted 2", // Testing the GetStreetNameAndNumberCaseInsensitive method
                StartHour = "10:30",
                FinishHour = "19:00",
                ImageFile = file
            };

            await service.AddBarberShopAsync(formModel, userId, wwwRootPath);

            Assert.True(dbContextWithSeededData.BarberShops.Any(b => b.Id == deletedBarberShop.Id && !b.IsDeleted && b.DeleteDate == null));
            //Test the added image
            Assert.True(dbContextWithSeededData.BarberShops.Any(b => b.Id == deletedBarberShop.Id && b.ImageName.Contains(NewTestImage.Replace(".jpg", ""))));

            Assert.False(dbContextWithSeededData.BarberShops.Any(b => b.IsDeleted && b.DeleteDate == null));
            Assert.True(dbContextWithSeededData.BarberShops.Count() == 3);
        }

        [Test]
        public async Task TestAddBarberShop_ShouldAddBarberShopWithNewCityAndDistrict()
        {
            var userId = BarberUserId.ToString();

            var formModel = new BarberShopAddFormModel()
            {
                Name = "NewTestBarberShop",
                City = "varna",
                District = "Chaika",
                Street = "st. Test 1",
                StartHour = "10:30",
                FinishHour = "19:00",
                ImageFile = file
            };

            await service.AddBarberShopAsync(formModel, userId, wwwRootPath);

            Assert.True(dbContextWithSeededData.BarberShops.Any(b => b.Name == formModel.Name && b.District.Name == formModel.District && b.Street == formModel.Street));
            //Test the added image
            Assert.True(dbContextWithSeededData.BarberShops.Any(b => b.Name == formModel.Name && b.ImageName.Contains(NewTestImage.Replace(".jpg", ""))));

            Assert.True(dbContextWithSeededData.BarberShops.Any(b => b.StartHour.ToString(@"hh\:mm") == formModel.StartHour && b.FinishHour.ToString(@"hh\:mm") == formModel.FinishHour));
            // varna from form input should be saved as Varna in db
            Assert.True(dbContextWithSeededData.Cities.Any(c => c.Name == "Varna"));
            Assert.True(dbContextWithSeededData.BarberShops.Count() == 4);
        }

        [Test]
        public async Task TestMineBarberShops_ShouldReturnModelWithCorrectData()
        {
            var userId = BarberShopOwnerUserId.ToString();

            var model = await service.MineBarberShopsAsync(userId, 0);

            var barberShops = dbContextWithSeededData.BarberShops.Where(b => !b.IsDeleted).ToList();

            Assert.True(model.TotalBarberShops == 2);

            foreach (var barbershop in barberShops)
            {
                Assert.True(model.BarberShops.Any(b => b.Id == barbershop.Id));
            }
            Assert.True(model.BarberShops.All(b => b.UserIsEmployee));
            Assert.True(model.BarberShops.All(b => b.UserIsOwner));
        }

        [Test]
        public async Task TestMineBarberShops_ShouldNotReturnDeletedBarberShopsButShouldReturnPrivateOnes()
        {
            var userId = BarberShopOwnerUserId.ToString();

            var model = await service.MineBarberShopsAsync(userId, 0);

            //Deleted barbershop
            var deletedBarberShop = dbContextWithSeededData.BarberShops.Where(b => b.IsDeleted).First();

            Assert.False(model.BarberShops.Any(b => b.Id == deletedBarberShop.Id));

            //Private Barbershop
            var barbershopPrivate = dbContextWithSeededData.BarberShops.Where(b => !b.IsDeleted).First();

            barbershopPrivate.IsPublic = false;

            dbContextWithSeededData.SaveChanges();

            model = await service.MineBarberShopsAsync(userId, 0);

            Assert.True(model.BarberShops.Any(b => b.Id == barbershopPrivate.Id));

            Assert.True(model.TotalBarberShops == 2);
            Assert.True(model.BarberShops.All(b => b.UserIsEmployee));
            Assert.True(model.BarberShops.All(b => b.UserIsOwner));
        }

        [Test]
        public async Task TestMineBarberShops_PageingShouldWork()
        {
            var userId = BarberShopOwnerUserId.ToString();

            var model = await service.MineBarberShopsAsync(userId, 2);

            Assert.False(model.BarberShops.Any());
        }

        [Test]
        public async Task TestDisplayBarberShopInfo_ShouldReturnModelWithCorrectData()
        {
            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            var model = await service.DisplayBarberShopInfoAsync(barberShop.Id);

            Assert.True(model.Name == barberShop.Name && model.District == barberShop.District.Name && model.City == barberShop.City.Name);
            Assert.True(model.Street == barberShop.Street);
            Assert.True(model.StartHour == barberShop.StartHour.ToString(@"hh\:mm") && model.FinishHour == barberShop.FinishHour.ToString(@"hh\:mm"));
        }

        [Test]
        public void TestDisplayBarberShopInfo_ShouldThrowModelStateCustomExceptionWhenBarberShopIsInvalid()
        {
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.DisplayBarberShopInfoAsync(FakeId), "This barbershop does not exist");
        }

        [Test]
        public async Task TestEditBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberShopIsInvalidOrDeleted()
        {
            var userId = BarberShopOwnerUserId.ToString();

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            var deletedBarberShop = dbContextWithSeededData.BarberShops.First(b => b.IsDeleted);

            var formModel = await service.DisplayBarberShopInfoAsync(barberShop.Id);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.EditBarberShopAsync(formModel, FakeId, userId, false, wwwRootPath), "This barbershop does not exist");
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.EditBarberShopAsync(formModel, deletedBarberShop.Id, userId, false, wwwRootPath), "This barbershop does not exist");
        }

        [Test]
        public async Task TestEditBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberIsInvalidOrDeletedOrIsNotOwner()
        {
            var userId = BarberUserId.ToString();

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            var formModel = await service.DisplayBarberShopInfoAsync(barberShop.Id);

            //Not employee at the shop
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.EditBarberShopAsync(formModel, barberShop.Id, userId, false, wwwRootPath), $"You have to be a owner of ${barberShop.Name} to do this action");

            //Not owner of the shop
            userId = BarberShopOwnerUserId.ToString();

            var barberInShop = barberShop.Barbers.First(b => b.IsOwner && b.Barber.UserId == userId);

            //Remove the only owner
            barberInShop.IsOwner = false;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.EditBarberShopAsync(formModel, barberShop.Id, userId, false, wwwRootPath), $"You have to be a owner of ${barberShop.Name} to do this action");

            //Deleted barber
            barberInShop = barberShop.Barbers.First(b => !b.IsOwner && b.Barber.UserId == userId);

            //Restore the owner
            barberInShop.IsOwner = true;

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == userId);

            barber.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.EditBarberShopAsync(formModel, barberShop.Id, userId, false, wwwRootPath), $"You have to be a owner of ${barberShop.Name} to do this action");
        }

        [Test]
        public void TestEditBarberShop_ShouldThrowModelStateCustomExceptionWhenHourInputIsIncorrect()
        {
            var userId = BarberShopOwnerUserId.ToString();

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            var formModel = new BarberShopEditFormModel()
            {
                Name = "NewTestBarberShop",
                City = "varna",
                District = "Chaika",
                Street = "st. Test 1",
                StartHour = "10-30",
                FinishHour = "19:00"
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.EditBarberShopAsync(formModel, barberShop.Id, userId, false, wwwRootPath), "Start hour input is incorect.");

            formModel = new BarberShopEditFormModel()
            {
                Name = "NewTestBarberShop",
                City = "varna",
                District = "Chaika",
                Street = "st. Test 1",
                StartHour = "10:30",
                FinishHour = "19-00"
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.EditBarberShopAsync(formModel, barberShop.Id, userId, false, wwwRootPath), "Finish hour input is incorect.");
        }

        [Test]
        public void TestEditBarberShop_ShouldThrowModelStateCustomExceptionWhenStartHourInputIsBiggerThanTheFinishOne()
        {
            var userId = BarberShopOwnerUserId.ToString();

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            var formModel = new BarberShopEditFormModel()
            {
                Name = "NewTestBarberShop",
                City = "varna",
                District = "Chaika",
                Street = "st. Test 1",
                StartHour = "19:30",
                FinishHour = "19:00"
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.EditBarberShopAsync(formModel, barberShop.Id, userId, false, wwwRootPath), "Opening hour must be smaller that the closing hour. And can not be the same as the closing hour");

            formModel = new BarberShopEditFormModel()
            {
                Name = "NewTestBarberShop",
                City = "varna",
                District = "Chaika",
                Street = "st. Test 1",
                StartHour = "10:30",
                FinishHour = "10:30"
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.EditBarberShopAsync(formModel, barberShop.Id, userId, false, wwwRootPath), "Opening hour must be smaller that the closing hour. And can not be the same as the closing hour");
        }

        [Test]
        public async Task TestEditBarberShop_ShouldEditTheBarberShopInformation()
        {
            var userId = BarberShopOwnerUserId.ToString();

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted && b.DistrictId == 1);

            var formModel = new BarberShopEditFormModel()
            {
                Name = "NewTestBarberShop",
                City = "varna",
                District = "Chaika",
                Street = "st. Test 1",
                StartHour = "10:30",
                FinishHour = "19:00",
                ImageFile = file
            };

            Assert.False(dbContextWithSeededData.Cities.Any(c => c.Name == "Varna" || c.Name == formModel.City));

            var oldDistrictId = barberShop.District.Id;

            await service.EditBarberShopAsync(formModel, barberShop.Id, userId, false, wwwRootPath);

            //city should be saved as Varna
            Assert.True(dbContextWithSeededData.Cities.Any(c => c.Name == "Varna"));
            Assert.True(dbContextWithSeededData.Districts.Any(d => d.Name == formModel.District));

            //old distric should be removed cos it is not used anymore but city should not be removed
            Assert.True(dbContextWithSeededData.Cities.Any(c => c.Name == barberShop.City.Name));
            Assert.False(dbContextWithSeededData.Districts.Any(d => d.Id == oldDistrictId));

            Assert.True(barberShop.IsPublic == false && barberShop.IsDeleted == false);
            Assert.True(barberShop.Name == formModel.Name && barberShop.Street == formModel.Street);
            //Test editing the image
            Assert.True(barberShop.ImageName.Contains(NewTestImage.Replace(".jpg", "")));
            //city should be saved as Varna
            Assert.True(barberShop.City.Name == "Varna");
            Assert.True(barberShop.District.Name == formModel.District);
            Assert.True(formModel.StartHour == barberShop.StartHour.ToString(@"hh\:mm") && formModel.FinishHour == barberShop.FinishHour.ToString(@"hh\:mm"));
        }

        [Test]
        public void TestDeleteBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberShopIsInvalidOrDeleted()
        {
            var userId = BarberShopOwnerUserId.ToString();

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            var deletedBarberShop = dbContextWithSeededData.BarberShops.First(b => b.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.DeleteBarberShopAsync(FakeId, userId, false, wwwRootPath), "This barbershop does not exist");
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.DeleteBarberShopAsync(deletedBarberShop.Id, userId, false, wwwRootPath), "This barbershop does not exist");
        }

        [Test]
        public void TestDeleteBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberIsInvalidOrDeletedOrIsNotOwner()
        {
            var userId = BarberUserId.ToString();

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            //Not employee at the shop
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.DeleteBarberShopAsync(barberShop.Id, userId, false, wwwRootPath), $"You have to be a owner of ${barberShop.Name} to do this action");

            //Not owner of the shop
            userId = BarberShopOwnerUserId.ToString();

            var barberInShop = barberShop.Barbers.First(b => b.IsOwner && b.Barber.UserId == userId);

            //Remove the only owner
            barberInShop.IsOwner = false;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.DeleteBarberShopAsync(barberShop.Id, userId, false, wwwRootPath), $"You have to be a owner of ${barberShop.Name} to do this action");

            //Deleted barber
            barberInShop = barberShop.Barbers.First(b => !b.IsOwner && b.Barber.UserId == userId);

            //Restore the owner
            barberInShop.IsOwner = true;

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == userId);

            barber.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.DeleteBarberShopAsync(barberShop.Id, userId, false, wwwRootPath), $"You have to be a owner of ${barberShop.Name} to do this action");
        }

        [Test]
        public async Task TestDeleteBarberShop_ShouldDeleteBarberShop()
        {
            var userId = BarberShopOwnerUserId.ToString();

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted && b.Id == 2);

            Assert.True(barberShop.Orders.All(o => !o.IsDeleted && o.DeleteDate == null));
            Assert.True(barberShop.Services.Any());
            Assert.True(barberShop.Barbers.Any());
            Assert.False(dbContextWithSeededData.Services.All(s => s.IsDeleted));

            await service.DeleteBarberShopAsync(barberShop.Id, userId, false, wwwRootPath);

            Assert.True(dbContextWithSeededData.BarberShops.Any(b => b.Id == barberShop.Id && b.IsDeleted && b.IsPublic == false && b.DeleteDate != null));
            //Should delete the image
            Assert.True(dbContextWithSeededData.BarberShops.Any(b => b.Id == barberShop.Id && b.ImageName == null));
            //When barbershop is marked as deleted orders in the shop are marked as deleted 
            Assert.True(barberShop.Orders.All(o => o.IsDeleted && o.DeleteDate != null));
            //Services and barbers in the barbershop are removed
            Assert.False(barberShop.Services.Any());
            Assert.False(barberShop.Barbers.Any());
            //When barbershop is deleted and service in the db is no longer used by any barbershop it is marked deleted
            Assert.True(dbContextWithSeededData.Services.All(s => s.IsDeleted && s.DeleteDate != null));
        }

        [Test]
        public void TestBarberShopInformation_ShouldThrowModelStateCustomExceptionWhenBarberShopIsInvalidOrDeleted()
        {
            var userId = BarberShopOwnerUserId.ToString();

            var deletedBarberShop = dbContextWithSeededData.BarberShops.First(b => b.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.BarberShopInformationAsync(userId, FakeId), "You have to be owner to do this action");
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.BarberShopInformationAsync(userId, deletedBarberShop.Id), "You have to be owner to do this action");
        }

        [Test]
        public async Task TestBarberShopInformation_ShouldThrowModelStateCustomExceptionWhenBarberIsInvalidOrDeletedOrIsNotOwner()
        {
            var userId = BarberUserId.ToString();

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            var formModel = await service.DisplayBarberShopInfoAsync(barberShop.Id);

            //Not employee at the shop
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.BarberShopInformationAsync(userId, barberShop.Id), "You have to be owner to do this action");

            //Not owner of the shop
            userId = BarberShopOwnerUserId.ToString();

            var barberInShop = barberShop.Barbers.First(b => b.IsOwner && b.Barber.UserId == userId);

            //Remove the only owner
            barberInShop.IsOwner = false;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.BarberShopInformationAsync(userId, barberShop.Id), "You have to be owner to do this action");

            //Deleted barber
            barberInShop = barberShop.Barbers.First(b => !b.IsOwner && b.Barber.UserId == userId);

            //Restore the owner
            barberInShop.IsOwner = true;

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == userId);

            barber.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.BarberShopInformationAsync(userId, barberShop.Id), "You have to be owner to do this action");
        }

        [Test]
        public async Task TestBarberShopInformation_ShouldReturnModelWithCorrectInformation()
        {
            var userId = BarberShopOwnerUserId.ToString();

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            var model = await service.BarberShopInformationAsync(userId, barberShop.Id);

            var barber = dbContextWithSeededData.Barbers.Where(b => b.UserId == userId).First();

            Assert.True(model.BarberShopName == barberShop.Name && model.BarberShopId == barberShop.Id);
            Assert.True(model.Barbers.Count() == barberShop.Barbers.Count);
            Assert.True(model.Barbers.All(b => b.BarberName == barber.FirstName + ' ' + barber.LastName));
            Assert.True(model.Barbers.All(b => b.BarberId == barber.Id && b.UserId == userId));
            Assert.True(model.Barbers.All(b => b.IsAvailable == true && b.IsOwner == true));
        }

        [Test]
        public async Task TestGetBarberShopNameToFriendlyUrl_ShouldReturnCorrectString()
        {
            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            barberShop.Name = "New Test Barbershop";

            dbContextWithSeededData.SaveChanges();

            var nameToFriendlyUrl = await service.GetBarberShopNameToFriendlyUrlAsync(barberShop.Id);

            Assert.True("New-Test-Barbershop" == nameToFriendlyUrl);
        }

        [Test]
        public void TestMakeBarberShopPublic_ShouldThrowModelStateCustomExceptionWhenBabrberShopIsNonExistent()
        {
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberShopPublicAsync(FakeId), "This barbershop does not exist");
        }

        [Test]
        public async Task TestMakeBarberShopPublic_ShouldMakeBarberShopPublic()
        {
            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            barberShop.IsPublic = false;

            dbContextWithSeededData.SaveChanges();

            Assert.False(barberShop.IsPublic);

            await service.MakeBarberShopPublicAsync(barberShop.Id);

            Assert.True(barberShop.IsPublic);
        }

        [Test]
        public void TestMakeBarberShopPrivate_ShouldThrowModelStateCustomExceptionWhenBabrberShopIsNonExistent()
        {
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberShopPrivateAsync(FakeId), "This barbershop does not exist");
        }

        [Test]
        public async Task TestMakeBarberShopPrivate_ShouldMakeBarberShopPrivate()
        {
            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            barberShop.IsPublic = true;

            dbContextWithSeededData.SaveChanges();

            Assert.True(barberShop.IsPublic);

            await service.MakeBarberShopPrivateAsync(barberShop.Id);

            Assert.False(barberShop.IsPublic);
        }
    }
}
