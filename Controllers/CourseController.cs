using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Web_SIMS.Data;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Controllers
{
[Authorize(Roles = "Admin,Faculty")]
[Route("admin/courses")]
public class CourseController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CourseController> _logger;

        public CourseController(AppDbContext context, ILogger<CourseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /admin/courses
        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CodeSortParm"] = sortOrder == "Code" ? "code_desc" : "Code";
            ViewData["CurrentFilter"] = searchString;

            var courses = from c in _context.Courses
                          select c;

            if (!String.IsNullOrEmpty(searchString))
            {
                courses = courses.Where(c => c.CourseName.Contains(searchString)
                                       || c.CourseCode.Contains(searchString)
                                       || c.Instructor.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    courses = courses.OrderByDescending(c => c.CourseName);
                    break;
                case "Code":
                    courses = courses.OrderBy(c => c.CourseCode);
                    break;
                case "code_desc":
                    courses = courses.OrderByDescending(c => c.CourseCode);
                    break;
                default:
                    courses = courses.OrderBy(c => c.CourseName);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<Course>.CreateAsync(courses.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: /admin/courses/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(c => c.AcademicRecords)
                    .ThenInclude(ar => ar.Student)
                .FirstOrDefaultAsync(m => m.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: /admin/courses/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /admin/courses
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseCode,CourseName,Description,Credits,MaxStudents,Semester,AcademicYear,Instructor")] Course course)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra mã khóa học đã tồn tại
                if (await _context.Courses.AnyAsync(c => c.CourseCode == course.CourseCode))
                {
                    ModelState.AddModelError("CourseCode", "Mã khóa học đã tồn tại.");
                    return View(course);
                }

                course.IsActive = true;
                course.CreatedDate = DateTime.Now;

                _context.Add(course);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Khóa học mới được tạo: {course.CourseName} ({course.CourseCode})");
                TempData["SuccessMessage"] = "Khóa học đã được tạo thành công.";

                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: /admin/courses/edit/{id}
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // POST: /admin/courses/{id}
        [HttpPost("{id}")]
        [HttpPut("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseId,CourseCode,CourseName,Description,Credits,MaxStudents,Semester,AcademicYear,Instructor,IsActive")] Course course)
        {
            if (id != course.CourseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra mã khóa học đã tồn tại (trừ chính nó)
                    if (await _context.Courses.AnyAsync(c => c.CourseCode == course.CourseCode && c.CourseId != id))
                    {
                        ModelState.AddModelError("CourseCode", "Mã khóa học đã tồn tại.");
                        return View(course);
                    }

                    course.UpdatedDate = DateTime.Now;
                    _context.Update(course);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Khóa học được cập nhật: {course.CourseName} ({course.CourseCode})");
                    TempData["SuccessMessage"] = "Thông tin khóa học đã được cập nhật thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseId))
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

            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // DELETE: /admin/courses/{id}
        [HttpPost("delete/{id}")]
        [HttpDelete("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                // Kiểm tra xem có sinh viên nào đang đăng ký khóa học này không
                var hasEnrollments = await _context.Enrollments.AnyAsync(e => e.CourseId == id);
                if (hasEnrollments)
                {
                    TempData["ErrorMessage"] = "Không thể xóa khóa học vì đã có sinh viên đăng ký.";
                    return RedirectToAction(nameof(Index));
                }

                // Soft delete - chỉ đánh dấu không active
                course.IsActive = false;
                course.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Khóa học bị xóa: {course.CourseName} ({course.CourseCode})");
                TempData["SuccessMessage"] = "Khóa học đã được xóa thành công.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}