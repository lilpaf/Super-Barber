using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using SuperBarber.Infrastructure.Data.Models;
using System.Security.Claims;

namespace SuperBarber.Tests.Mocks
{
    public static class UserManegerMock
    {
        public static Mock<UserManager<User>> MockUserManager(IEnumerable<User> userList)
        {
            Mock<UserManager<User>> userManager = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(),
                null, null, null, null, null, null, null, null);

            userManager.Object.UserValidators.Add(new UserValidator<User>());
            userManager.Object.PasswordValidators.Add(new PasswordValidator<User>());

            userManager.Setup(um => um.GetUserAsync(
                    It.IsAny<ClaimsPrincipal>()))!
                .ReturnsAsync((ClaimsPrincipal principal) =>
                    userList.FirstOrDefault(u => u.Id == 
                        Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)).ToString()));

            userManager.Setup(um => um.UpdateAsync(
                    It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            userManager.Setup(um => um.FindByNameAsync(
                    It.IsAny<string>()))!
                .ReturnsAsync((string username) =>
                    userList.FirstOrDefault(u => u.UserName == username));

            userManager.Setup(um => um.SetUserNameAsync(
                    It.IsAny<User>(), It.IsAny<string>()))!
                .ReturnsAsync((User user, string newUsername) =>
                {
                    user.UserName = newUsername;
                    return IdentityResult.Success;
                });

            userManager.Setup(um => um.CheckPasswordAsync(
                    It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync((User user, string givenPassword) =>
                {
                    PasswordHasher<User> hasher = new PasswordHasher<User>();

                    PasswordVerificationResult result =
                        hasher.VerifyHashedPassword(user, user.PasswordHash, givenPassword);

                    return result == PasswordVerificationResult.Success;
                });

            userManager.Setup(um => um.DeleteAsync(
                    It.IsAny<User>()))
                .ReturnsAsync((User user) =>
                {
                    user.UserName = $"DELETED{user.UserName}";

                    return IdentityResult.Success;
                });

            userManager.Setup(um => um.IsEmailConfirmedAsync(
                    It.IsAny<User>()))
                .ReturnsAsync((User user) => user.EmailConfirmed);

            userManager.Setup(um => um.GetEmailAsync(
                    It.IsAny<User>()))
                .ReturnsAsync((User user) => user.Email);

            userManager.Setup(um => um.FindByEmailAsync(
                    It.IsAny<string>()))!
                .ReturnsAsync((string email) => userList.FirstOrDefault(u => u.Email == email));

            userManager.Setup(um => um.GetUserIdAsync(
                    It.IsAny<User>()))
                .ReturnsAsync((User user) => user.Id.ToString());

            userManager.Setup(um => um.GenerateChangeEmailTokenAsync(
                    It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync("random-string");

            userManager.Setup(um => um.GenerateEmailConfirmationTokenAsync(
                    It.IsAny<User>()))
                .ReturnsAsync("random-string");

            userManager.Setup(um => um.FindByIdAsync(
                    It.IsAny<string>()))!
                .ReturnsAsync((string id) =>
                    userList.FirstOrDefault(u => u.Id == Guid.Parse(id).ToString()));

            userManager.Setup(um => um.ChangeEmailAsync(
                    It.IsAny<User>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))!
                .ReturnsAsync((User user, string email, string token) =>
                {
                    user.Email = email;
                    return IdentityResult.Success;
                });

            userManager.Setup(um => um.ConfirmEmailAsync(
                    It.IsAny<User>(),
                    It.IsAny<string>()))!
                .ReturnsAsync((User user, string token) =>
                {
                    user.EmailConfirmed = true;
                    return IdentityResult.Success;
                });

            userManager.Setup(um => um.ChangePasswordAsync(
                    It.IsAny<User>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))!
                .ReturnsAsync((User user, string oldPass, string newPass) =>
                {
                    PasswordHasher<User> hasher = new PasswordHasher<User>();
                    user.PasswordHash = hasher.HashPassword(user, newPass);
                    return IdentityResult.Success;
                });

            return userManager;
        }
    }
}
