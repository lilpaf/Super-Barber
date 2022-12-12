using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using SuperBarber.Infrastructure.Data.Models;

namespace SuperBarber.UnitTests.Mocks
{
    public static class SignInManegerMock
    {
        public static Mock<SignInManager<User>> MockSignInManager()
        {
            Mock<UserManager<User>> userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(),
                null, null, null, null, null, null, null, null);

            Mock<SignInManager<User>> signInManager = new Mock<SignInManager<User>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<User>>(),
                null, null, null, null);

            signInManager.Setup(sim => sim.RefreshSignInAsync(
                    It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            signInManager.Setup(sim => sim.SignOutAsync())
                .Returns(Task.CompletedTask);

            return signInManager;
        }
    }
}
