using SuperBarber.Areas.Identity.Services.Account;
using SuperBarber.Data.Models;
using SuperBarber.Infrastructure;
using SuperBarber.Services.BarberShops;
using static SuperBarber.Tests.CreateTestDb;

namespace SuperBarber.Tests
{
    public class AccountServiceTests : BaseTest
    {
        private IAccountService service;
        private IBarberShopService barberShopService;
        private const int FakeId = 100;
        private User user;

        [SetUp]
        public void TestInitialize()
        {
            barberShopService = new BarberShopService(dbContextWithSeededData, userManager, signInManager);
            service = new AccountService(dbContextWithSeededData, userManager, signInManager, barberShopService);
            user = dbContextWithSeededData.Users.Find(BarberShopOwnerUserId.ToString());
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

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.DeleteBarberAsync(user, false), "This barber does not exist!");
        }

        [Test]
        public async Task TestDeleteBarber_ShouldMarkTheBarberAsDeltedButSaveHisPersonalInformation()
        {
            await service.DeleteBarberAsync(user, false);

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
            await service.DeleteBarberAsync(user, true);

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
