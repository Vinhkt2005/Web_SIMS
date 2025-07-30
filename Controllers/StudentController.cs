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
[Route("admin/students")]
public class StudentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StudentController> _logger;

        public StudentController(AppDbContext context, ILogger<StudentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /admin/students
        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CodeSortParm"] = sortOrder == "Code" ? "code_desc" : "Code";
            ViewData["CurrentFilter"] = searchString;

            var students = from s in _context.Students
                           select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.FullName.Contains(searchString)
                                         || s.StudentCode.Contains(searchString)
                                         || s.Email.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.FullName);
                    break;
                case "Code":
                    students = students.OrderBy(s => s.StudentCode);
                    break;
                case "code_desc":
                    students = students.OrderByDescending(s => s.StudentCode);
                    break;
                default:
                    students = students.OrderBy(s => s.FullName);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: /admin/students/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .Include(s => s.AcademicRecords)
                    .ThenInclude(ar => ar.Course)
                .FirstOrDefaultAsync(m => m.StudentId == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: /admin/students/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /admin/students
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentCode,FullName,Email,PhoneNumber,DateOfBirth,Address,Gender,Major,AcademicYear,Notes")] Student student)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra mã sinh viên đã tồn tại
                if (await _context.Students.AnyAsync(s => s.StudentCode == student.StudentCode))
                {
                    ModelState.AddModelError("StudentCode", "Mã sinh viên đã tồn tại.");
                    return View(student);
                }

                // Kiểm tra email đã tồn tại
                if (await _context.Students.AnyAsync(s => s.Email == student.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại.");
                    return View(student);
                }

                student.EnrollmentDate = DateTime.Now;
                student.IsActive = true;
                student.CreatedDate = DateTime.Now;

                _context.Add(student);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Sinh viên mới được tạo: {student.FullName} ({student.StudentCode})");
                TempData["SuccessMessage"] = "Sinh viên đã được tạo thành công.";

                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: /admin/students/edit/{id}
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: /admin/students/{id}
        [HttpPost("{id}")]
        [HttpPut("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StudentId,StudentCode,FullName,Email,PhoneNumber,DateOfBirth,Address,Gender,Major,AcademicYear,Notes,IsActive")] Student student)
        {
            if (id != student.StudentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra mã sinh viên đã tồn tại (trừ chính nó)
                    if (await _context.Students.AnyAsync(s => s.StudentCode == student.StudentCode && s.StudentId != id))
                    {
                        ModelState.AddModelError("StudentCode", "Mã sinh viên đã tồn tại.");
                        return View(student);
                    }

                    // Kiểm tra email đã tồn tại (trừ chính nó)
                    if (await _context.Students.AnyAsync(s => s.Email == student.Email && s.StudentId != id))
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại.");
                        return View(student);
                    }

                    student.UpdatedDate = DateTime.Now;
                    _context.Update(student);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Sinh viên được cập nhật: {student.FullName} ({student.StudentCode})");
                    TempData["SuccessMessage"] = "Thông tin sinh viên đã được cập nhật thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.StudentId))
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

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.StudentId == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // DELETE: /admin/students/{id}
        [HttpPost("delete/{id}")]
        [HttpDelete("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                // Soft delete - chỉ đánh dấu không active
                student.IsActive = false;
                student.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Sinh viên bị xóa: {student.FullName} ({student.StudentCode})");
                TempData["SuccessMessage"] = "Sinh viên đã được xóa thành công.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }
    }
}