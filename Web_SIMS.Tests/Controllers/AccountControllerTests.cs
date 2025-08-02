using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Web_SIMS.Controllers;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Web_SIMS.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IAuthenticationService> _mockAuthService;
        private readonly Mock<ILogger<AccountController>> _mockLogger;
        private readonly AccountController _controller;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<IAuthenticationService> _mockHttpAuthService;

        public AccountControllerTests()
        {
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockLogger = new Mock<ILogger<AccountController>>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpAuthService = new Mock<IAuthenticationService>();

            _controller = new AccountController(_mockAuthService.Object, _mockLogger.Object);
            
            // Setup HttpContext
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = _mockHttpContext.Object
            };

            // Setup authentication service for HttpContext
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IAuthenticationService)))
                          .Returns(_mockHttpAuthService.Object);
            _mockHttpContext.Setup(x => x.RequestServices).Returns(serviceProvider.Object);
        }

        [Fact]
        public void Login_Get_ReturnsViewWithModel()
        {
            // Arrange
            string returnUrl = "/dashboard";

            // Act
            var result = _controller.Login(returnUrl);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<LoginViewModel>().Subject;
            model.ReturnUrl.Should().Be(returnUrl);
            viewResult.ViewData["ReturnUrl"].Should().Be(returnUrl);
        }

        [Fact]
        public void Login_Get_WithInvalidReturnUrl_SetsReturnUrlToNull()
        {
            // Arrange
            string invalidReturnUrl = "http://external-site.com/malicious";
            
            // Setup URL helper mock
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(x => x.IsLocalUrl(invalidReturnUrl)).Returns(false);
            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = _controller.Login(invalidReturnUrl);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<LoginViewModel>().Subject;
            model.ReturnUrl.Should().BeNull();
            viewResult.ViewData["ReturnUrl"].Should().BeNull();
        }

        [Fact]
        public async Task Login_Post_WithValidCredentials_RedirectsToCorrectPage()
        {
            // Arrange
            var loginModel = new LoginViewModel
            {
                Username = "admin",
                Password = "admin123",
                ReturnUrl = null
            };

            var user = new User
            {
                UserId = 1,
                Username = "admin",
                Email = "admin@sims.edu",
                FullName = "Administrator",
                Role = new Role { RoleId = 1, RoleName = "Admin" }
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin")
            }, CookieAuthenticationDefaults.AuthenticationScheme));

            _mockAuthService.Setup(x => x.IsAccountLockedAsync(loginModel.Username))
                           .ReturnsAsync(false);
            _mockAuthService.Setup(x => x.ValidateUserAsync(loginModel.Username, loginModel.Password))
                           .ReturnsAsync(user);
            _mockAuthService.Setup(x => x.CreateClaimsPrincipalAsync(user))
                           .ReturnsAsync(claimsPrincipal);

            _mockHttpAuthService.Setup(x => x.SignInAsync(
                It.IsAny<HttpContext>(),
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Dashboard");
        }

        [Fact]
        public async Task Login_Post_WithInvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            var loginModel = new LoginViewModel
            {
                Username = "invalid",
                Password = "invalid",
                ReturnUrl = null
            };

            _mockAuthService.Setup(x => x.IsAccountLockedAsync(loginModel.Username))
                           .ReturnsAsync(false);
            _mockAuthService.Setup(x => x.ValidateUserAsync(loginModel.Username, loginModel.Password))
                           .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(loginModel);
            _controller.ModelState.Should().ContainKey("");
            _controller.ModelState[""]!.Errors.Should().Contain(e => 
                e.ErrorMessage == "Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        [Fact]
        public async Task Login_Post_WithLockedAccount_ReturnsViewWithError()
        {
            // Arrange
            var loginModel = new LoginViewModel
            {
                Username = "locked_user",
                Password = "password123",
                ReturnUrl = null
            };

            _mockAuthService.Setup(x => x.IsAccountLockedAsync(loginModel.Username))
                           .ReturnsAsync(true);

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(loginModel);
            _controller.ModelState.Should().ContainKey("");
            _controller.ModelState[""]!.Errors.Should().Contain(e => 
                e.ErrorMessage == "Tài khoản đã bị khóa tạm thời. Vui lòng thử lại sau.");
        }

        [Fact]
        public async Task Login_Post_WithStudentRole_RedirectsToStudentPortal()
        {
            // Arrange
            var loginModel = new LoginViewModel
            {
                Username = "student1",
                Password = "student123",
                ReturnUrl = null
            };

            var user = new User
            {
                UserId = 3,
                Username = "student1",
                Email = "student1@sims.edu",
                FullName = "Nguyễn Văn A",
                Role = new Role { RoleId = 3, RoleName = "Student" }
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "3"),
                new Claim(ClaimTypes.Name, "student1"),
                new Claim(ClaimTypes.Role, "Student")
            }, CookieAuthenticationDefaults.AuthenticationScheme));

            _mockAuthService.Setup(x => x.IsAccountLockedAsync(loginModel.Username))
                           .ReturnsAsync(false);
            _mockAuthService.Setup(x => x.ValidateUserAsync(loginModel.Username, loginModel.Password))
                           .ReturnsAsync(user);
            _mockAuthService.Setup(x => x.CreateClaimsPrincipalAsync(user))
                           .ReturnsAsync(claimsPrincipal);

            _mockHttpAuthService.Setup(x => x.SignInAsync(
                It.IsAny<HttpContext>(),
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Home");
            redirectResult.ControllerName.Should().Be("StudentPortal");
        }

        [Fact]
        public async Task Logout_Post_SignsOutAndRedirectsToLogin()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "testuser")
            }, CookieAuthenticationDefaults.AuthenticationScheme));

            _controller.ControllerContext.HttpContext.User = user;

            _mockHttpAuthService.Setup(x => x.SignOutAsync(
                It.IsAny<HttpContext>(),
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Login");

            _mockHttpAuthService.Verify(x => x.SignOutAsync(
                It.IsAny<HttpContext>(),
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<AuthenticationProperties>()), Times.Once);
        }

        [Fact]
        public async Task ChangePassword_Post_WithValidData_ReturnsSuccessRedirect()
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                CurrentPassword = "oldpassword",
                NewPassword = "newpassword123",
                ConfirmPassword = "newpassword123"
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, CookieAuthenticationDefaults.AuthenticationScheme));

            _controller.ControllerContext.HttpContext.User = user;

            _mockAuthService.Setup(x => x.ChangePasswordAsync(1, model.CurrentPassword, model.NewPassword))
                           .ReturnsAsync(true);

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("ChangePassword");
            _controller.TempData["SuccessMessage"].Should().Be("Đổi mật khẩu thành công.");
        }

        [Fact]
        public async Task ChangePassword_Post_WithInvalidCurrentPassword_ReturnsViewWithError()
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                CurrentPassword = "wrongpassword",
                NewPassword = "newpassword123",
                ConfirmPassword = "newpassword123"
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, CookieAuthenticationDefaults.AuthenticationScheme));

            _controller.ControllerContext.HttpContext.User = user;

            _mockAuthService.Setup(x => x.ChangePasswordAsync(1, model.CurrentPassword, model.NewPassword))
                           .ReturnsAsync(false);

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(model);
            _controller.ModelState.Should().ContainKey("");
            _controller.ModelState[""]!.Errors.Should().Contain(e => 
                e.ErrorMessage == "Mật khẩu hiện tại không đúng.");
        }
    }
}