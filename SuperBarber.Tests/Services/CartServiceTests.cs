using SuperBarber.Core.Extensions;
using SuperBarber.Core.Models.Cart;
using SuperBarber.Core.Models.Service;
using SuperBarber.Core.Services.Cart;
using SuperBarber.Infrastructure.Data.Models;
using SuperBarber.Tests.Common;
using System.Globalization;
using static SuperBarber.Tests.Common.CreateTestDb;

namespace SuperBarber.Tests.Services
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

        [Test]
        public void TestAddOrder_ShouldThrowModelStateCustomExceptionWhenTimeInputOrDateInputIsIncorrect()
        {
            var model = new BookServiceFormModel()
            {
                Date = DateTime.UtcNow.AddDays(1).ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture),
                Time = "12-00"
            };

            var cartList = new List<ServiceListingViewModel>();

            var barberShopService = dbContextWithSeededData.BarberShops
                .First(b => !b.IsDeleted && b.IsPublic && b.Services.Any())
                .Services.First();

            cartList.Add(new ServiceListingViewModel()
            {
                BarberShopId = barberShopService.BarberShopId,
                ServiceId = barberShopService.ServiceId,
                Price = barberShopService.Price,
                Category = barberShopService.Service.Category.Name,
                BarberShopName = barberShopService.BarberShop.Name,
                ServiceName = barberShopService.Service.Name
            });

            var guestUserId = GuestUserId.ToString();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOrderAsync(model, cartList, guestUserId), "Hour input is incorect.");

            model = new BookServiceFormModel()
            {
                Date = "22:08:12",
                Time = "12:00"
            };

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOrderAsync(model, cartList, guestUserId), "Date input is incorect.");
        }

        [Test]
        public void TestAddOrder_ShouldThrowModelStateCustomExceptionWhenTimeIsOutOfTheWorkingDay()
        {
            var model = new BookServiceFormModel()
            {
                Date = DateTime.UtcNow.AddDays(1).ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture),
                Time = "23:00"
            };

            var cartList = new List<ServiceListingViewModel>();

            var barberShopService = dbContextWithSeededData.BarberShops
                .First(b => !b.IsDeleted && b.IsPublic && b.Services.Any())
                .Services.First();

            cartList.Add(new ServiceListingViewModel()
            {
                BarberShopId = barberShopService.BarberShopId,
                ServiceId = barberShopService.ServiceId,
                Price = barberShopService.Price,
                Category = barberShopService.Service.Category.Name,
                BarberShopName = barberShopService.BarberShop.Name,
                ServiceName = barberShopService.Service.Name
            });

            var guestUserId = GuestUserId.ToString();

            var barberShop = dbContextWithSeededData.BarberShops.Find(barberShopService.BarberShopId);

            var timeArr = model.Time.Split(':');

            var ts = new TimeSpan(int.Parse(timeArr[0]), int.Parse(timeArr[1]), 0);

            Assert.False(barberShop.StartHour <= ts && barberShop.FinishHour >= ts);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOrderAsync(model, cartList, guestUserId));
        }

        [Test]
        public void TestAddOrder_ShouldThrowModelStateCustomExceptionWhenBarbershopIsDeletedIsPrivateOrNonExistent()
        {
            var model = new BookServiceFormModel()
            {
                Date = DateTime.UtcNow.AddDays(1).ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture),
                Time = "13:00"
            };

            var cartList = new List<ServiceListingViewModel>();

            var barberShopService = dbContextWithSeededData.BarberShops
                .First(b => !b.IsDeleted && b.IsPublic && b.Services.Any())
                .Services.First();

            cartList.Add(new ServiceListingViewModel()
            {
                BarberShopId = barberShopService.BarberShopId,
                ServiceId = barberShopService.ServiceId,
                Price = barberShopService.Price,
                Category = barberShopService.Service.Category.Name,
                BarberShopName = barberShopService.BarberShop.Name,
                ServiceName = barberShopService.Service.Name
            });

            var guestUserId = GuestUserId.ToString();

            var barberShop = dbContextWithSeededData.BarberShops.Find(barberShopService.BarberShopId);

            //Private barbershop
            barberShop.IsPublic = false;

            dbContextWithSeededData.SaveChanges();

            Assert.False(barberShop.IsPublic);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOrderAsync(model, cartList, guestUserId));

            //Deleted Barbershop

            barberShop.IsPublic = true;
            barberShop.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.True(barberShop.IsPublic);
            Assert.True(barberShop.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOrderAsync(model, cartList, guestUserId));

            //Fake barbershop

            cartList.Clear();

            cartList.Add(new ServiceListingViewModel()
            {
                BarberShopId = FakeId,
                ServiceId = barberShopService.ServiceId,
                Price = barberShopService.Price,
                Category = barberShopService.Service.Category.Name,
                BarberShopName = barberShopService.BarberShop.Name,
                ServiceName = barberShopService.Service.Name
            });

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOrderAsync(model, cartList, guestUserId));
        }

        [Test]
        public void TestAddOrder_ShouldThrowModelStateCustomExceptionWhenBarbershopDosentContainTheService()
        {
            var model = new BookServiceFormModel()
            {
                Date = DateTime.UtcNow.AddDays(1).ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture),
                Time = "13:00"
            };

            var cartList = new List<ServiceListingViewModel>();

            var barberShopService = dbContextWithSeededData.BarberShops
                .First(b => !b.IsDeleted && b.IsPublic && b.Services.Any())
                .Services.First();

            //Fake service
            cartList.Add(new ServiceListingViewModel()
            {
                BarberShopId = barberShopService.BarberShopId,
                ServiceId = FakeId,
                Price = barberShopService.Price,
                Category = barberShopService.Service.Category.Name,
                BarberShopName = barberShopService.BarberShop.Name,
                ServiceName = barberShopService.Service.Name
            });

            var guestUserId = GuestUserId.ToString();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOrderAsync(model, cartList, guestUserId));

            //Barshop has deleted the service

            cartList.Clear();

            barberShopService.Service.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.True(barberShopService.Service.IsDeleted);

            cartList.Add(new ServiceListingViewModel()
            {
                BarberShopId = barberShopService.BarberShopId,
                ServiceId = barberShopService.ServiceId,
                Price = barberShopService.Price,
                Category = barberShopService.Service.Category.Name,
                BarberShopName = barberShopService.BarberShop.Name,
                ServiceName = barberShopService.Service.Name
            });

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOrderAsync(model, cartList, guestUserId));
        }

        [Test]
        public void TestAddOrder_ShouldThrowModelStateCustomExceptionWhenDateToBookIsSmallerThanTheMinimumDate()
        {
            var model = new BookServiceFormModel()
            {
                Date = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture),
                Time = DateTime.UtcNow.Hour.ToString() + ":00"
            };

            var cartList = new List<ServiceListingViewModel>();

            var barberShopService = dbContextWithSeededData.BarberShops
                .First(b => !b.IsDeleted && b.IsPublic && b.Services.Any())
                .Services.First();

            cartList.Add(new ServiceListingViewModel()
            {
                BarberShopId = barberShopService.BarberShopId,
                ServiceId = barberShopService.ServiceId,
                Price = barberShopService.Price,
                Category = barberShopService.Service.Category.Name,
                BarberShopName = barberShopService.BarberShop.Name,
                ServiceName = barberShopService.Service.Name
            });

            var guestUserId = GuestUserId.ToString();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOrderAsync(model, cartList, guestUserId));
        }

        [Test]
        public void TestAddOrder_ShouldThrowModelStateCustomExceptionWhenBarberIsDeletedOrUnavalible()
        {
            var model = new BookServiceFormModel()
            {
                Date = DateTime.UtcNow.AddDays(1).ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture),
                Time = "13:00"
            };

            var cartList = new List<ServiceListingViewModel>();

            var barberShopService = dbContextWithSeededData.BarberShops
                .First(b => !b.IsDeleted && b.IsPublic && b.Services.Any())
                .Services.First();

            cartList.Add(new ServiceListingViewModel()
            {
                BarberShopId = barberShopService.BarberShopId,
                ServiceId = barberShopService.ServiceId,
                Price = barberShopService.Price,
                Category = barberShopService.Service.Category.Name,
                BarberShopName = barberShopService.BarberShop.Name,
                ServiceName = barberShopService.Service.Name
            });

            var barberShop = dbContextWithSeededData.BarberShops.Find(barberShopService.BarberShopId);

            var barber = barberShop.Barbers.First();

            barber.IsAvailable = false;

            dbContextWithSeededData.SaveChanges();

            Assert.False(barber.IsAvailable);
            Assert.False(barber.Barber.IsDeleted);

            var guestUserId = GuestUserId.ToString();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOrderAsync(model, cartList, guestUserId));

            barber.IsAvailable = true;
            barber.Barber.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.True(barber.IsAvailable);
            Assert.True(barber.Barber.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOrderAsync(model, cartList, guestUserId));
        }

        [Test]
        public void TestAddOrder_ShouldThrowModelStateCustomExceptionWhenBarberHasOrderForTheSameDate()
        {
            var model = new BookServiceFormModel()
            {
                Date = DateTime.UtcNow.AddDays(1).ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture),
                Time = "12:00"
            };

            var cartList = new List<ServiceListingViewModel>();

            var barberShopService = dbContextWithSeededData.BarberShops
                .First(b => !b.IsDeleted && b.IsPublic && b.Services.Any())
                .Services.First();

            cartList.Add(new ServiceListingViewModel()
            {
                BarberShopId = barberShopService.BarberShopId,
                ServiceId = barberShopService.ServiceId,
                Price = barberShopService.Price,
                Category = barberShopService.Service.Category.Name,
                BarberShopName = barberShopService.BarberShop.Name,
                ServiceName = barberShopService.Service.Name
            });

            var guestUserId = GuestUserId.ToString();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOrderAsync(model, cartList, guestUserId));
        }

        [Test]
        public async Task TestAddOrder_ShouldAddNewOrderToTheBarber()
        {
            var model = new BookServiceFormModel()
            {
                Date = DateTime.UtcNow.AddDays(1).ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture),
                Time = "13:00"
            };

            var cartList = new List<ServiceListingViewModel>();

            var barberShopService = dbContextWithSeededData.BarberShops
                .First(b => !b.IsDeleted && b.IsPublic && b.Services.Any())
                .Services.First();

            cartList.Add(new ServiceListingViewModel()
            {
                BarberShopId = barberShopService.BarberShopId,
                ServiceId = barberShopService.ServiceId,
                Price = barberShopService.Price,
                Category = barberShopService.Service.Category.Name,
                BarberShopName = barberShopService.BarberShop.Name,
                ServiceName = barberShopService.Service.Name
            });

            var guestUserId = GuestUserId.ToString();

            await service.AddOrderAsync(model, cartList, guestUserId);

            var barberShop = dbContextWithSeededData.BarberShops.Find(barberShopService.BarberShopId);

            var barber = barberShop.Barbers.First();

            Assert.True(barberShop.Orders.Count == 2);
            Assert.True(barberShop.Orders.Any(o => o.ServiceId == barberShopService.ServiceId
                            && o.BarberId == barber.BarberId
                            && o.IsDeleted == false
                            && o.DeleteDate == null
                            && o.UserId == guestUserId
                            && o.Price == barberShopService.Price));
        }

        [Test]
        public async Task TestGetBarberShopName_ShouldReturnNullWhenBarbershopIsDeletedOrNonExistent()
        {
            var deletedBarberShopId = dbContextWithSeededData.BarberShops.First(b => b.IsDeleted).Id;

            var barberShopName = await service.GetBarberShopNameAsync(deletedBarberShopId);

            Assert.IsNull(barberShopName);

            barberShopName = await service.GetBarberShopNameAsync(FakeId);

            Assert.IsNull(barberShopName);
        }

        [Test]
        public async Task TestGetBarberShopName_ShouldReturnNameOfTheBarberShop()
        {
            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            var barberShopName = await service.GetBarberShopNameAsync(barberShop.Id);

            Assert.IsNotNull(barberShopName);
            Assert.True(barberShopName == barberShop.Name);
        }

        [Test]
        public void TestGetBarberShopNameToFriendlyUrl_ShouldReturnNameOfTheBarberShopToFriendlyUrl()
        {
            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            barberShop.Name = "Test New Name";

            dbContextWithSeededData.SaveChanges();

            var barberShopNameResult = service.GetBarberShopNameToFriendlyUrl(barberShop.Name);

            Assert.True(barberShopNameResult == "Test-New-Name");
        }
    }
}
