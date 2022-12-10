using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using SuperBarber.Infrastructure.Data;
using SuperBarber.Infrastructure.Data.Models;

namespace SuperBarber.Tests.Common
{
    public abstract class BaseTest
    {
        protected SuperBarberDbContext dbContextWithSeededData;
        protected CreateTestDb testDb;
        protected UserManager<User> userManager;
        protected SignInManager<User> signInManager;
        protected IWebHostEnvironment webHostEnvironment;

        [SetUp]
        public void TestDbInitialize()
        {
            testDb = new CreateTestDb();
            dbContextWithSeededData = testDb.SeedDataInDb();
            userManager = testDb.UserManager;
            signInManager = testDb.SignInManager;
            webHostEnvironment = testDb.WebHostEnvironment;
        }

        [TearDown]
        public void TestDbTearDown()
        {
            testDb.DisposeTestDb();
        }
    }
}
