using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Controllers
{
    [Authorize(Roles = "Admin,Faculty")]
    [Route("admin/courses")]
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IFacultyService _facultyService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly ILogger<CourseController> _logger;

        public CourseController(ICourseService courseService, IFacultyService facultyService, IEnrollmentService enrollmentService, ILogger<CourseController> logger)
        {
            _courseService = courseService;
            _facultyService = facultyService;
            _enrollmentService = enrollmentService;
            _logger = logger;
        }

        private async Task LoadFacultiesAsync()
        {
            try
            {
                var filterOptions = new FacultyFilterOptions
                {
                    StatusFilter = "active",
                    PageSize = 1000 // Load all active faculties
                };
                var paginatedFaculties = await _facultyService.GetFacultiesAsync(filterOptions);
                ViewBag.Faculties = paginatedFaculties.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading faculties for dropdown");
                ViewBag.Faculties = new List<Faculty>();
            }
        }

        // GET: /admin/courses
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

                var filterOptions = new CourseFilterOptions
                {
                    SearchString = searchString,
                    StatusFilter = statusFilter,
                    SortOrder = sortOrder,
                    PageNumber = pageNumber ?? 1,
                    PageSize = 10
                };

                var courses = await _courseService.GetCoursesAsync(filterOptions);
                return View(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting courses list");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách khóa học.";
                return View(new PaginatedList<Course>(new List<Course>(), 0, 1, 10));
            }
        }

        // GET: /admin/courses/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var course = await _courseService.GetCourseWithDetailsAsync(id.Value);
                if (course == null)
                {
                    return NotFound();
                }

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting course details for id: {CourseId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin khóa học.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /admin/courses/create
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            await LoadFacultiesAsync();
            return View();
        }

        // POST: /admin/courses/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseCode,CourseName,Description,Credits,Semester,AcademicYear,Instructor,MaxStudents,IsActive,FacultyId")] Course course)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _courseService.CreateCourseAsync(course);
                    TempData["SuccessMessage"] = "Tạo khóa học thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating course");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo khóa học.");
                }
            }

            await LoadFacultiesAsync();
            return View(course);
        }

        // GET: /admin/courses/{id}/edit
        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var course = await _courseService.GetCourseByIdAsync(id.Value);
                if (course == null)
                {
                    return NotFound();
                }

                await LoadFacultiesAsync();
                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting course for edit: {CourseId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin khóa học.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/courses/{id}/edit
        [HttpPost("{id}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseId,CourseCode,CourseName,Description,Credits,Semester,AcademicYear,Instructor,MaxStudents,IsActive,FacultyId")] Course course)
        {
            if (id != course.CourseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _courseService.UpdateCourseAsync(id, course);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Cập nhật khóa học thành công.";
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
                    _logger.LogError(ex, "Error occurred while updating course: {CourseId}", id);
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật khóa học.");
                }
            }

            await LoadFacultiesAsync();
            return View(course);
        }

        // GET: /admin/courses/delete/{id}
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var course = await _courseService.GetCourseByIdAsync(id.Value);
                if (course == null)
                {
                    return NotFound();
                }

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting course for delete: {CourseId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin khóa học.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/courses/delete/{id}
        [HttpPost("delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _courseService.DeleteCourseAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Xóa khóa học thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy khóa học để xóa.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting course: {CourseId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa khóa học.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /admin/courses/check-course-code
        [HttpPost("check-course-code")]
        public async Task<IActionResult> CheckCourseCode(string courseCode, int? excludeCourseId)
        {
            try
            {
                var exists = await _courseService.IsCourseCodeExistsAsync(courseCode, excludeCourseId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking course code: {CourseCode}", courseCode);
                return Json(new { exists = false, error = "Có lỗi xảy ra khi kiểm tra mã khóa học." });
            }
        }

        // GET: /admin/courses/by-faculty/{facultyId}
        [HttpGet("by-faculty/{facultyId}")]
        public async Task<IActionResult> GetCoursesByFaculty(int facultyId)
        {
            try
            {
                var courses = await _courseService.GetCoursesByFacultyAsync(facultyId);
                return Json(courses.Select(c => new { id = c.CourseId, name = c.CourseName, code = c.CourseCode }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting courses by faculty: {FacultyId}", facultyId);
                return Json(new { error = "Có lỗi xảy ra khi tải danh sách khóa học." });
            }
        }

        // GET: /admin/courses/{id}/enrollments
        [HttpGet("{id}/enrollments")]
        public async Task<IActionResult> Enrollments(int id, string searchString, string statusFilter, int? pageNumber)
        {
            try
            {
                // Get course information
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy khóa học.";
                    return RedirectToAction(nameof(Index));
                }

                ViewData["CourseId"] = id;
                ViewData["CourseName"] = course.CourseName;
                ViewData["CourseCode"] = course.CourseCode;
                ViewData["CurrentFilter"] = searchString;
                ViewData["StatusFilter"] = statusFilter;

                // Get enrollments for this course
                var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(id);
                
                // Apply filters
                if (!string.IsNullOrEmpty(searchString))
                {
                    enrollments = enrollments.Where(e => 
                        (e.Student != null && e.Student.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                        (e.Student != null && e.Student.StudentCode.Contains(searchString, StringComparison.OrdinalIgnoreCase)));
                }

                if (!string.IsNullOrEmpty(statusFilter))
                {
                    if (Enum.TryParse<EnrollmentStatus>(statusFilter, out var status))
                    {
                        enrollments = enrollments.Where(e => e.Status == status);
                    }
                }

                // Apply pagination
                const int pageSize = 10;
                var enrollmentsList = enrollments.ToList();
                var totalCount = enrollmentsList.Count;
                var items = enrollmentsList.Skip(((pageNumber ?? 1) - 1) * pageSize).Take(pageSize).ToList();
                
                var paginatedEnrollments = new PaginatedList<Enrollment>(items, totalCount, pageNumber ?? 1, pageSize);

                return View(paginatedEnrollments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting enrollments for course: {CourseId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách sinh viên.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}