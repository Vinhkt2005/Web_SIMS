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
[Route("admin/enrollments")]
public class EnrollmentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EnrollmentController> _logger;

        public EnrollmentController(AppDbContext context, ILogger<EnrollmentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /admin/enrollments
        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, string statusFilter, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;

            var enrollments = from e in _context.Enrollments
                             .Include(e => e.Student)
                             .Include(e => e.Course)
                              select e;

            if (!String.IsNullOrEmpty(searchString))
            {
                enrollments = enrollments.Where(e => (e.Student != null && e.Student.FullName.Contains(searchString))
                                               || (e.Course != null && e.Course.CourseName.Contains(searchString))
                                               || (e.Student != null && e.Student.StudentCode.Contains(searchString)));
            }

            if (!String.IsNullOrEmpty(statusFilter))
            {
                if (Enum.TryParse<EnrollmentStatus>(statusFilter, out var status))
                {
                    enrollments = enrollments.Where(e => e.Status == status);
                }
            }

            enrollments = enrollments.OrderByDescending(e => e.EnrollmentDate);

            int pageSize = 10;
            return View(await PaginatedList<Enrollment>.CreateAsync(enrollments.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: /admin/enrollments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(m => m.EnrollmentId == id);

            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // GET: /admin/enrollments/create
        [Authorize(Roles = "Admin,Faculty")]
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            ViewData["StudentId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Students.Where(s => s.IsActive), "StudentId", "FullName");
            ViewData["CourseId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Courses.Where(c => c.IsActive), "CourseId", "CourseName");
            return View();
        }

        // POST: /admin/enrollments
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Faculty")]
        public async Task<IActionResult> Create([Bind("StudentId,CourseId,Notes")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra sinh viên đã đăng ký khóa học này chưa
                var existingEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.StudentId == enrollment.StudentId && e.CourseId == enrollment.CourseId);

                if (existingEnrollment != null)
                {
                    ModelState.AddModelError(string.Empty, "Sinh viên đã đăng ký khóa học này.");
                    ViewData["StudentId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Students.Where(s => s.IsActive), "StudentId", "FullName");
                    ViewData["CourseId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Courses.Where(c => c.IsActive), "CourseId", "CourseName");
                    return View(enrollment);
                }

                // Kiểm tra số lượng sinh viên đã đăng ký khóa học
                var course = await _context.Courses.FindAsync(enrollment.CourseId);
                var currentEnrollments = await _context.Enrollments
                    .CountAsync(e => e.CourseId == enrollment.CourseId && e.Status == EnrollmentStatus.Approved);

                if (course != null && currentEnrollments >= course.MaxStudents)
                {
                    ModelState.AddModelError(string.Empty, "Khóa học đã đạt số lượng sinh viên tối đa.");
                    ViewData["StudentId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Students.Where(s => s.IsActive), "StudentId", "FullName");
                    ViewData["CourseId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Courses.Where(c => c.IsActive), "CourseId", "CourseName");
                    return View(enrollment);
                }

                enrollment.EnrollmentDate = DateTime.Now;
                enrollment.Status = EnrollmentStatus.Pending;

                _context.Add(enrollment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Đăng ký mới được tạo: Sinh viên {enrollment.StudentId} - Khóa học {enrollment.CourseId}");
                TempData["SuccessMessage"] = "Đăng ký khóa học đã được tạo thành công.";

                return RedirectToAction(nameof(Index));
            }
            ViewData["StudentId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Students.Where(s => s.IsActive), "StudentId", "FullName");
            ViewData["CourseId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Courses.Where(c => c.IsActive), "CourseId", "CourseName");
            return View(enrollment);
        }

        // GET: /admin/enrollments/edit/{id}
        [Authorize(Roles = "Admin,Faculty")]
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(m => m.EnrollmentId == id);

            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // POST: /admin/enrollments/{id}
        [HttpPost("{id}")]
        [HttpPut("{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Faculty")]
        public async Task<IActionResult> Edit(int id, [Bind("EnrollmentId,StudentId,CourseId,EnrollmentDate,Status,Notes")] Enrollment enrollment)
        {
            if (id != enrollment.EnrollmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Đăng ký được cập nhật: {enrollment.EnrollmentId}");
                    TempData["SuccessMessage"] = "Thông tin đăng ký đã được cập nhật thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.EnrollmentId))
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
            return View(enrollment);
        }

        // POST: /admin/enrollments/approve/{id}
        [HttpPost("approve/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Faculty")]
        public async Task<IActionResult> Approve(int id)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrollment == null)
            {
                return NotFound();
            }

            // Kiểm tra số lượng sinh viên đã đăng ký khóa học
            var currentEnrollments = await _context.Enrollments
                .CountAsync(e => e.CourseId == enrollment.CourseId && e.Status == EnrollmentStatus.Approved);

            if (enrollment.Course != null && currentEnrollments >= enrollment.Course.MaxStudents)
            {
                TempData["ErrorMessage"] = "Khóa học đã đạt số lượng sinh viên tối đa.";
                return RedirectToAction(nameof(Index));
            }

            enrollment.Status = EnrollmentStatus.Approved;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Đăng ký được duyệt: {enrollment.EnrollmentId}");
            TempData["SuccessMessage"] = "Đăng ký đã được duyệt thành công.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /admin/enrollments/reject/{id}
        [HttpPost("reject/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Faculty")]
        public async Task<IActionResult> Reject(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }

            enrollment.Status = EnrollmentStatus.Rejected;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Đăng ký bị từ chối: {enrollment.EnrollmentId}");
            TempData["SuccessMessage"] = "Đăng ký đã bị từ chối.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /admin/enrollments/delete/{id}
        [Authorize(Roles = "Admin,Faculty")]
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(m => m.EnrollmentId == id);

            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // DELETE: /admin/enrollments/{id}
        [HttpPost("delete/{id}")]
        [HttpDelete("{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Faculty")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Đăng ký bị xóa: {enrollment.EnrollmentId}");
                TempData["SuccessMessage"] = "Đăng ký đã được xóa thành công.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EnrollmentExists(int id)
        {
            return _context.Enrollments.Any(e => e.EnrollmentId == id);
        }
    }
}