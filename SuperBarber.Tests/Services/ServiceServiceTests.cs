using SuperBarber.Core.Extensions;
using SuperBarber.Core.Models.Service;
using SuperBarber.Core.Services.Service;
using SuperBarber.Infrastructure.Data.Models;
using SuperBarber.Services.Service;
using SuperBarber.Tests.Common;
using static SuperBarber.Tests.Common.CreateTestDb;

namespace SuperBarber.Tests.Services
{
    public class ServiceServiceTests : BaseTest
    {
        private IServiceService service;
        private const int FakeId = 100;
        private BarberShop barberShop;
        private string userId;

        [SetUp]
        public void TestInitialize()
        {
            service = new ServiceService(dbContextWithSeededData);

            barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted && b.Services.Any());
            userId = BarberShopOwnerUserId.ToString();
        }

        [Test]
        public async Task TestGetServiceCategories_ShouldReturnDataWithCorrectInformation()
        {
            var categories = await service.GetServiceCategoriesAsync();

            var categoriesDb = dbContextWithSeededData.Categories.ToList();

            foreach (var category in categoriesDb)
            {
                Assert.True(categories.Any(c => c.Id == category.Id));
                Assert.True(categories.Any(c => c.Name == category.Name));
            }
        }

        [Test]
        public void TestAddService_ShouldThrowModelStateCustomExceptionWhenCategoryIdIsNonExistent()
        {
            var model = new AddServiceFormModel() { Name = "Hair cut test", CategoryId = FakeId, Price = 20 };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddServiceAsync(model, userId, barberShop.Id), "Category does not exist");
        }

        [Test]
        public void TestAddService_ShouldThrowModelStateCustomExceptionWhenBarberShopIsNonExistent()
        {
            var model = new AddServiceFormModel() { Name = "Hair cut test", CategoryId = 1, Price = 20 };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddServiceAsync(model, userId, FakeId), "You are not the owner of this barbershop");
        }

        [Test]
        public void TestAddService_ShouldThrowModelStateCustomExceptionWhenBarberIsNotOwnerOrNonExistent()
        {
            var model = new AddServiceFormModel() { Name = "Hair cut test", CategoryId = 1, Price = 20 };

            //Barber is not working at this shop
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddServiceAsync(model, BarberUserId.ToString(), barberShop.Id), "You are not the owner of this barbershop");

            //Barber is not owner

            var barberInShop = barberShop.Barbers.First();

            barberInShop.IsOwner = false;

            dbContextWithSeededData.SaveChanges();

            Assert.False(barberInShop.IsOwner);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddServiceAsync(model, userId, barberShop.Id), "You are not the owner of this barbershop");

            //Barber is deleted
            barberInShop.IsOwner = true;

            var barber = barberShop.Barbers.First().Barber;

            barber.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.True(barberInShop.IsOwner);
            Assert.True(barber.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddServiceAsync(model, userId, barberShop.Id), "You are not the owner of this barbershop");
        }

        [Test]
        public void TestAddService_ShouldThrowModelStateCustomExceptionWhenServiceIsAllReadyExistentInBarberShop()
        {
            var model = new AddServiceFormModel() { Name = "hair Cut ", CategoryId = 1, Price = 20 };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddServiceAsync(model, userId, barberShop.Id), "This service already exists in your barber shop");
        }

        [Test]
        public async Task TestAddService_ShouldMarkAsNotDeletedIfItFindsTheSameServiceAndAddItBackToTheBarberShopServices()
        {
            var model = new AddServiceFormModel() { Name = "hair Cut  ", CategoryId = 1, Price = 20 };

            var serviceToDelete = dbContextWithSeededData.Services.First();

            //Deletes the service from the barbershop
            serviceToDelete.IsDeleted = true;
            serviceToDelete.DeleteDate = DateTime.UtcNow;
            barberShop.Services.Clear();

            dbContextWithSeededData.SaveChanges();

            Assert.True(serviceToDelete.IsDeleted);

            await service.AddServiceAsync(model, userId, barberShop.Id);

            Assert.False(serviceToDelete.IsDeleted);
            Assert.True(barberShop.Services.Count == 1);
        }

        [Test]
        public async Task TestAddService_ShouldAddNewServiceToTheBarberShop()
        {
            var model = new AddServiceFormModel() { Name = "Hair Cut test", CategoryId = 1, Price = 20 };

            await service.AddServiceAsync(model, userId, barberShop.Id);

            Assert.True(barberShop.Services.Count == 2);
            Assert.True(barberShop.Services.Any(s => s.Price == model.Price));
            Assert.True(barberShop.Services.Any(s => s.Service.Name == model.Name));
            Assert.True(barberShop.Services.Any(s => s.Service.CategoryId == model.CategoryId));
        }

        [Test]
        public void TestShowServices_ShouldThrowModelStateCustomExceptionWhenServiceIsNonExistent()
        {
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.ShowServicesAsync(FakeId), "This barbershop does not exist");
        }

        [Test]
        public async Task TestShowServices_ShouldReturnServicesInTheBarberShop()
        {
            var model = await service.ShowServicesAsync(barberShop.Id);

            Assert.True(model.Any());

            foreach (var service in barberShop.Services)
            {
                Assert.True(model.Any(s => s.ServiceId == service.ServiceId));
            }
        }

        [Test]
        public async Task TestShowServices_ShouldNotReturnDeltedServicesFromTheBarberShop()
        {
            //Deletes the services in the barbershop
            barberShop.Services.Clear();

            dbContextWithSeededData.SaveChanges();

            var model = await service.ShowServicesAsync(barberShop.Id);

            Assert.False(model.Any());
        }

        [Test]
        public void TestRemoveService_ShouldThrowModelStateCustomExceptionWhenBarberShopIsNonExistent()
        {
            var serviceId = barberShop.Services.First().ServiceId;

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveServiceAsync(FakeId, serviceId, userId, false), "This barbershop does not exist");
        }

        [Test]
        public void TestRemoveService_ShouldThrowModelStateCustomExceptionWhenBarberIsNotOwnerOrNonExistent()
        {
            var serviceId = barberShop.Services.First().ServiceId;

            //Barber is not working at this shop
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveServiceAsync(barberShop.Id, serviceId, BarberUserId.ToString(), false), $"You have to be owner of {barberShop.Name} to perform this action");

            //Barber is not owner

            var barberInShop = barberShop.Barbers.First();

            barberInShop.IsOwner = false;

            dbContextWithSeededData.SaveChanges();

            Assert.False(barberInShop.IsOwner);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveServiceAsync(barberShop.Id, serviceId, userId, false), $"You have to be owner of {barberShop.Name} to perform this action");

            //Barber is deleted
            barberInShop.IsOwner = true;

            var barber = barberShop.Barbers.First().Barber;

            barber.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.True(barberInShop.IsOwner);
            Assert.True(barber.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveServiceAsync(barberShop.Id, serviceId, userId, false), $"You have to be owner of {barberShop.Name} to perform this action");
        }

        [Test]
        public void TestRemoveService_ShouldThrowModelStateCustomExceptionWhenServiceIsNotExistentInBarberShop()
        {
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveServiceAsync(barberShop.Id, FakeId, userId, false), $"This service does not exist in {barberShop.Name}");
        }



        [Test]
        public async Task TestRemoveService_ShouldRemoveTheServiceFromTheBarberShopAndIfServiceIsNotUsedAnywhereMarkItAsDeleted()
        {
            var serviceId = barberShop.Services.First().ServiceId;

            await service.RemoveServiceAsync(barberShop.Id, serviceId, userId, false);

            Assert.False(barberShop.Services.Any());
            Assert.True(dbContextWithSeededData.Services.Any(s => s.Id == serviceId && s.IsDeleted && s.DeleteDate != null));
        }

        [Test]
        public async Task TestRemoveService_ShouldReturnNullWhenBarberShopIsNonExistent()
        {
            var stringResult = await service.GetBarberShopNameToFriendlyUrlAsync(FakeId);

            Assert.IsNull(stringResult);
        }

        [Test]
        public async Task TestGetBarberShopNameToFriendlyUrl_ShouldReturnTheCorrectString()
        {
            var stringResult = await service.GetBarberShopNameToFriendlyUrlAsync(barberShop.Id);

            var barbershopNameTest = barberShop.Name.Replace(' ', '-');

            Assert.True(stringResult == barbershopNameTest);
        }
    }
}
