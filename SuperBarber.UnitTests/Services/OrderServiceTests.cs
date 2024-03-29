﻿using SuperBarber.Core.Extensions;
using SuperBarber.Core.Services.Order;
using SuperBarber.Infrastructure.Data.Models;
using SuperBarber.UnitTests.Common;
using static SuperBarber.UnitTests.Common.CreateTestDb;
using static SuperBarber.Core.Extensions.ExeptionErrors;
using static SuperBarber.Core.Extensions.ExeptionErrors.OrderServiceErrors;

namespace SuperBarber.Tests.Services
{
    public class OrderServiceTests : BaseTest
    {
        private IOrderService service;
        private const int FakeId = 100;
        private BarberShop barberShop;
        private BarberShopBarbers barber;

        [SetUp]
        public void TestInitialize()
        {
            service = new OrderService(dbContextWithSeededData);

            barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted && b.IsPublic && b.Services.Any());

            barber = barberShop.Barbers.First();
        }

        [Test]
        public void TestRemoveOrder_ShouldThrowModelStateCustomExceptionWhenOrderIsNonExistent()
        {
            var barberUserId = BarberShopOwnerUserId.ToString();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveOrderAsync(FakeId.ToString(), barber.BarberId, barberUserId), OrderNonExistent);
        }

        [Test]
        public void TestRemoveOrder_ShouldThrowModelStateCustomExceptionWhenOrderIsNotAssignedToTheCurrentUserBarber()
        {
            var barberUserId = BarberShopOwnerUserId.ToString();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveOrderAsync(OrderId.ToString(), FakeId, barberUserId), UserIsNotTheAssignedBarber);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveOrderAsync(OrderId.ToString(), barber.BarberId, GuestUserId.ToString()), UserIsNotTheAssignedBarber);
        }

        [Test]
        public void TestRemoveOrder_ShouldThrowModelStateCustomExceptionWhenYouTryToCancelTheOrderAfterTheMinimumTime()
        {
            var barberUserId = BarberShopOwnerUserId.ToString();

            var order = dbContextWithSeededData.Orders.First();

            DateTime date;

            if (DateTime.UtcNow.Minute >= 30)
            {
                date = new DateTime
                (DateTime.UtcNow.Year,
                 DateTime.UtcNow.Month,
                 DateTime.UtcNow.Day,
                 DateTime.UtcNow.Hour,
                 DateTime.UtcNow.Minute - 30,
                 0
                 );
            }
            else
            {
                date = DateTime.UtcNow;
            }

            order.Date = date;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveOrderAsync(OrderId.ToString(), barber.BarberId, barberUserId), CancelOrderCanNoLongerBeDone);
        }

        [Test]
        public void TestRemoveOrder_ShouldThrowModelStateCustomExceptionWhenYouTryToCancelDeletedOrder()
        {
            var barberUserId = BarberShopOwnerUserId.ToString();

            var order = dbContextWithSeededData.Orders.First();

            order.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveOrderAsync(OrderId.ToString(), barber.BarberId, barberUserId), CancelOrderCanNoLongerBeDone);
        }

        [Test]
        public async Task TestRemoveOrder_ShouldDeleteTheBarberOrder()
        {
            var barberUserId = BarberShopOwnerUserId.ToString();

            await service.RemoveOrderAsync(OrderId.ToString(), barber.BarberId, barberUserId);

            var order = dbContextWithSeededData.Orders.First();

            Assert.True(order.IsDeleted);
            Assert.IsNotNull(order.DeleteDate);
        }

        [Test]
        public void TestRemoveYourOrder_ShouldThrowModelStateCustomExceptionWhenOrderIsNonExistent()
        {
            var userId = GuestUserId.ToString();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveYourOrderAsync(FakeId.ToString(), userId), OrderNonExistent);
        }

        [Test]
        public void TestRemoveYourOrder_ShouldThrowModelStateCustomExceptionWhenOrderIsNotTheOneThatMadeTheOrder()
        {
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveYourOrderAsync(OrderId.ToString(), FakeId.ToString()), UserDidNotMakeTheOrder);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveYourOrderAsync(OrderId.ToString(), BarberShopOwnerUserId.ToString()), UserDidNotMakeTheOrder);
        }

        [Test]
        public void TestRemoveYourOrder_ShouldThrowModelStateCustomExceptionWhenYouTryToCancelTheOrderAfterTheMinimumTime()
        {
            var userId = GuestUserId.ToString();

            var order = dbContextWithSeededData.Orders.First();

            DateTime date;

            if (DateTime.UtcNow.Minute >= 30)
            {
                date = new DateTime
                (DateTime.UtcNow.Year,
                 DateTime.UtcNow.Month,
                 DateTime.UtcNow.Day,
                 DateTime.UtcNow.Hour,
                 DateTime.UtcNow.Minute - 30,
                 0
                 );
            }
            else
            {
                date = DateTime.UtcNow;
            }

            order.Date = date;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveYourOrderAsync(OrderId.ToString(), userId), CancelOrderCanNoLongerBeDone);
        }

        [Test]
        public void TestRemoveYourOrder_ShouldThrowModelStateCustomExceptionWhenYouTryToCancelDeletedOrder()
        {
            var userId = GuestUserId.ToString();

            var order = dbContextWithSeededData.Orders.First();

            order.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveYourOrderAsync(OrderId.ToString(), userId), CancelOrderCanNoLongerBeDone);
        }

        [Test]
        public async Task TestRemoveYourOrder_ShouldDeleteTheBarberOrder()
        {
            var userId = GuestUserId.ToString();

            await service.RemoveYourOrderAsync(OrderId.ToString(), userId);

            var order = dbContextWithSeededData.Orders.First();

            Assert.True(order.IsDeleted);
            Assert.IsNotNull(order.DeleteDate);
        }

        [Test]
        public async Task TestGetMyOrders_ShouldReturnNullWhenUserHasNotMadeAnyOrders()
        {
            var userId = BarberShopOwnerUserId.ToString();

            var model = await service.GetMyOrdersAsync(userId, 0);

            Assert.False(model.Orders.Any());
        }

        [Test]
        public async Task TestGetMyOrders_PagingShouldWork()
        {
            var userId = GuestUserId.ToString();

            var model = await service.GetMyOrdersAsync(userId, 0);

            Assert.True(model.Orders.Any());

            model = await service.GetMyOrdersAsync(userId, 1);

            Assert.True(model.Orders.Any());

            model = await service.GetMyOrdersAsync(userId, 2);

            Assert.False(model.Orders.Any());
        }

        [Test]
        public async Task TestGetMyOrders_ShouldReturnModelWithCorrectData()
        {
            var userId = GuestUserId.ToString();

            var model = await service.GetMyOrdersAsync(userId, 0);

            Assert.True(model.Orders.Any());
            Assert.True(model.TotalOrders == 1);
            Assert.True(model.Orders.First().OrderId == OrderId.ToString());
            Assert.True(model.Orders.First().BarberFirstName == barber.Barber.FirstName);
            Assert.True(model.Orders.First().BarberShop == barberShop.Name);
        }
    }
}
