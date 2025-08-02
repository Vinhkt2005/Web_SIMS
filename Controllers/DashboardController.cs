using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Controllers
{
    [Authorize]
    [Route("admin/dashboard")]
    public class DashboardController : Controller
    {
        private readonly IUserService _userService;
        private readonly IStudentService _studentService;
        private readonly ICourseService _courseService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IFacultyService _facultyService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IUserService userService,
            IStudentService studentService,
            ICourseService courseService,
            IEnrollmentService enrollmentService,
            IFacultyService facultyService,
            ILogger<DashboardController> logger)
        {
            _userService = userService;
            _studentService = studentService;
            _courseService = courseService;
            _enrollmentService = enrollmentService;
            _facultyService = facultyService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = new DashboardViewModel();

                // Get all data for statistics
                var allStudents = await _studentService.GetStudentsAsync(new StudentFilterOptions { PageSize = 10000 });
                var allCourses = await _courseService.GetCoursesAsync(new CourseFilterOptions { PageSize = 10000 });
                var allEnrollments = await _enrollmentService.GetEnrollmentsAsync(new EnrollmentFilterOptions { PageSize = 10000 });
                var allUsers = await _userService.GetUsersAsync(new UserFilterOptions { PageSize = 10000 });
                var allFaculties = await _facultyService.GetFacultiesAsync(new FacultyFilterOptions { PageSize = 10000 });

                // Basic statistics
                viewModel.TotalStudents = allStudents.TotalCount;
                viewModel.TotalCourses = allCourses.TotalCount;
                viewModel.TotalEnrollments = allEnrollments.TotalCount;
                viewModel.TotalUsers = allUsers.TotalCount;

                // Active counts
                viewModel.ActiveStudents = allStudents.Count(s => s.IsActive);
                viewModel.ActiveCourses = allCourses.Count(c => c.IsActive);

                // Faculty statistics
                ViewBag.TotalFaculties = allFaculties.TotalCount;
                ViewBag.ActiveFaculties = allFaculties.Count(f => f.IsActive);

                // Pending enrollments
                viewModel.PendingEnrollments = allEnrollments.Count(e => e.Status == EnrollmentStatus.Pending);

                // Average score calculation
                var studentsWithRecords = allStudents.Where(s => s.AcademicRecords?.Any() == true).ToList();
                if (studentsWithRecords.Any())
                {
                    var allScores = studentsWithRecords
                        .SelectMany(s => s.AcademicRecords!)
                        .Where(ar => ar.TotalScore.HasValue)
                        .Select(ar => ar.TotalScore!.Value)
                        .ToList();
                    
                    viewModel.AverageScore = allScores.Any() ? allScores.Average() : 0;
                }
                else
                {
                    viewModel.AverageScore = 0;
                }

                // Recent enrollments
                var recentEnrollments = allEnrollments
                    .OrderByDescending(e => e.EnrollmentDate)
                    .Take(5)
                    .ToList();

                viewModel.RecentEnrollments = recentEnrollments.Select(e => new RecentEnrollmentViewModel
                {
                    EnrollmentId = e.EnrollmentId,
                    StudentName = e.Student?.FullName ?? "N/A",
                    CourseName = e.Course?.CourseName ?? "N/A",
                    EnrollmentDate = e.EnrollmentDate,
                    Status = e.Status.ToString()
                }).ToList();

                // Top courses by enrollment
                var activeCourses = allCourses.Where(c => c.IsActive).ToList();
                viewModel.TopCourses = activeCourses.Select(c => new TopCourseViewModel
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    EnrollmentCount = c.Enrollments?.Count(e => e.Status == EnrollmentStatus.Approved) ?? 0,
                    MaxStudents = c.MaxStudents,
                    EnrollmentRate = c.MaxStudents > 0 ? 
                        (decimal)(c.Enrollments?.Count(e => e.Status == EnrollmentStatus.Approved) ?? 0) / c.MaxStudents * 100 : 0
                })
                .OrderByDescending(c => c.EnrollmentCount)
                .Take(5)
                .ToList();

                // Student performance
                var activeStudents = allStudents.Where(s => s.IsActive).ToList();
                viewModel.StudentPerformance = activeStudents.Select(s => new StudentPerformanceViewModel
                {
                    StudentId = s.StudentId,
                    StudentName = s.FullName ?? "N/A",
                    StudentCode = s.StudentCode,
                    AverageScore = s.AcademicRecords?.Any() == true ? 
                        s.AcademicRecords.Where(ar => ar.TotalScore.HasValue).Average(ar => ar.TotalScore!.Value) : 0,
                    TotalCourses = s.Enrollments?.Count ?? 0,
                    CompletedCourses = s.AcademicRecords?.Count(ar => ar.TotalScore.HasValue) ?? 0
                })
                .OrderByDescending(s => s.AverageScore)
                .Take(10)
                .ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading dashboard");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dashboard.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var students = await _studentService.GetStudentsAsync(new StudentFilterOptions { PageSize = 10000 });
                var courses = await _courseService.GetCoursesAsync(new CourseFilterOptions { PageSize = 10000 });
                var enrollments = await _enrollmentService.GetEnrollmentsAsync(new EnrollmentFilterOptions { PageSize = 10000 });

                var statistics = new
                {
                    totalStudents = students.TotalCount,
                    activeStudents = students.Count(s => s.IsActive),
                    totalCourses = courses.TotalCount,
                    activeCourses = courses.Count(c => c.IsActive),
                    totalEnrollments = enrollments.TotalCount,
                    pendingEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Pending),
                    approvedEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Approved)
                };

                return Json(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting dashboard statistics");
                return Json(new { error = "Có lỗi xảy ra khi tải thống kê." });
            }
        }

        [HttpGet("enrollment-trends")]
        public async Task<IActionResult> GetEnrollmentTrends()
        {
            try
            {
                var enrollments = await _enrollmentService.GetEnrollmentsAsync(new EnrollmentFilterOptions { PageSize = 10000 });
                
                var trends = enrollments
                    .GroupBy(e => e.EnrollmentDate.Date)
                    .OrderBy(g => g.Key)
                    .Take(30) // Last 30 days
                    .Select(g => new
                    {
                        date = g.Key.ToString("yyyy-MM-dd"),
                        count = g.Count()
                    })
                    .ToList();

                return Json(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting enrollment trends");
                return Json(new { error = "Có lỗi xảy ra khi tải xu hướng đăng ký." });
            }
        }
    }
}