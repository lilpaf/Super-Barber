using Microsoft.AspNetCore.Identity;
using SuperBarber.Data.Models;
using SuperBarber.Data;

namespace SuperBarber.Tests
{
    public abstract class BaseTest
    {
        protected SuperBarberDbContext dbContextWithSeededData;
        protected CreateTestDb testDb;
        protected UserManager<User> userManager;
        protected SignInManager<User> signInManager;

        [OneTimeSetUp]
        public void TestDbInitialize()
        {
            this.testDb = new CreateTestDb();
            this.dbContextWithSeededData = testDb.SeedDataInDb();
            this.userManager = testDb.UserManager;
            this.signInManager = testDb.SignInManager;
        }

        [OneTimeTearDown]
        public void TestDbTearDown()
        {
            this.testDb.DisposeTestDb();
        }
    }
}
