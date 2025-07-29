using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using Web_SIMS.Data;
using Web_SIMS.Helpers;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AppDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
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

                // Tìm kiếm người dùng theo tên đăng nhập
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == model.Username && u.IsActive);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                    _logger.LogWarning($"Đăng nhập thất bại cho người dùng {model.Username}");
                    return View(model);
                }

                // Kiểm tra mật khẩu đã hash hoặc plain text (để tương thích với dữ liệu cũ)
                bool isPasswordValid = false;
                if (!string.IsNullOrEmpty(user.PasswordSalt))
                {
                    // Sử dụng hash + salt
                    isPasswordValid = PasswordHelper.VerifyPassword(model.Password, user.Password, user.PasswordSalt);
                }
                else
                {
                    // Fallback cho dữ liệu cũ (plain text)
                    isPasswordValid = user.Password == model.Password;
                }

                if (!isPasswordValid)
                {
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                    _logger.LogWarning($"Đăng nhập thất bại cho người dùng {model.Username}");
                    return View(model);
                }

                _logger.LogInformation($"Người dùng {model.Username} đăng nhập thành công.");

                // Tạo danh sách claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "NoRole"),
                    new Claim("FullName", user.FullName ?? string.Empty)
                };

                // Tạo identity và principal
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Cấu hình đăng nhập
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                // Đăng nhập người dùng
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties);

                // Chuyển hướng đến trang yêu cầu hoặc dashboard
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình đăng nhập");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại sau.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Không tìm thấy thông tin người dùng.");
                    return View(model);
                }

                // Kiểm tra mật khẩu hiện tại
                bool isCurrentPasswordValid = false;
                if (!string.IsNullOrEmpty(user.PasswordSalt))
                {
                    isCurrentPasswordValid = PasswordHelper.VerifyPassword(model.CurrentPassword, user.Password, user.PasswordSalt);
                }
                else
                {
                    isCurrentPasswordValid = user.Password == model.CurrentPassword;
                }

                if (!isCurrentPasswordValid)
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                    return View(model);
                }

                // Hash mật khẩu mới
                var (hashedPassword, salt) = PasswordHelper.HashPasswordWithSalt(model.NewPassword);
                user.Password = hashedPassword;
                user.PasswordSalt = salt;
                user.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Người dùng {user.Username} đã đổi mật khẩu thành công.");
                TempData["SuccessMessage"] = "Mật khẩu đã được thay đổi thành công.";

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đổi mật khẩu");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi đổi mật khẩu. Vui lòng thử lại sau.");
                return View(model);
            }
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

                if (user == null)
                {
                    // Không hiển thị lỗi để tránh lộ thông tin
                    TempData["SuccessMessage"] = "Nếu email tồn tại trong hệ thống, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu.";
                    return RedirectToAction("Login");
                }

                // Tạo token reset password (đơn giản - trong thực tế nên dùng JWT)
                var resetToken = Guid.NewGuid().ToString();
                // Lưu token vào database hoặc cache (đơn giản hóa)

                // Gửi email (đơn giản hóa - trong thực tế nên dùng email service)
                _logger.LogInformation($"Reset password token cho {user.Email}: {resetToken}");

                TempData["SuccessMessage"] = "Nếu email tồn tại trong hệ thống, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý quên mật khẩu");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi. Vui lòng thử lại sau.");
                return View(model);
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}