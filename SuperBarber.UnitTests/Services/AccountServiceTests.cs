using Microsoft.AspNetCore.Hosting;
using SuperBarber.Core.Extensions;
using SuperBarber.Core.Services.Account;
using SuperBarber.Core.Services.BarberShops;
using SuperBarber.Infrastructure.Data.Models;
using SuperBarber.UnitTests.Common;
using static SuperBarber.UnitTests.Common.CreateTestDb;

namespace SuperBarber.UnitTests.Services
{
    public class AccountServiceTests : BaseTest
    {
        private IAccountService service;
        private IBarberShopService barberShopService;
        private User user;
        private string wwwRootPath;

        [SetUp]
        public void TestInitialize()
        {
            barberShopService = new BarberShopService(dbContextWithSeededData, userManager, signInManager);
            service = new AccountService(dbContextWithSeededData, userManager, signInManager, barberShopService);
            user = dbContextWithSeededData.Users.Find(BarberShopOwnerUserId.ToString());
            wwwRootPath = webHostEnvironment.WebRootPath;
        }

        [Test]
        public async Task TestSetFirstAndLastName_ShouldSetTheNameOfTheUserAndTheBarber()
        {
            await service.SetFirstAndLastNameAsync(user, "New first name", "New last name");

            Assert.True(user.FirstName == "New first name");
            Assert.True(user.LastName == "New last name");

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == user.Id.ToString());
            Assert.True(barber.FirstName == "New first name");
            Assert.True(barber.LastName == "New last name");
        }

        
        [Test]
        public void TestDeleteBarber_ShouldThrowModelStateCustomExceptionWhenBarberIsNonExistent()
        {
            user = dbContextWithSeededData.Users.Find(GuestUserId.ToString());

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.DeleteBarberAsync(user, false, wwwRootPath), "This barber does not exist!");
        }

        [Test]
        public async Task TestDeleteBarber_ShouldMarkTheBarberAsDeltedButSaveHisPersonalInformation()
        {
            await service.DeleteBarberAsync(user, false, wwwRootPath);

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == user.Id.ToString());
            Assert.IsNotNull(barber.FirstName);
            Assert.IsNotNull(barber.LastName);
            Assert.IsNotNull(barber.Email);
            Assert.IsNotNull(barber.PhoneNumber);
            Assert.True(barber.IsDeleted);
            Assert.IsNotNull(barber.DeleteDate);
            Assert.False(barber.Orders.Any(o => !o.IsDeleted));
            Assert.False(barber.BarberShops.Any());
        }

        [Test]
        public async Task TestDeleteBarber_ShouldMarkTheBarberAsDeltedAndDeleteHisPersonalInformation()
        {
            await service.DeleteBarberAsync(user, true, wwwRootPath);

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == user.Id.ToString());
            Assert.Null(barber.FirstName);
            Assert.Null(barber.LastName);
            Assert.Null(barber.Email);
            Assert.Null(barber.PhoneNumber);
            Assert.True(barber.IsDeleted);
            Assert.IsNotNull(barber.DeleteDate);
            Assert.False(barber.Orders.Any(o => !o.IsDeleted));
            Assert.False(barber.BarberShops.Any());
        }

        [Test]
        public async Task TestDeleteUser_ShouldMarkTheUserAsDeltedAndDeleteHisPersonalInformation()
        {
            user = dbContextWithSeededData.Users.Find(GuestUserId.ToString());

            await service.DeleteUserAsync(user);

            Assert.Null(user.FirstName);
            Assert.Null(user.LastName);
            Assert.Null(user.Email);
            Assert.Null(user.PhoneNumber);
            Assert.True(user.IsDeleted);
            Assert.IsNotNull(user.DeleteDate);
            Assert.False(user.Orders.Any(o => !o.IsDeleted));
        }

        [Test]
        public async Task TestUpdateBarberEmail_ShouldUpdateTheEmailOfTheBarber()
        {
            await service.UpdateBarberEmailAsync(user, "newemail@email.com");

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == user.Id.ToString());
            Assert.True(barber.Email == "newemail@email.com");
        }

        [Test]
        public async Task TestUpdateBarberPhoneNumber_ShouldUpdateThePhoneNumberOfTheBarber()
        {
            await service.UpdateBarberPhoneNumberAsync(user, "+359 2 771 8915");

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == user.Id.ToString());
            Assert.True(barber.PhoneNumber == "+359 2 771 8915");
        }
    }
}
