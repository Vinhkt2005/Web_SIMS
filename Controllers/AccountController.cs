using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.ViewModels;
using AuthService = Web_SIMS.Interfaces.Services.IAuthenticationService;

namespace Web_SIMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authenticationService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AuthService authenticationService, ILogger<AccountController> logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && !Url.IsLocalUrl(returnUrl))
            {
                _logger.LogWarning("Phát hiện returnUrl không hợp lệ");
                returnUrl = null;
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (model == null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                ViewData["ReturnUrl"] = model.ReturnUrl;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Check if account is locked
                if (await _authenticationService.IsAccountLockedAsync(model.Username))
                {
                    ModelState.AddModelError("", "Tài khoản đã bị khóa tạm thời. Vui lòng thử lại sau.");
                    return View(model);
                }

                // Validate user credentials
                var user = await _authenticationService.ValidateUserAsync(model.Username, model.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                    
                    // Consider implementing account lockout after multiple failed attempts
                    // await _authenticationService.LockAccountAsync(model.Username, TimeSpan.FromMinutes(15));
                    
                    return View(model);
                }

                // Create claims principal and sign in
                var claimsPrincipal = await _authenticationService.CreateClaimsPrincipalAsync(user);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                _logger.LogInformation("User {Username} logged in successfully", model.Username);

                // Redirect to return URL or default page
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                // Redirect based on user role
                if (user.Role?.RoleName == "Admin")
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else if (user.Role?.RoleName == "Student")
                {
                    return RedirectToAction("Home", "StudentPortal");
                }
                else if (user.Role?.RoleName == "Faculty")
                {
                    return RedirectToAction("Home", "Faculty");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for username: {Username}", model?.Username);
                ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình đăng nhập. Vui lòng thử lại.");
                return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var username = User.Identity?.Name;
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                
                _logger.LogInformation("User {Username} logged out successfully", username);
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout");
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await _authenticationService.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    ViewBag.Message = "Nếu email tồn tại trong hệ thống, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu.";
                    return View("ForgotPasswordConfirmation");
                }

                // In a real application, you would send an email with a reset token
                // For now, we'll just show a confirmation message
                ViewBag.Message = "Hướng dẫn đặt lại mật khẩu đã được gửi đến email của bạn.";
                return View("ForgotPasswordConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during forgot password for email: {Email}", model?.Email);
                ModelState.AddModelError("", "Có lỗi xảy ra. Vui lòng thử lại.");
                return View(model);
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    ModelState.AddModelError("", "Không thể xác định người dùng.");
                    return View(model);
                }

                var success = await _authenticationService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
                if (success)
                {
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
                    return RedirectToAction("ChangePassword");
                }
                else
                {
                    ModelState.AddModelError("", "Mật khẩu hiện tại không đúng.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password change for user");
                ModelState.AddModelError("", "Có lỗi xảy ra khi đổi mật khẩu.");
                return View(model);
            }
        }
    }
}