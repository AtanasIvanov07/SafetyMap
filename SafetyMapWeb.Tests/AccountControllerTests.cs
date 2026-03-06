using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NUnit.Framework;
using SafetyMap.Core.Contracts;
using SafetyMapWeb.Controllers;
using SafetyMapWeb.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SafetyMapWeb.Tests.Controllers
{
    [TestFixture]
    public class AccountControllerTests
    {
        private Mock<IAccountService> _accountServiceMock;
        private AccountController _controller;

        [SetUp]
        public void SetUp()
        {
            _accountServiceMock = new Mock<IAccountService>();
            _controller = new AccountController(_accountServiceMock.Object);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Content("~/")).Returns("/");
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("callback-url");
            _controller.Url = urlHelperMock.Object;
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        private void AuthenticateUser()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public void Register_Get_ShouldReturnView_WhenNotAuthenticated()
        {
            var result = _controller.Register() as ViewResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Register_Get_ShouldRedirectToHome_WhenAuthenticated()
        {
            AuthenticateUser();

            var result = _controller.Register() as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public async Task Register_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Error", "Invalid model");
            var model = new RegisterViewModel();

            var result = await _controller.Register(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.EqualTo(model));
        }

        [Test]
        public async Task Register_Post_ShouldRedirectToHome_WhenRegistrationSucceeds()
        {
            var model = new RegisterViewModel { UserName = "testuser", Email = "test@test.com", FirstName = "Test", LastName = "User", Password = "Password123!" };
            _accountServiceMock.Setup(s => s.RegisterAsync(model.UserName, model.Email, model.FirstName, model.LastName, model.Password))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Register(model) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public async Task Register_Post_ShouldReturnViewWithErrors_WhenRegistrationFails()
        {
            var model = new RegisterViewModel { UserName = "testuser" };
            _accountServiceMock.Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error message" }));

            var result = await _controller.Register(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(_controller.ModelState.IsValid, Is.False);
            Assert.That(_controller.ModelState[""]!.Errors[0].ErrorMessage, Is.EqualTo("Error message"));
        }

        [Test]
        public void Login_Get_ShouldReturnView_WhenNotAuthenticated()
        {
            var result = _controller.Login() as ViewResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Login_Get_ShouldRedirectToHome_WhenAuthenticated()
        {
            AuthenticateUser();

            var result = _controller.Login() as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public async Task Login_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Error", "Invalid model");
            var model = new LoginViewModel();

            var result = await _controller.Login(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Login_Post_ShouldRedirectToHome_WhenLoginSucceeds()
        {
            var model = new LoginViewModel { UserName = "testuser", Password = "password" };
            _accountServiceMock.Setup(s => s.LoginAsync(model.UserName, model.Password))
                .ReturnsAsync(true);

            var result = await _controller.Login(model) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public async Task Login_Post_ShouldReturnViewWithError_WhenLoginFails()
        {
            var model = new LoginViewModel { UserName = "testuser", Password = "password" };
            _accountServiceMock.Setup(s => s.LoginAsync(model.UserName, model.Password))
                .ReturnsAsync(false);

            var result = await _controller.Login(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(_controller.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Logout_ShouldCallLogoutAndRedirectToHome()
        {
            var result = await _controller.Logout() as RedirectToActionResult;

            _accountServiceMock.Verify(s => s.LogoutAsync(), Times.Once);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Home"));
        }

        // Note: For ExternalLogin methods, testing Url manipulation and Challenge results requires extensive mocking of HttpContext, routing, and UrlHelper. Often skipped or verified to return correctly typed results.

        [Test]
        public async Task ExternalLoginCallback_ShouldRedirectToHome_WhenSucceeds()
        {
            _accountServiceMock.Setup(s => s.ExternalLoginCallbackAsync())
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var result = await _controller.ExternalLoginCallback() as LocalRedirectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Url, Is.EqualTo("/"));
        }

        [Test]
        public async Task ExternalLoginCallback_ShouldRedirectToLogin_WhenFails()
        {
            _accountServiceMock.Setup(s => s.ExternalLoginCallbackAsync())
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var result = await _controller.ExternalLoginCallback() as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Login"));
        }
    }
}
