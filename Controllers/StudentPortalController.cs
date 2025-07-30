using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Web_SIMS.Data;
using Web_SIMS.Models;

namespace Web_SIMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentPortalController : Controller
    {
        private readonly AppDbContext _context;

        public StudentPortalController(AppDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentStudentId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                var user = _context.Users.AsNoTracking().FirstOrDefault(u => u.UserId == userId);
                return user?.StudentId;
            }
            return null;
        }

        public async Task<IActionResult> Home()
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin sinh viên.";
                return RedirectToAction("Index", "Home");
            }

            var student = await _context.Students
                .Include(s => s.Enrollments!)
                    .ThenInclude(e => e.Course)
                .Include(s => s.AcademicRecords!)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin sinh viên.";
                return RedirectToAction("Index", "Home");
            }

            var activeEnrollments = student.Enrollments?.Where(e => e.Status == EnrollmentStatus.Approved).ToList() ?? new List<Enrollment>();
            var completedRecords = student.AcademicRecords?.Where(ar => ar.TotalScore.HasValue).ToList() ?? new List<AcademicRecord>();
            var gpa = completedRecords.Any() ? completedRecords.Average(ar => ar.TotalScore!.Value) : 0m;
            var totalCredits = activeEnrollments.Sum(e => e.Course?.Credits ?? 0);

            var model = new ViewModels.StudentHomeViewModel
            {
                CurrentCourses = activeEnrollments.Count,
                CurrentGpa = gpa,
                TotalCredits = totalCredits,
                CompletedCourses = completedRecords.Count
            };

            return View(model);
        }

        public async Task<IActionResult> Courses()
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin sinh viên.";
                return RedirectToAction("Index", "Home");
            }

            var courses = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Approved)
                .Include(e => e.Course)
                .Select(e => e.Course)
                .ToListAsync();

            return View(courses);
        }

        public async Task<IActionResult> CourseDetails(int id)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        public async Task<IActionResult> Profile()
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin sinh viên.";
                return RedirectToAction("Index", "Home");
            }

            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }
    }
}
