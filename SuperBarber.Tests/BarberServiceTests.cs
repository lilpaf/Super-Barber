using SuperBarber.Services.Barbers;
using static SuperBarber.Tests.CreateTestDb;
using SuperBarber.Infrastructure;
using SuperBarber.Data.Models;

namespace SuperBarber.Tests
{
    [TestFixture]
    public class BarberServiceTests : BaseTest
    {
        private IBarberService service;

        [OneTimeSetUp]
        public void TestInitialize()
        {
            service = new BarberService(dbContextWithSeededData, userManager, signInManager);
        }

        [Test]
        public async Task Test_ShouldAddUserAsBarberInTheDbAndGiveHimBarberRole()
        {
            var testUser = testDb.Users.First(u => u.Id == GuestUserId.ToString());

            await service.AddBarberAsync(testUser.Id);
            
            Assert.True(dbContextWithSeededData.Barbers.Any(b => b.UserId == GuestUserId.ToString()));
        }
        
        [Test]
        public async Task Test_ThrowModelStateCustomExceptionWhenThereIsExitingBarber()
        {
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberAsync(BarberShopOwnerUserId.ToString()), "User is already a barber");
        }
        
        [Test]
        public async Task Test_RestoreDeletedBarberOfUser()
        {
            var userId = GuestUserId.ToString();

            var barber = new Barber() 
            { 
                Id = 3, 
                UserId = userId, 
                FirstName = "", LastName = "", 
                Email = "", PhoneNumber = "", 
                IsDeleted = true, DeleteDate = DateTime.UtcNow 
            };

            dbContextWithSeededData.Barbers.Add(barber);

            dbContextWithSeededData.SaveChanges();

            await service.AddBarberAsync(userId);

            Assert.True(dbContextWithSeededData.Barbers.Any(b => b.UserId == userId && !b.IsDeleted));

            dbContextWithSeededData.Barbers.Remove(barber);

            dbContextWithSeededData.SaveChanges();
        }

        [Test]
        public async Task Test_ShouldAssignBarberToBarberShop()
        {
            var userBabrberId = BarberUserId.ToString();

            var barberShopId = dbContextWithSeededData.BarberShops.First().Id;

            await service.AsignBarberToBarberShopAsync(barberShopId, userBabrberId);

            var barberId = dbContextWithSeededData.Barbers.First(b => b.UserId == userBabrberId).Id;

            Assert.True(dbContextWithSeededData.BarberShops.First().Barbers.Any(b => b.BarberId == barberId));
        }
    }
}
