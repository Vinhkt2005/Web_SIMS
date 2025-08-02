using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/faculties")]
    public class FacultyManagementController : Controller
    {
        private readonly IFacultyService _facultyService;
        private readonly IUserService _userService;
        private readonly ILogger<FacultyManagementController> _logger;

        public FacultyManagementController(
            IFacultyService facultyService,
            IUserService userService,
            ILogger<FacultyManagementController> logger)
        {
            _facultyService = facultyService;
            _userService = userService;
            _logger = logger;
        }

        // GET: /admin/faculties
        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, string statusFilter, string accountFilter, string sortOrder, int? pageNumber)
        {
            try
            {
                ViewData["CurrentSort"] = sortOrder;
                ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewData["CodeSortParm"] = sortOrder == "Code" ? "code_desc" : "Code";
                ViewData["DepartmentSortParm"] = sortOrder == "Department" ? "department_desc" : "Department";
                ViewData["CurrentFilter"] = searchString;
                ViewData["StatusFilter"] = statusFilter;
                ViewData["AccountFilter"] = accountFilter;

                var filterOptions = new FacultyFilterOptions
                {
                    SearchString = searchString,
                    StatusFilter = statusFilter,
                    SortOrder = sortOrder,
                    PageNumber = pageNumber ?? 1,
                    PageSize = 10
                };

                var faculties = await _facultyService.GetFacultiesAsync(filterOptions);
                return View(faculties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting faculties list");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách giảng viên.";
                return View(new PaginatedList<Faculty>(new List<Faculty>(), 0, 1, 10));
            }
        }

        // GET: /admin/faculties/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var faculty = await _facultyService.GetFacultyWithDetailsAsync(id.Value);
                if (faculty == null)
                {
                    return NotFound();
                }

                return View(faculty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting faculty details for id: {FacultyId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin giảng viên.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /admin/faculties/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /admin/faculties/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFacultyViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Create Faculty object from ViewModel
                    var faculty = new Faculty
                    {
                        FacultyCode = model.FacultyCode,
                        FullName = model.FullName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        DateOfBirth = model.DateOfBirth,
                        Gender = model.Gender,
                        Department = model.Department,
                        Specialization = model.Specialization,
                        IsActive = true
                    };

                    // Only create user account if username and password are provided
                    if (!string.IsNullOrWhiteSpace(model.Username) && !string.IsNullOrWhiteSpace(model.Password))
                    {
                        await _facultyService.CreateFacultyAsync(faculty, model.Username, model.Password);
                    }
                    else
                    {
                        await _facultyService.CreateFacultyAsync(faculty);
                    }
                    TempData["SuccessMessage"] = "Tạo giảng viên thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating faculty");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo giảng viên.");
                }
            }

            return View(model);
        }

        // GET: /admin/faculties/{id}/edit
        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var faculty = await _facultyService.GetFacultyByIdAsync(id.Value);
                if (faculty == null)
                {
                    return NotFound();
                }

                return View(faculty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting faculty for edit: {FacultyId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin giảng viên.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/faculties/{id}/edit
        [HttpPost("{id}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FacultyId,FacultyCode,FullName,Email,PhoneNumber,Address,DateOfBirth,Gender,Department,Specialization,IsActive")] Faculty faculty)
        {
            if (id != faculty.FacultyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _facultyService.UpdateFacultyAsync(id, faculty);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Cập nhật giảng viên thành công.";
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
                    _logger.LogError(ex, "Error occurred while updating faculty: {FacultyId}", id);
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật giảng viên.");
                }
            }

            return View(faculty);
        }

        // GET: /admin/faculties/delete/{id}
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var faculty = await _facultyService.GetFacultyByIdAsync(id.Value);
                if (faculty == null)
                {
                    return NotFound();
                }

                return View(faculty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting faculty for delete: {FacultyId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin giảng viên.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/faculties/delete/{id}
        [HttpPost("delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _facultyService.DeleteFacultyAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Xóa giảng viên thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy giảng viên để xóa.";
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error occurred while deleting faculty: {FacultyId}", id);
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting faculty: {FacultyId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa giảng viên.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /admin/faculties/check-faculty-code
        [HttpPost("check-faculty-code")]
        public async Task<IActionResult> CheckFacultyCode(string facultyCode, int? excludeFacultyId)
        {
            try
            {
                var exists = await _facultyService.IsFacultyCodeExistsAsync(facultyCode, excludeFacultyId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking faculty code: {FacultyCode}", facultyCode);
                return Json(new { exists = false, error = "Có lỗi xảy ra khi kiểm tra mã giảng viên." });
            }
        }

        // POST: /admin/faculties/check-email
        [HttpPost("check-email")]
        public async Task<IActionResult> CheckEmail(string email, int? excludeFacultyId)
        {
            try
            {
                var exists = await _facultyService.IsEmailExistsAsync(email, excludeFacultyId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking email: {Email}", email);
                return Json(new { exists = false, error = "Có lỗi xảy ra khi kiểm tra email." });
            }
        }

        // GET: /admin/faculties/{id}/create-account
        [HttpGet("{id}/create-account")]
        public async Task<IActionResult> CreateAccount(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var faculty = await _facultyService.GetFacultyByIdAsync(id.Value);
                if (faculty == null)
                {
                    return NotFound();
                }

                // Check if account already exists
                var existingUser = await _userService.GetUserByEmailAsync(faculty.Email);
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "Tài khoản cho giảng viên này đã tồn tại.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                ViewBag.Faculty = faculty;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading create account page for faculty: {FacultyId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang tạo tài khoản.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/faculties/{id}/create-account
        [HttpPost("{id}/create-account")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(int id, string username, string password)
        {
            try
            {
                var faculty = await _facultyService.GetFacultyByIdAsync(id);
                if (faculty == null)
                {
                    return NotFound();
                }

                // Check if account already exists
                var existingUser = await _userService.GetUserByEmailAsync(faculty.Email);
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "Tài khoản cho giảng viên này đã tồn tại.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Create user account
                var user = new User
                {
                    Username = username,
                    Password = password, // Will be hashed in service
                    Email = faculty.Email,
                    FullName = faculty.FullName,
                    IsActive = true,
                    RoleId = 3, // Faculty role
                    FacultyId = faculty.FacultyId
                };

                await _userService.CreateUserAsync(user);
                TempData["SuccessMessage"] = "Tạo tài khoản thành công.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Faculty = await _facultyService.GetFacultyByIdAsync(id);
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating account for faculty: {FacultyId}", id);
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo tài khoản.");
                ViewBag.Faculty = await _facultyService.GetFacultyByIdAsync(id);
                return View();
            }
        }

        // GET: /admin/faculties/by-department/{department}
        [HttpGet("by-department/{department}")]
        public async Task<IActionResult> GetFacultiesByDepartment(string department)
        {
            try
            {
                var faculties = await _facultyService.GetFacultiesByDepartmentAsync(department);
                return Json(faculties.Select(f => new { id = f.FacultyId, name = f.FullName, code = f.FacultyCode }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting faculties by department: {Department}", department);
                return Json(new { error = "Có lỗi xảy ra khi tải danh sách giảng viên." });
            }
        }

        // POST: /admin/faculties/toggle-status/{id}
        [HttpPost("toggle-status/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var faculty = await _facultyService.GetFacultyByIdAsync(id);
                if (faculty == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy giảng viên.";
                    return RedirectToAction(nameof(Index));
                }

                // Toggle the IsActive status
                faculty.IsActive = !faculty.IsActive;
                var success = await _facultyService.UpdateFacultyAsync(faculty.FacultyId, faculty);

                if (success)
                {
                    var statusText = faculty.IsActive ? "kích hoạt" : "vô hiệu hóa";
                    TempData["SuccessMessage"] = $"Đã {statusText} giảng viên {faculty.FullName} thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật trạng thái giảng viên.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling faculty status: {FacultyId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật trạng thái giảng viên.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}