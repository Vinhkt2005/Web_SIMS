using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Web_SIMS.Data;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Controllers
{
[Authorize]
[Route("admin/dashboard")]
public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(AppDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = new DashboardViewModel();

                // Thống kê tổng quan
                viewModel.TotalStudents = await _context.Students.CountAsync(s => s.IsActive);
                viewModel.TotalCourses = await _context.Courses.CountAsync(c => c.IsActive);
                viewModel.TotalEnrollments = await _context.Enrollments.CountAsync();
                viewModel.TotalUsers = await _context.Users.CountAsync(u => u.IsActive);
                viewModel.PendingEnrollments = await _context.Enrollments.CountAsync(e => e.Status == EnrollmentStatus.Pending);
                viewModel.ActiveStudents = await _context.Students.CountAsync(s => s.IsActive);
                viewModel.ActiveCourses = await _context.Courses.CountAsync(c => c.IsActive);

                // Điểm trung bình
                var academicRecords = await _context.AcademicRecords
                    .Where(ar => ar.TotalScore.HasValue)
                    .ToListAsync();
                viewModel.AverageScore = academicRecords.Any() ? academicRecords.Average(ar => ar.TotalScore.Value) : 0;

                // Đăng ký gần đây
                viewModel.RecentEnrollments = await _context.Enrollments
                    .Include(e => e.Student)
                    .Include(e => e.Course)
                    .OrderByDescending(e => e.EnrollmentDate)
                    .Take(5)
                    .Select(e => new RecentEnrollmentViewModel
                    {
                        EnrollmentId = e.EnrollmentId,
                        StudentName = e.Student.FullName ?? "N/A",
                        CourseName = e.Course.CourseName,
                        EnrollmentDate = e.EnrollmentDate,
                        Status = e.Status.ToString()
                    })
                    .ToListAsync();

                // Khóa học phổ biến
                viewModel.TopCourses = await _context.Courses
                    .Include(c => c.Enrollments)
                    .Where(c => c.IsActive)
                    .Select(c => new TopCourseViewModel
                    {
                        CourseId = c.CourseId,
                        CourseName = c.CourseName,
                        EnrollmentCount = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Approved),
                        MaxStudents = c.MaxStudents,
                        EnrollmentRate = c.MaxStudents > 0 ? (decimal)c.Enrollments.Count(e => e.Status == EnrollmentStatus.Approved) / c.MaxStudents * 100 : 0
                    })
                    .OrderByDescending(c => c.EnrollmentCount)
                    .Take(5)
                    .ToListAsync();

                // Hiệu suất sinh viên
                viewModel.StudentPerformance = await _context.Students
                    .Include(s => s.AcademicRecords)
                    .Where(s => s.IsActive)
                    .Select(s => new StudentPerformanceViewModel
                    {
                        StudentId = s.StudentId,
                        StudentName = s.FullName ?? "N/A",
                        StudentCode = s.StudentCode,
                        AverageScore = s.AcademicRecords.Any() ? s.AcademicRecords.Average(ar => ar.TotalScore ?? 0) : 0,
                        TotalCourses = s.Enrollments.Count,
                        CompletedCourses = s.AcademicRecords.Count(ar => ar.TotalScore.HasValue)
                    })
                    .OrderByDescending(s => s.AverageScore)
                    .Take(10)
                    .ToListAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải dashboard");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dashboard.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}