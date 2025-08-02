using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/users")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: /admin/users
        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, string roleFilter, string statusFilter, string sortOrder, int? pageNumber)
        {
            try
            {
                ViewData["CurrentSort"] = sortOrder;
                ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewData["UsernameSortParm"] = sortOrder == "Username" ? "username_desc" : "Username";
                ViewData["CurrentFilter"] = searchString;
                ViewData["RoleFilter"] = roleFilter;
                ViewData["StatusFilter"] = statusFilter;

                var filterOptions = new UserFilterOptions
                {
                    SearchString = searchString,
                    RoleFilter = roleFilter,
                    StatusFilter = statusFilter,
                    SortOrder = sortOrder,
                    PageNumber = pageNumber ?? 1,
                    PageSize = 10
                };

                var users = await _userService.GetUsersAsync(filterOptions);
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting users list");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách người dùng.";
                return View(new PaginatedList<User>(new List<User>(), 0, 1, 10));
            }
        }

        // GET: /admin/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(id.Value);
                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user details for id: {UserId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin người dùng.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /admin/users/create
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                // Load roles for dropdown
                var roles = await _userService.GetRolesAsync();
                ViewBag.RoleId = roles.Select(r => new SelectListItem
                {
                    Value = r.RoleId.ToString(),
                    Text = r.RoleName
                }).ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading create user page");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang tạo người dùng.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/users/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Password,Email,FullName,IsActive,RoleId,StudentId,FacultyId")] User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.CreateUserAsync(user);
                    TempData["SuccessMessage"] = "Tạo người dùng thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating user");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo người dùng.");
                }
            }

            // Reload roles for dropdown if validation fails
            try
            {
                var roles = await _userService.GetRolesAsync();
                ViewBag.RoleId = roles.Select(r => new SelectListItem
                {
                    Value = r.RoleId.ToString(),
                    Text = r.RoleName,
                    Selected = r.RoleId == user.RoleId
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading roles");
            }

            return View(user);
        }

        // GET: /admin/users/{id}/edit
        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(id.Value);
                if (user == null)
                {
                    return NotFound();
                }

                // Load roles for dropdown
                var roles = await _userService.GetRolesAsync();
                ViewBag.RoleId = roles.Select(r => new SelectListItem
                {
                    Value = r.RoleId.ToString(),
                    Text = r.RoleName,
                    Selected = r.RoleId == user.RoleId
                }).ToList();

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user for edit: {UserId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin người dùng.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/users/{id}/edit
        [HttpPost("{id}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Username,Password,Email,FullName,IsActive,RoleId,StudentId,FacultyId")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _userService.UpdateUserAsync(id, user);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Cập nhật người dùng thành công.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating user: {UserId}", id);
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật người dùng.");
                }
            }

            // Reload roles for dropdown if validation fails
            try
            {
                var roles = await _userService.GetRolesAsync();
                ViewBag.RoleId = roles.Select(r => new SelectListItem
                {
                    Value = r.RoleId.ToString(),
                    Text = r.RoleName,
                    Selected = r.RoleId == user.RoleId
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading roles");
            }

            return View(user);
        }

        // GET: /admin/users/{id}/delete
        [HttpGet("{id}/delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var user = await _userService.GetUserByIdAsync(id.Value);
                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user for delete: {UserId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin người dùng.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/users/{id}/delete
        [HttpPost("{id}/delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _userService.DeleteUserAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Xóa người dùng thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng để xóa.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user: {UserId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa người dùng.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /admin/users/check-username
        [HttpPost("check-username")]
        public async Task<IActionResult> CheckUsername(string username, int? excludeUserId)
        {
            try
            {
                var exists = await _userService.IsUsernameExistsAsync(username, excludeUserId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking username: {Username}", username);
                return Json(new { exists = false, error = "Có lỗi xảy ra khi kiểm tra tên đăng nhập." });
            }
        }

        // POST: /admin/users/check-email
        [HttpPost("check-email")]
        public async Task<IActionResult> CheckEmail(string email, int? excludeUserId)
        {
            try
            {
                var exists = await _userService.IsEmailExistsAsync(email, excludeUserId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking email: {Email}", email);
                return Json(new { exists = false, error = "Có lỗi xảy ra khi kiểm tra email." });
            }
        }

        // POST: /admin/users/{id}/change-password
        [HttpPost("{id}/change-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                var success = await _userService.ChangePasswordAsync(id, model.CurrentPassword, model.NewPassword);
                if (success)
                {
                    return Json(new { success = true, message = "Đổi mật khẩu thành công." });
                }
                else
                {
                    return Json(new { success = false, message = "Mật khẩu hiện tại không đúng hoặc người dùng không tồn tại." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing password for user: {UserId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi đổi mật khẩu." });
            }
        }

        // POST: /admin/users/{id}/toggle-status
        [HttpPost("{id}/toggle-status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                // Toggle the status
                user.IsActive = !user.IsActive;
                var success = await _userService.UpdateUserAsync(id, user);

                if (success)
                {
                    var statusText = user.IsActive ? "kích hoạt" : "vô hiệu hóa";
                    TempData["SuccessMessage"] = $"Đã {statusText} người dùng thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật trạng thái người dùng.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling user status: {UserId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật trạng thái người dùng.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /admin/users/{id}/toggle-status
        [HttpGet("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatusGet(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                // Toggle the status
                user.IsActive = !user.IsActive;
                var success = await _userService.UpdateUserAsync(id, user);

                if (success)
                {
                    var statusText = user.IsActive ? "kích hoạt" : "vô hiệu hóa";
                    TempData["SuccessMessage"] = $"Đã {statusText} người dùng thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật trạng thái người dùng.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling user status: {UserId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật trạng thái người dùng.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}