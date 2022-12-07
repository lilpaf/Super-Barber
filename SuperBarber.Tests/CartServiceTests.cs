using SuperBarber.Data.Models;
using SuperBarber.Infrastructure;
using SuperBarber.Services.Cart;
using static SuperBarber.Tests.CreateTestDb;

namespace SuperBarber.Tests
{
    public class CartServiceTests : BaseTest
    {
        private ICartService service;
        private const int FakeId = 100;

        [SetUp]
        public void TestInitialize()
        {
            service = new CartService(dbContextWithSeededData);
        }

        [Test]
        public async Task TestGetService_ShouldReturnModelWithCorrrectInformation()
        {
            var serviceInDb = dbContextWithSeededData.Services.First(s => !s.IsDeleted);

            var serviceResult = await service.GetServiceAsync(serviceInDb.Id);

            Assert.IsNotNull(serviceResult);
            Assert.True(serviceResult.Id == serviceInDb.Id);
            Assert.True(serviceResult.IsDeleted == serviceInDb.IsDeleted);
            Assert.True(serviceResult.Name == serviceInDb.Name);
            Assert.True(serviceResult.CategoryId == serviceInDb.CategoryId);
            Assert.True(serviceResult.DeleteDate == serviceInDb.DeleteDate);
        }
        
        [Test]
        public async Task TestGetService_ShouldReturnNullWhenServiceIsNonExistentOrDeleted()
        {
            var serviceInDb = dbContextWithSeededData.Services.First(s => !s.IsDeleted);

            serviceInDb.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.True(serviceInDb.IsDeleted == true);

            var serviceResult = await service.GetServiceAsync(serviceInDb.Id);

            Assert.IsNull(serviceResult);
            
            serviceResult = await service.GetServiceAsync(FakeId);

            Assert.IsNull(serviceResult);
        }
        
        [Test]
        public async Task TestBarberShopServicePrice_ShouldThrowModelStateCustomExceptionWhenServiceIsNonExistentOrDeletedOrNotInThisBarberShop()
        {
            var serviceInDb = dbContextWithSeededData.Services.First(s => !s.IsDeleted);

            var barbershop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted 
                && b.Services.Any(s => s.ServiceId == serviceInDb.Id));

            serviceInDb.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.True(serviceInDb.IsDeleted == true);
            var model = await service.GetServiceAsync(serviceInDb.Id);

            //Deleted service
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.BarberShopServicePriceAsync(barbershop.Id, serviceInDb.Id), "This barbershop dose not contain this service");
            
            //Fake service id
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.BarberShopServicePriceAsync(barbershop.Id, FakeId), "This barbershop dose not contain this service");

            //Service not in barbershop
            var newService = new Service()
            {
                Id = 2,
                IsDeleted = false,
                CategoryId = 1,
            };
            
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.BarberShopServicePriceAsync(barbershop.Id, newService.Id), "This barbershop dose not contain this service");
        }
        
        [Test]
        public async Task TestBarberShopServicePrice_ShouldReturnThePriceOfTheServiceInTheBarberShop()
        {
            var serviceInDb = dbContextWithSeededData.Services.First(s => !s.IsDeleted);

            var barbershop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted 
                && b.Services.Any(s => s.ServiceId == serviceInDb.Id));

            var servicePriceResult = await service.BarberShopServicePriceAsync(barbershop.Id, serviceInDb.Id);

            var servicePrice = barbershop.Services.First(s => s.ServiceId == serviceInDb.Id).Price;

            Assert.True(servicePriceResult == servicePrice);
        }
    }
}
