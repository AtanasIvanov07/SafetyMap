using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SafetyMap.Core.Services;
using SafetyMapData.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SafetyMap.Core.Tests.Services
{
    [TestFixture]
    public class AccountServiceTests
    {
        private Mock<UserManager<UserIdentity>> _userManagerMock;
        private Mock<SignInManager<UserIdentity>> _signInManagerMock;
        private AccountService _accountService;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = MockUserManager();
            _signInManagerMock = MockSignInManager(_userManagerMock.Object);
            _accountService = new AccountService(_userManagerMock.Object, _signInManagerMock.Object);
        }

        private static Mock<UserManager<UserIdentity>> MockUserManager()
        {
            var store = new Mock<IUserStore<UserIdentity>>();
            return new Mock<UserManager<UserIdentity>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private static Mock<SignInManager<UserIdentity>> MockSignInManager(UserManager<UserIdentity> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<UserIdentity>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<UserIdentity>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<UserIdentity>>();

            return new Mock<SignInManager<UserIdentity>>(
                userManager,
                contextAccessor.Object,
                claimsFactory.Object,
                options.Object,
                logger.Object,
                schemes.Object,
                confirmation.Object);
        }

        [Test]
        public async Task RegisterAsync_ShouldReturnSuccess_WhenCreationSucceeds()
        {
            // Arrange
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserIdentity>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<UserIdentity>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _signInManagerMock.Setup(x => x.SignInAsync(It.IsAny<UserIdentity>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _accountService.RegisterAsync("testuser", "test@test.com", "First", "Last", "Password123!");

            // Assert
            Assert.That(result.Succeeded, Is.True);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<UserIdentity>(), "User"), Times.Once);
            _signInManagerMock.Verify(x => x.SignInAsync(It.IsAny<UserIdentity>(), false, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task RegisterAsync_ShouldReturnFailure_WhenCreationFailed()
        {
            // Arrange
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserIdentity>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

            // Act
            var result = await _accountService.RegisterAsync("testuser", "test@test.com", "First", "Last", "Password123!");

            // Assert
            Assert.That(result.Succeeded, Is.False);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<UserIdentity>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task LoginAsync_ShouldReturnTrue_WhenLoginSucceeds()
        {
            // Arrange
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _accountService.LoginAsync("testuser", "password");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task LoginAsync_ShouldReturnFalse_WhenLoginFails()
        {
            // Arrange
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _accountService.LoginAsync("testuser", "password");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task LogoutAsync_ShouldCallSignOutAsync()
        {
            // Arrange
            _signInManagerMock.Setup(x => x.SignOutAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _accountService.LogoutAsync();

            // Assert
            _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Once);
        }

        [Test]
        public async Task ExternalLoginCallbackAsync_ShouldReturnFailed_WhenInfoIsNull()
        {
            // Arrange
            _signInManagerMock.Setup(x => x.GetExternalLoginInfoAsync(It.IsAny<string>()))
                .ReturnsAsync((ExternalLoginInfo)null!);

            // Act
            var result = await _accountService.ExternalLoginCallbackAsync();

            // Assert
            Assert.That(result, Is.EqualTo(Microsoft.AspNetCore.Identity.SignInResult.Failed));
        }
    }
}
