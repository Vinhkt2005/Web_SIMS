using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/students")]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(IStudentService studentService, IEnrollmentService enrollmentService, ILogger<StudentController> logger)
        {
            _studentService = studentService;
            _enrollmentService = enrollmentService;
            _logger = logger;
        }

        // GET: /admin/students
        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, string statusFilter, string sortOrder, int? pageNumber)
        {
            try
            {
                ViewData["CurrentSort"] = sortOrder;
                ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewData["CodeSortParm"] = sortOrder == "Code" ? "code_desc" : "Code";
                ViewData["CurrentFilter"] = searchString;
                ViewData["StatusFilter"] = statusFilter;

                var filterOptions = new StudentFilterOptions
                {
                    SearchString = searchString,
                    StatusFilter = statusFilter,
                    SortOrder = sortOrder,
                    PageNumber = pageNumber ?? 1,
                    PageSize = 10
                };

                var students = await _studentService.GetStudentsAsync(filterOptions);
                return View(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting students list");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách sinh viên.";
                return View(new PaginatedList<Student>(new List<Student>(), 0, 1, 10));
            }
        }

        // GET: /admin/students/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var student = await _studentService.GetStudentWithDetailsAsync(id.Value);
                if (student == null)
                {
                    return NotFound();
                }

                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting student details for id: {StudentId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin sinh viên.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /admin/students/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /admin/students/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Create Student object from ViewModel
                    var student = new Student
                    {
                        StudentCode = model.StudentCode,
                        FullName = model.FullName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        DateOfBirth = model.DateOfBirth,
                        Gender = model.Gender,
                        Major = model.Major,
                        AcademicYear = model.AcademicYear,
                        Notes = model.Notes,
                        IsActive = true
                    };

                    // Only create user account if username and password are provided
                    if (!string.IsNullOrWhiteSpace(model.Username) && !string.IsNullOrWhiteSpace(model.Password))
                    {
                        await _studentService.CreateStudentAsync(student, model.Username, model.Password);
                    }
                    else
                    {
                        await _studentService.CreateStudentAsync(student);
                    }
                    TempData["SuccessMessage"] = "Tạo sinh viên thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating student");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo sinh viên.");
                }
            }

            return View(model);
        }

        // GET: /admin/students/{id}/edit
        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var student = await _studentService.GetStudentByIdAsync(id.Value);
                if (student == null)
                {
                    return NotFound();
                }

                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting student for edit: {StudentId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin sinh viên.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/students/{id}/edit
        [HttpPost("{id}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StudentId,StudentCode,FullName,Email,PhoneNumber,Address,DateOfBirth,Gender,Major,AcademicYear,IsActive,Notes")] Student student)
        {
            if (id != student.StudentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _studentService.UpdateStudentAsync(id, student);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Cập nhật sinh viên thành công.";
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
                    _logger.LogError(ex, "Error occurred while updating student: {StudentId}", id);
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật sinh viên.");
                }
            }

            return View(student);
        }

        // GET: /admin/students/delete/{id}
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var student = await _studentService.GetStudentByIdAsync(id.Value);
                if (student == null)
                {
                    return NotFound();
                }

                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting student for delete: {StudentId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin sinh viên.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/students/delete/{id}
        [HttpPost("delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _studentService.DeleteStudentAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Xóa sinh viên thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sinh viên để xóa.";
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete student {StudentId}: {Message}", id, ex.Message);
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting student: {StudentId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa sinh viên: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /admin/students/{id}/enrollments
        [HttpGet("{id}/enrollments")]
        public async Task<IActionResult> Enrollments(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sinh viên.";
                    return RedirectToAction(nameof(Index));
                }

                var enrollments = await _enrollmentService.GetEnrollmentsByStudentAsync(id);
                
                ViewBag.Student = student;
                return View(enrollments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting enrollments for student: {StudentId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách đăng ký.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/students/check-student-code
        [HttpPost("check-student-code")]
        public async Task<IActionResult> CheckStudentCode(string studentCode, int? excludeStudentId)
        {
            try
            {
                var exists = await _studentService.IsStudentCodeExistsAsync(studentCode, excludeStudentId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking student code: {StudentCode}", studentCode);
                return Json(new { exists = false, error = "Có lỗi xảy ra khi kiểm tra mã sinh viên." });
            }
        }

        // POST: /admin/students/check-email
        [HttpPost("check-email")]
        public async Task<IActionResult> CheckEmail(string email, int? excludeStudentId)
        {
            try
            {
                var exists = await _studentService.IsEmailExistsAsync(email, excludeStudentId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking email: {Email}", email);
                return Json(new { exists = false, error = "Có lỗi xảy ra khi kiểm tra email." });
            }
        }

        // GET: /admin/students/by-major/{major}
        [HttpGet("by-major/{major}")]
        public async Task<IActionResult> GetStudentsByMajor(string major)
        {
            try
            {
                var students = await _studentService.GetStudentsByMajorAsync(major);
                return Json(students.Select(s => new { id = s.StudentId, name = s.FullName, code = s.StudentCode }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting students by major: {Major}", major);
                return Json(new { error = "Có lỗi xảy ra khi tải danh sách sinh viên." });
            }
        }
    }
}