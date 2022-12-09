using Microsoft.AspNetCore.Identity;
using SuperBarber.Infrastructure.Data;
using SuperBarber.Infrastructure.Data.Models;
using SuperBarber.Tests.Services;

namespace SuperBarber.Tests.Common
{
    public abstract class BaseTest
    {
        protected SuperBarberDbContext dbContextWithSeededData;
        protected CreateTestDb testDb;
        protected UserManager<User> userManager;
        protected SignInManager<User> signInManager;

        [SetUp]
        public void TestDbInitialize()
        {
            testDb = new CreateTestDb();
            dbContextWithSeededData = testDb.SeedDataInDb();
            userManager = testDb.UserManager;
            signInManager = testDb.SignInManager;
        }

        [TearDown]
        public void TestDbTearDown()
        {
            testDb.DisposeTestDb();
        }
    }
}
