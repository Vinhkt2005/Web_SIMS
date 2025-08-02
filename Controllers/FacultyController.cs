using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Controllers
{
    [Authorize(Roles = "Faculty")]
    public class FacultyController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IFacultyService _facultyService;
        private readonly IUserService _userService;
        private readonly ILogger<FacultyController> _logger;

        public FacultyController(
            ICourseService courseService,
            IEnrollmentService enrollmentService,
            IFacultyService facultyService,
            IUserService userService,
            ILogger<FacultyController> logger)
        {
            _courseService = courseService;
            _enrollmentService = enrollmentService;
            _facultyService = facultyService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> Home()
        {
            try
            {
                var facultyIdClaim = User.FindFirst("FacultyId");
                if (facultyIdClaim == null || !int.TryParse(facultyIdClaim.Value, out int facultyId))
                {
                    _logger.LogWarning("Faculty ID not found in claims for user: {Username}", User.Identity?.Name);
                    return RedirectToAction("AccessDenied", "Account");
                }

                var courses = await _courseService.GetCoursesByFacultyAsync(facultyId);
                var courseList = courses.ToList();

                int totalStudents = 0;
                foreach (var course in courseList)
                {
                    var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(course.CourseId);
                    totalStudents += enrollments.Count(e => e.Status == Models.EnrollmentStatus.Approved);
                }

            var viewModel = new FacultyHomeViewModel
            {
                    ClassCount = courseList.Count,
                    StudentCount = totalStudents
            };

            return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading faculty home page");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang chủ.";
                return View(new FacultyHomeViewModel());
            }
        }

        public async Task<IActionResult> MyClasses()
        {
            try
            {
                var facultyIdClaim = User.FindFirst("FacultyId");
                if (facultyIdClaim == null || !int.TryParse(facultyIdClaim.Value, out int facultyId))
                {
                    _logger.LogWarning("Faculty ID not found in claims for user: {Username}", User.Identity?.Name);
                    return RedirectToAction("AccessDenied", "Account");
                }

                var courses = await _courseService.GetCoursesByFacultyAsync(facultyId);
                var courseList = courses.ToList();

                // Load enrollment details for each course
                foreach (var course in courseList)
                {
                    var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(course.CourseId);
                    course.Enrollments = enrollments.ToList();
                }

                return View(courseList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading faculty classes");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách lớp học.";
                return View(new List<Models.Course>());
            }
        }

        public async Task<IActionResult> ClassDetails(int id)
        {
            try
            {
                var course = await _courseService.GetCourseWithDetailsAsync(id);
                if (course == null)
            {
                return NotFound();
            }

                // Verify faculty ownership
                var facultyIdClaim = User.FindFirst("FacultyId");
                if (facultyIdClaim == null || !int.TryParse(facultyIdClaim.Value, out int facultyId) || course.FacultyId != facultyId)
                {
                    return Forbid();
                }

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading class details for course: {CourseId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin lớp học.";
                return RedirectToAction(nameof(MyClasses));
            }
        }

        public async Task<IActionResult> StudentList(int courseId)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(courseId);
                if (course == null)
            {
                return NotFound();
            }

                // Verify faculty ownership
                var facultyIdClaim = User.FindFirst("FacultyId");
                if (facultyIdClaim == null || !int.TryParse(facultyIdClaim.Value, out int facultyId) || course.FacultyId != facultyId)
                {
                    return Forbid();
                }

                var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(courseId);
                var approvedEnrollments = enrollments.Where(e => e.Status == Models.EnrollmentStatus.Approved).ToList();

                ViewBag.CourseName = course.CourseName;
                ViewBag.CourseCode = course.CourseCode;

                return View(approvedEnrollments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading student list for course: {CourseId}", courseId);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách sinh viên.";
                return RedirectToAction(nameof(MyClasses));
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEnrollmentStatus(int enrollmentId, Models.EnrollmentStatus status)
        {
            try
            {
                var enrollment = await _enrollmentService.GetEnrollmentWithDetailsAsync(enrollmentId);
                if (enrollment == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đăng ký." });
                }

                // Verify faculty ownership
                var facultyIdClaim = User.FindFirst("FacultyId");
                if (facultyIdClaim == null || !int.TryParse(facultyIdClaim.Value, out int facultyId) || 
                    enrollment.Course?.FacultyId != facultyId)
                {
                    return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này." });
                }

                enrollment.Status = status;
                var success = await _enrollmentService.UpdateEnrollmentAsync(enrollmentId, enrollment);

                if (success)
                {
                    return Json(new { success = true, message = "Cập nhật trạng thái thành công." });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể cập nhật trạng thái." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating enrollment status: {EnrollmentId}", enrollmentId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái." });
            }
        }

        public async Task<IActionResult> Students(int classId)
        {
            try
            {
                var facultyIdClaim = User.FindFirst("FacultyId");
                if (facultyIdClaim == null || !int.TryParse(facultyIdClaim.Value, out int facultyId))
                {
                    _logger.LogWarning("Faculty ID not found in claims for user: {Username}", User.Identity?.Name);
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Verify that this course belongs to the current faculty
                var course = await _courseService.GetCourseByIdAsync(classId);
                if (course == null || course.FacultyId != facultyId)
                {
                    return Forbid();
                }

                // Get students enrolled in this course
                var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(classId);
                var approvedEnrollments = enrollments.Where(e => e.Status == Models.EnrollmentStatus.Approved).ToList();

                ViewBag.CourseName = course.CourseName;
                ViewBag.CourseCode = course.CourseCode;
                
                return View(approvedEnrollments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading students for course: {CourseId}", classId);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách sinh viên.";
                return RedirectToAction("MyClasses");
            }
        }

        public IActionResult Grades(int classId)
        {
            TempData["InfoMessage"] = "Tính năng nhập điểm đang trong quá trình phát triển. Vui lòng quay lại sau!";
            return RedirectToAction("MyClasses");
        }

        public IActionResult GradeReports()
        {
            TempData["InfoMessage"] = "Tính năng báo cáo điểm số đang trong quá trình phát triển. Vui lòng quay lại sau!";
            return RedirectToAction("Home");
        }

        public async Task<IActionResult> Profile()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogWarning("User ID not found in claims for user: {Username}", User.Identity?.Name);
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Get user with role information
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading faculty profile");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin cá nhân.";
                return RedirectToAction("Home");
            }
        }
    }
}