using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Web_SIMS.Data;
using Web_SIMS.Helpers;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(AppDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: User
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["UsernameSortParm"] = sortOrder == "Username" ? "username_desc" : "Username";
            ViewData["CurrentFilter"] = searchString;

            var users = from u in _context.Users.Include(u => u.Role)
                        select u;

            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => (u.FullName != null && u.FullName.Contains(searchString))
                                   || u.Username.Contains(searchString)
                                   || u.Email.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    users = users.OrderByDescending(u => u.FullName);
                    break;
                case "Username":
                    users = users.OrderBy(u => u.Username);
                    break;
                case "username_desc":
                    users = users.OrderByDescending(u => u.Username);
                    break;
                default:
                    users = users.OrderBy(u => u.FullName);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<User>.CreateAsync(users.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            ViewData["RoleId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Roles.Where(r => r.IsActive), "RoleId", "RoleName");
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Password,Email,FullName,RoleId")] User user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra username đã tồn tại
                if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                    ViewData["RoleId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Roles.Where(r => r.IsActive), "RoleId", "RoleName");
                    return View(user);
                }

                // Kiểm tra email đã tồn tại
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại.");
                    ViewData["RoleId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Roles.Where(r => r.IsActive), "RoleId", "RoleName");
                    return View(user);
                }

                // Hash mật khẩu
                var (hashedPassword, salt) = PasswordHelper.HashPasswordWithSalt(user.Password);
                user.Password = hashedPassword;
                user.PasswordSalt = salt;
                user.IsActive = true;
                user.CreatedDate = DateTime.Now;

                _context.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Người dùng mới được tạo: {user.FullName} ({user.Username})");
                TempData["SuccessMessage"] = "Người dùng đã được tạo thành công.";

                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Roles.Where(r => r.IsActive), "RoleId", "RoleName");
            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Roles.Where(r => r.IsActive), "RoleId", "RoleName");
            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Username,Email,FullName,RoleId,IsActive")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra username đã tồn tại (trừ chính nó)
                    if (await _context.Users.AnyAsync(u => u.Username == user.Username && u.UserId != id))
                    {
                        ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                        ViewData["RoleId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Roles.Where(r => r.IsActive), "RoleId", "RoleName");
                        return View(user);
                    }

                    // Kiểm tra email đã tồn tại (trừ chính nó)
                    if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.UserId != id))
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại.");
                        ViewData["RoleId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Roles.Where(r => r.IsActive), "RoleId", "RoleName");
                        return View(user);
                    }

                    user.UpdatedDate = DateTime.Now;
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Người dùng được cập nhật: {user.FullName} ({user.Username})");
                    TempData["SuccessMessage"] = "Thông tin người dùng đã được cập nhật thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Roles.Where(r => r.IsActive), "RoleId", "RoleName");
            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Soft delete - chỉ đánh dấu không active
                user.IsActive = false;
                user.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Người dùng bị xóa: {user.FullName} ({user.Username})");
                TempData["SuccessMessage"] = "Người dùng đã được xóa thành công.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}