using SuperBarber.Core.Services.Barbers;
using SuperBarber.Core.Extensions;
using SuperBarber.Infrastructure.Data.Models;
using SuperBarber.UnitTests.Common;
using static SuperBarber.UnitTests.Common.CreateTestDb;
using static SuperBarber.Core.Extensions.ExeptionErrors;
using static SuperBarber.Core.Extensions.ExeptionErrors.BarberServiceErrors;

namespace SuperBarber.UnitTests.Services
{
    [TestFixture]
    public class BarberServiceTests : BaseTest
    {
        private IBarberService service;
        private const int FakeId = 100;

        [SetUp]
        public void TestInitialize()
        {
            service = new BarberService(dbContextWithSeededData, userManager, signInManager);
        }

        [Test]
        public async Task TestAddBarber_ShouldAddUserAsBarberInTheDbAndGiveHimBarberRole()
        {
            var testUser = testDb.Users.First(u => u.Id == GuestUserId.ToString());

            await service.AddBarberAsync(testUser.Id);

            Assert.True(dbContextWithSeededData.Barbers.Any(b => b.UserId == GuestUserId.ToString()));
        }

        [Test]
        public void TestAddBarber_ThrowModelStateCustomExceptionWhenThereIsExitingBarber()
        {
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddBarberAsync(BarberShopOwnerUserId.ToString()), UserIsBarber);
        }

        [Test]
        public async Task TestAddBarber_RestoreDeletedBarberOfUser()
        {
            var userId = GuestUserId.ToString();

            var barber = new Barber()
            {
                Id = 3,
                UserId = userId,
                FirstName = "",
                LastName = "",
                Email = "",
                PhoneNumber = "",
                IsDeleted = true,
                DeleteDate = DateTime.UtcNow
            };

            dbContextWithSeededData.Barbers.Add(barber);

            dbContextWithSeededData.SaveChanges();

            await service.AddBarberAsync(userId);

            Assert.True(dbContextWithSeededData.Barbers.Any(b => b.UserId == userId && !b.IsDeleted));
        }

        [Test]
        public async Task TestAsignBarberToBarberShop_ShouldAssignBarberToBarberShop()
        {
            var userBabrberId = BarberUserId.ToString();

            var barberShopId = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted).Id;

            await service.AsignBarberToBarberShopAsync(barberShopId, userBabrberId);

            var barberId = dbContextWithSeededData.Barbers.First(b => b.UserId == userBabrberId).Id;

            Assert.True(dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted).Barbers.Any(b => b.BarberId == barberId));
        }

        [Test]
        public void TestAsignBarberToBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberShopIsInvalid()
        {
            var userBabrberId = BarberUserId.ToString();

            //Ivalid Id
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AsignBarberToBarberShopAsync(FakeId, userBabrberId), BarberShopNonExistent);

            //Deleted BarberShop
            var deletedBarberShop = dbContextWithSeededData.BarberShops.First(b => b.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AsignBarberToBarberShopAsync(deletedBarberShop.Id, userBabrberId), BarberShopNonExistent);

            //Private BarberShop
            var barberShop = dbContextWithSeededData.BarberShops.First();

            barberShop.IsPublic = false;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AsignBarberToBarberShopAsync(barberShop.Id, userBabrberId), BarberShopNonExistent);
        }

        [Test]
        public async Task TestAsignBarberToBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberIsAllreadyWorkingInTheBarberShop()
        {
            var userBabrberId = BarberUserId.ToString();

            var barberShopId = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted).Id;

            // Add user to barbershop
            await service.AsignBarberToBarberShopAsync(barberShopId, userBabrberId);

            //Try to add it again
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AsignBarberToBarberShopAsync(barberShopId, userBabrberId), UserIsAsignedToBarberShop);
        }

        [Test]
        public async Task TestAsignBarberToBarberShop_IfUserInNotBarberHeIsNotAddedToBarberShop()
        {
            var userGuestId = GuestUserId.ToString();

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Try to add user to barbershop
            await service.AsignBarberToBarberShopAsync(barberShop.Id, userGuestId);

            //User should not be added
            barberShop = dbContextWithSeededData.BarberShops.First(b => b.Id == barberShop.Id);

            Assert.True(barberShop.Barbers.Any(b => b.Barber.UserId == userGuestId) == false);
        }

        [Test]
        public async Task TestUnasignBarberFromBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberShopIsDeletedOrNonExistent()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Add barberToBarberShop
            await service.AsignBarberToBarberShopAsync(barberShop.Id, barber.User.Id);

            // Try to remove barber from non existent barbershop
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.UnasignBarberFromBarberShopAsync(FakeId, barber.Id, barberShopOwnerUserId), BarberShopNonExistent);

            // Try to remove barber from deleted barbershop
            var deletedBarberShop = dbContextWithSeededData.BarberShops.First(b => b.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.UnasignBarberFromBarberShopAsync(deletedBarberShop.Id, barber.Id, barberShopOwnerUserId), BarberShopNonExistent);
        }

        [Test]
        public async Task TestUnasignBarberFromBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberIsNotOwnerOrNotWorkingAtTheBarberShopAtAll()
        {
            var barberNotOwnerUserId = BarberUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == barberNotOwnerUserId);

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            //Try to remove barber withought beeing a employee at the barbershop
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.UnasignBarberFromBarberShopAsync(barberShop.Id, barber.Id, barberNotOwnerUserId), string.Format(UserIsNotOwnerOfBarberShop, barberShop.Name));

            // Add barberToBarberShop
            await service.AsignBarberToBarberShopAsync(barberShop.Id, barberNotOwnerUserId);

            // Try to remove barber as non owner user
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.UnasignBarberFromBarberShopAsync(barberShop.Id, barber.Id, barberNotOwnerUserId), string.Format(UserIsNotOwnerOfBarberShop, barberShop.Name));
        }

        [Test]
        public async Task TestUnasignBarberFromBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberIsDeletedDoseNotExistsOrIsNotWorkingAtThisBarberShop()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Barber is non existent
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.UnasignBarberFromBarberShopAsync(barberShop.Id, FakeId, barberShopOwnerUserId), BarberNonExistent);

            // Try to remove barber that is not employee
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.UnasignBarberFromBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId), BarberNonExistent);

            // This is edge case test usually when a barber is deleted he is removed from all of his shops.
            // In this test we will only test to check if the barber is deleted

            // Add barber to barbershop
            await service.AsignBarberToBarberShopAsync(barberShop.Id, BarberUserId.ToString());

            barber.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.UnasignBarberFromBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId), BarberNonExistent);
        }

        [Test]
        public void TestUnasignBarberFromBarberShop_ShouldThrowModelStateCustomExceptionWhenTheOnlyOwnerTriesToResign()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == barberShopOwnerUserId);

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Barber that is resigning is the only owner
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.UnasignBarberFromBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId), string.Format(UserIsTheOnlyOwnerOfBarberShop, barberShop.Name));
        }

        [Test]
        public async Task TestUnasignBarberFromBarberShop_ShouldFireBarberFromBarberShopAndMarkAsDeletedAllOfHisOrders()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            //Barber is added to barbershop
            await service.AsignBarberToBarberShopAsync(barberShop.Id, BarberUserId.ToString());

            barberShop = dbContextWithSeededData.BarberShops.First(b => b.Id == barberShop.Id);

            Assert.True(barberShop.Barbers.Any(b => b.Barber.UserId == BarberUserId.ToString()));

            //Add an order to be cancelled after the fireing of the barber
            var orderId = Guid.Parse("c5621332-4d81-462d-bd53-e08e557d0000");

            var order = new Order() { Id = orderId, BarberId = barber.Id, Date = DateTime.UtcNow, ServiceId = 1, UserId = GuestUserId.ToString(), IsDeleted = false, DeleteDate = null };

            dbContextWithSeededData.Orders.Add(order);

            barberShop.Orders.Add(order);

            dbContextWithSeededData.SaveChanges();

            //Owner fires the barber
            await service.UnasignBarberFromBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId);

            barberShop = dbContextWithSeededData.BarberShops.First(b => b.Id == barberShop.Id);

            barber = dbContextWithSeededData.Barbers.First(b => b.Id == barber.Id);

            Assert.True(!barberShop.Barbers.Any(b => b.Barber.UserId == BarberUserId.ToString()));
            Assert.True(barber.Orders.Any(o => o.Id == orderId && o.IsDeleted && o.DeleteDate != null));
        }

        [Test]
        public async Task Test_CheckIfTheBarberToResignIsTheCurrentUserShoulReturnTrue()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barberId = dbContextWithSeededData.Barbers.First(b => b.UserId == barberShopOwnerUserId).Id;

            Assert.True(await service.CheckIfUserIsTheBabrerToFire(barberShopOwnerUserId, barberId));
        }

        [Test]
        public async Task Test_ShouldReturnFalseWhenBarberIsNonExistentOrDeleted()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == barberShopOwnerUserId);

            //Check if the barber exists
            Assert.True(await service.CheckIfUserIsTheBabrerToFire(barberShopOwnerUserId, FakeId) == false);

            //Check if the barber is deleted
            barber.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.True(await service.CheckIfUserIsTheBabrerToFire(barberShopOwnerUserId, barber.Id) == false);
        }

        [Test]
        public void TestAddOwnerToBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberShopIsDeletedOrNonExistent()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Try to accsess non existent barbershop
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOwnerToBarberShopAsync(FakeId, barber.Id, barberShopOwnerUserId), BarberShopNonExistent);

            // Try to accsess deleted barbershop
            var deletedBarberShop = dbContextWithSeededData.BarberShops.First(b => b.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOwnerToBarberShopAsync(deletedBarberShop.Id, barber.Id, barberShopOwnerUserId), BarberShopNonExistent);
        }

        [Test]
        public void TestAddOwnerToBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberIsNotOwnerOrNotWorkingAtTheBarberShopAtAll()
        {
            var barberNotOwnerUserId = BarberUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == barberNotOwnerUserId);

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            //User not owner of the shop tries to make owner employee
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOwnerToBarberShopAsync(barberShop.Id, barber.Id, barberNotOwnerUserId), string.Format(UserIsNotOwnerOfBarberShop, barberShop.Name));

            //Babrer is not owner and it is not working at the barbershop
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOwnerToBarberShopAsync(barberShop.Id, barber.Id, FakeId.ToString()), string.Format(UserIsNotOwnerOfBarberShop, barberShop.Name));
        }

        [Test]
        public async Task TestAddOwnerToBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberIsDeletedDoseNotExistsOrIsNotWorkingAtThisBarberShop()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Barber is non existent
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOwnerToBarberShopAsync(barberShop.Id, FakeId, barberShopOwnerUserId), BarberNonExistent);

            // Try to remove barber that is not employee
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOwnerToBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId), BarberNonExistent);

            // This is edge case test usually when a barber is deleted he is removed from all of his shops.
            // In this test we will only test to check if the barber is deleted

            // Add barber to barbershop
            await service.AsignBarberToBarberShopAsync(barberShop.Id, BarberUserId.ToString());

            barber.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOwnerToBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId), BarberNonExistent);
        }

        [Test]
        public async Task TestAddOwnerToBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberIsAllreadyAnOwner()
        {
            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            await service.AsignBarberToBarberShopAsync(barberShop.Id, BarberUserId.ToString());

            // Barber is not owner
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.AddOwnerToBarberShopAsync(barberShop.Id, barber.Id, barber.User.Id), string.Format(UserIsAlreadyOwnerOfBarberShop, barberShop.Name));
        }

        [Test]
        public async Task TestAddOwnerToBarberShop_ShouldAddEmployeeAsOwner()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            await service.AsignBarberToBarberShopAsync(barberShop.Id, BarberUserId.ToString());

            await service.AddOwnerToBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId);

            barberShop = dbContextWithSeededData.BarberShops.First(b => b.Id == barberShop.Id);

            Assert.True(barberShop.Barbers.Any(b => b.BarberId == barber.Id && b.IsOwner));
        }

        [Test]
        public void TestRemoveOwnerFromBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberShopIsDeletedOrNonExistent()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Try to accsess non existent barbershop
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveOwnerFromBarberShopAsync(FakeId, barber.Id, barberShopOwnerUserId), BarberShopNonExistent);

            // Try to accsess deleted barbershop
            var deletedBarberShop = dbContextWithSeededData.BarberShops.First(b => b.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveOwnerFromBarberShopAsync(deletedBarberShop.Id, barber.Id, barberShopOwnerUserId), BarberShopNonExistent);
        }

        [Test]
        public void TestRemoveOwnerFromBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberIsNonExistentOrDeleted()
        {
            var barberNotOwnerUserId = BarberUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == barberNotOwnerUserId);

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            //User not owner of the shop tries to make owner employee
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveOwnerFromBarberShopAsync(barberShop.Id, barber.Id, barberNotOwnerUserId), string.Format(UserIsNotOwnerOfBarberShop, barberShop.Name));

            //Babrer is not owner and it is not working at the barbershop
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveOwnerFromBarberShopAsync(barberShop.Id, barber.Id, FakeId.ToString()), string.Format(UserIsNotOwnerOfBarberShop, barberShop.Name));
        }

        [Test]
        public void TestRemoveOwnerFromBarberShop_ShouldThrowModelStateCustomExceptionWhenTryingToDelteTheOnlyOwner()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == barberShopOwnerUserId);

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            //Deleteing the only owner
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveOwnerFromBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId), BarberShopHasToHaveAtLeastOneOwner);
        }

        [Test]
        public async Task TestRemoveOwnerFromBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberIsDeletedDoseNotExistsOrIsNotWorkingAtThisBarberShop()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Barber is non existent
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveOwnerFromBarberShopAsync(barberShop.Id, FakeId, barberShopOwnerUserId), BarberNonExistent);

            // Try to remove barber that is not employee
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveOwnerFromBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId), BarberNonExistent);

            // This is edge case test usually when a barber is deleted he is removed from all of his shops.
            // In this test we will only test to check if the barber is deleted

            // Add barber to barbershop
            await service.AsignBarberToBarberShopAsync(barberShop.Id, BarberUserId.ToString());

            barber.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveOwnerFromBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId), BarberNonExistent);
        }

        [Test]
        public async Task TestRemoveOwnerFromBarberShop_ShouldThrowModelStateCustomExceptionWhenOwnerIsTryingToDeleteHimself()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barberShopOwnerId = dbContextWithSeededData.Barbers.First(b => b.UserId == barberShopOwnerUserId).Id;

            var barberToAdd = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Add barber as owner to barbershop
            await service.AsignBarberToBarberShopAsync(barberShop.Id, BarberUserId.ToString());

            var barberInShop = barberShop.Barbers.First(b => b.BarberId == barberToAdd.Id);

            barberInShop.IsOwner = true;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveOwnerFromBarberShopAsync(barberShop.Id, barberShopOwnerId, barberShopOwnerUserId), string.Format(UserIsOwnerOfTheBarberShop, barberShop.Name));
        }

        [Test]
        public async Task TestRemoveOwnerFromBarberShop_ShouldThrowModelStateCustomExceptionWhenTheBarberToRevokeOwnershipIsNotOwner()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Add barber to barbershop
            await service.AsignBarberToBarberShopAsync(barberShop.Id, BarberUserId.ToString());

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.RemoveOwnerFromBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId), string.Format(BarberIsNotOwnerOfBarberShop, barberShop.Name));
        }

        [Test]
        public async Task TestRemoveOwnerFromBarberShop_ShouldRevokeOwnershipOfBarber()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Add barber as owner to barbershop
            await service.AsignBarberToBarberShopAsync(barberShop.Id, BarberUserId.ToString());

            var barberInShop = barberShop.Barbers.First(b => b.BarberId == barber.Id);

            barberInShop.IsOwner = true;

            dbContextWithSeededData.SaveChanges();

            barberShop = dbContextWithSeededData.BarberShops.First(b => b.Id == barberShop.Id);

            Assert.True(barberShop.Barbers.Any(b => b.BarberId == barber.Id && b.IsOwner));

            await service.RemoveOwnerFromBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId);

            barberShop = dbContextWithSeededData.BarberShops.First(b => b.Id == barberShop.Id);

            Assert.True(barberShop.Barbers.Any(b => b.BarberId == barber.Id && !b.IsOwner));
        }

        [Test]
        public void TestMakeBarberUnavailableAtBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberShopIsDeletedOrNonExistent()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Try to accsess non existent barbershop
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberUnavailableAtBarberShopAsync(FakeId, barber.Id, barberShopOwnerUserId), BarberShopNonExistent);

            // Try to accsess deleted barbershop
            var deletedBarberShop = dbContextWithSeededData.BarberShops.First(b => b.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberUnavailableAtBarberShopAsync(deletedBarberShop.Id, barber.Id, barberShopOwnerUserId), BarberShopNonExistent);
        }

        [Test]
        public async Task TestMakeBarberUnavailableAtBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberIsDeletedDoseNotExistsOrIsNotWorkingAtThisBarberShop()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Barber is non existent
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberUnavailableAtBarberShopAsync(barberShop.Id, FakeId, barberShopOwnerUserId), BarberNonExistent);

            // Try to remove barber that is not employee
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberUnavailableAtBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId), BarberNonExistent);

            // This is edge case test usually when a barber is deleted he is removed from all of his shops.
            // In this test we will only test to check if the barber is deleted

            // Add barber to barbershop
            await service.AsignBarberToBarberShopAsync(barberShop.Id, BarberUserId.ToString());

            barber.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberUnavailableAtBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId), BarberNonExistent);
        }

        [Test]
        public void TestMakeBarberUnavailableAtBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberIsNonExistentOrDeleted()
        {
            var barberNotOwnerUserId = BarberUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == barberNotOwnerUserId);

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            //User not owner of the shop tries to make employee unavailable
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberUnavailableAtBarberShopAsync(barberShop.Id, barber.Id, barberNotOwnerUserId), string.Format(UserIsNotOwnerOfBarberShop, barberShop.Name));

            //Babrer is not owner and it is not working at the barbershop
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberUnavailableAtBarberShopAsync(barberShop.Id, barber.Id, FakeId.ToString()), string.Format(UserIsNotOwnerOfBarberShop, barberShop.Name));
        }

        [Test]
        public async Task TestMakeBarberUnavailableAtBarberShop_ShouldMakeBarberUnavailable()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Add barber barbershop
            await service.AsignBarberToBarberShopAsync(barberShop.Id, BarberUserId.ToString());

            barberShop = dbContextWithSeededData.BarberShops.First(b => b.Id == barberShop.Id);

            Assert.True(barberShop.Barbers.Any(b => b.BarberId == barber.Id && !b.IsOwner && !b.IsAvailable));

            //Make barber Avalible
            var barberInShop = barberShop.Barbers.First(b => b.BarberId == barber.Id);

            barberInShop.IsAvailable = true;

            dbContextWithSeededData.SaveChanges();

            barberShop = dbContextWithSeededData.BarberShops.First(b => b.Id == barberShop.Id);

            Assert.True(barberShop.Barbers.Any(b => b.BarberId == barber.Id && b.IsAvailable));

            await service.MakeBarberUnavailableAtBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId);

            barberShop = dbContextWithSeededData.BarberShops.First(b => b.Id == barberShop.Id);

            Assert.True(barberShop.Barbers.Any(b => b.BarberId == barber.Id && !b.IsOwner && !b.IsAvailable));

            //Owner make himself unavailable

            var ownerOfShop = barberShop.Barbers.First(b => b.Barber.UserId == barberShopOwnerUserId);

            ownerOfShop.IsAvailable = true;

            dbContextWithSeededData.SaveChanges();

            barberShop = dbContextWithSeededData.BarberShops.First(b => b.Id == barberShop.Id);

            Assert.True(barberShop.Barbers.Any(b => b.BarberId == ownerOfShop.BarberId && b.IsOwner && b.IsAvailable));

            await service.MakeBarberUnavailableAtBarberShopAsync(barberShop.Id, ownerOfShop.BarberId, barberShopOwnerUserId);

            barberShop = dbContextWithSeededData.BarberShops.First(b => b.Id == barberShop.Id);

            Assert.True(barberShop.Barbers.Any(b => b.BarberId == ownerOfShop.BarberId && b.IsOwner && !b.IsAvailable));
        }

        [Test]
        public void TestMakeBarberAvailableAtBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberShopIsDeletedOrNonExistent()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Try to accsess non existent barbershop
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberAvailableAtBarberShopAsync(FakeId, barber.Id, barberShopOwnerUserId), BarberShopNonExistent);

            // Try to accsess deleted barbershop
            var deletedBarberShop = dbContextWithSeededData.BarberShops.First(b => b.IsDeleted);

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberAvailableAtBarberShopAsync(deletedBarberShop.Id, barber.Id, barberShopOwnerUserId), BarberShopNonExistent);
        }

        [Test]
        public async Task TestMakeBarberAvailableAtBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberIsDeletedDoseNotExistsOrIsNotWorkingAtThisBarberShop()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Barber is non existent
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberAvailableAtBarberShopAsync(barberShop.Id, FakeId, barberShopOwnerUserId), BarberNonExistent);

            // Try to remove barber that is not employee
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberAvailableAtBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId), BarberNonExistent);

            // This is edge case test usually when a barber is deleted he is removed from all of his shops.
            // In this test we will only test to check if the barber is deleted

            // Add barber to barbershop
            await service.AsignBarberToBarberShopAsync(barberShop.Id, BarberUserId.ToString());

            barber.IsDeleted = true;

            dbContextWithSeededData.SaveChanges();

            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberAvailableAtBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId), BarberNonExistent);
        }

        [Test]
        public void TestMakeBarberAvailableAtBarberShop_ShouldThrowModelStateCustomExceptionWhenBarberIsNonExistentOrDeleted()
        {
            var barberNotOwnerUserId = BarberUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == barberNotOwnerUserId);

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            //User not owner of the shop tries to make employee unavailable
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberAvailableAtBarberShopAsync(barberShop.Id, barber.Id, barberNotOwnerUserId), string.Format(UserIsNotOwnerOfBarberShop, barberShop.Name));

            //Babrer is not owner and it is not working at the barbershop
            Assert.ThrowsAsync<ModelStateCustomException>(async () => await service.MakeBarberAvailableAtBarberShopAsync(barberShop.Id, barber.Id, FakeId.ToString()), string.Format(UserIsNotOwnerOfBarberShop, barberShop.Name));
        }

        [Test]
        public async Task TestMakeBarberAvailableAtBarberShopAsync_ShouldMakeBarberAvalible()
        {
            var barberShopOwnerUserId = BarberShopOwnerUserId.ToString();

            var barber = dbContextWithSeededData.Barbers.First(b => b.UserId == BarberUserId.ToString());

            var barberShop = dbContextWithSeededData.BarberShops.First(b => !b.IsDeleted);

            // Add barber barbershop
            await service.AsignBarberToBarberShopAsync(barberShop.Id, BarberUserId.ToString());

            barberShop = dbContextWithSeededData.BarberShops.First(b => b.Id == barberShop.Id);

            Assert.True(barberShop.Barbers.Any(b => b.BarberId == barber.Id && !b.IsOwner && !b.IsAvailable));

            await service.MakeBarberAvailableAtBarberShopAsync(barberShop.Id, barber.Id, barberShopOwnerUserId);

            barberShop = dbContextWithSeededData.BarberShops.First(b => b.Id == barberShop.Id);

            Assert.True(barberShop.Barbers.Any(b => b.BarberId == barber.Id && !b.IsOwner && b.IsAvailable));

            //Owner make himself available

            var ownerOfShop = barberShop.Barbers.First(b => b.Barber.UserId == barberShopOwnerUserId);

            //Make owner unavalible 
            ownerOfShop.IsAvailable = false;

            dbContextWithSeededData.SaveChanges();

            barberShop = dbContextWithSeededData.BarberShops.First(b => b.Id == barberShop.Id);

            Assert.True(barberShop.Barbers.Any(b => b.BarberId == ownerOfShop.BarberId && b.IsOwner && !b.IsAvailable));

            await service.MakeBarberAvailableAtBarberShopAsync(barberShop.Id, ownerOfShop.BarberId, barberShopOwnerUserId);

            barberShop = dbContextWithSeededData.BarberShops.First(b => b.Id == barberShop.Id);

            Assert.True(barberShop.Barbers.Any(b => b.BarberId == ownerOfShop.BarberId && b.IsOwner && b.IsAvailable));
        }

        [Test]
        public async Task TestGetBarberOrders_ShouldReturnBarberOrders()
        {
            var model = await service.GetBarberOrdersAsync(BarberShopOwnerUserId.ToString(), 0);

            Assert.True(model.TotalOrders == model.Orders.Count && model.Orders.Count == 1);
        }
    }
}
