using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Web_SIMS.Data;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Controllers
{
    [Authorize(Roles = "Faculty")]
    public class FacultyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FacultyController> _logger;

        public FacultyController(AppDbContext context, ILogger<FacultyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Home()
        {
            var facultyName = User.FindFirst("FullName")?.Value ?? string.Empty;
            var courses = await _context.Courses
                .Where(c => c.Instructor == facultyName && c.IsActive)
                .ToListAsync();

            var courseIds = courses.Select(c => c.CourseId).ToList();
            var studentCount = await _context.Enrollments
                .CountAsync(e => courseIds.Contains(e.CourseId) && e.Status == EnrollmentStatus.Approved);

            var viewModel = new FacultyHomeViewModel
            {
                ClassCount = courses.Count,
                StudentCount = studentCount
            };
            return View(viewModel);
        }

        public async Task<IActionResult> MyClasses()
        {
            var facultyName = User.FindFirst("FullName")?.Value ?? string.Empty;
            var courses = await _context.Courses
                .Include(c => c.Enrollments)
                .Where(c => c.Instructor == facultyName && c.IsActive)
                .ToListAsync();
            return View(courses);
        }

        public async Task<IActionResult> ClassStudents(int classId)
        {
            var facultyName = User.FindFirst("FullName")?.Value ?? string.Empty;
            var course = await _context.Courses
                .Include(c => c.Enrollments.Where(e => e.Status == EnrollmentStatus.Approved))
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.CourseId == classId && c.Instructor == facultyName);
            if (course == null)
            {
                return NotFound();
            }

            var viewModel = new ClassStudentsViewModel
            {
                Course = course,
                Enrollments = course.Enrollments.ToList()
            };
            return View(viewModel);
        }

        public async Task<IActionResult> EnterGrades(int classId)
        {
            var facultyName = User.FindFirst("FullName")?.Value ?? string.Empty;
            var course = await _context.Courses
                .Include(c => c.Enrollments.Where(e => e.Status == EnrollmentStatus.Approved))
                    .ThenInclude(e => e.Student)
                .Include(c => c.AcademicRecords)
                .FirstOrDefaultAsync(c => c.CourseId == classId && c.Instructor == facultyName);
            if (course == null)
            {
                return NotFound();
            }

            var viewModel = new ClassGradesViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Students = course.Enrollments.Select(e =>
                {
                    var record = course.AcademicRecords?.FirstOrDefault(ar => ar.StudentId == e.StudentId);
                    return new StudentGradeViewModel
                {
                    StudentId = e.StudentId,
                        StudentName = e.Student?.FullName ?? string.Empty,
                        MidtermScore = record?.MidtermScore,
                        FinalScore = record?.FinalScore,
                        TotalScore = record?.TotalScore,
                        Grade = record?.Grade
                    };
                }).ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterGrades(int classId, ClassGradesViewModel model)
        {
            var facultyName = User.FindFirst("FullName")?.Value ?? string.Empty;
            var course = await _context.Courses
                .Include(c => c.AcademicRecords)
                .FirstOrDefaultAsync(c => c.CourseId == classId && c.Instructor == facultyName);
            if (course == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            foreach (var sg in model.Students)
            {
                var record = course.AcademicRecords.FirstOrDefault(r => r.StudentId == sg.StudentId);
                if (record == null)
                {
                    record = new AcademicRecord
                    {
                        CourseId = classId,
                        StudentId = sg.StudentId,
                        AcademicYear = DateTime.Now.Year,
                        Semester = 1,
                        CreatedDate = DateTime.Now
                    };
                    _context.AcademicRecords.Add(record);
                }
                record.MidtermScore = sg.MidtermScore;
                record.FinalScore = sg.FinalScore;
                if (sg.MidtermScore.HasValue && sg.FinalScore.HasValue)
                {
                    record.TotalScore = Math.Round((sg.MidtermScore.Value + sg.FinalScore.Value) / 2, 2);
                    record.Grade = GetGrade(record.TotalScore.Value);
                }
                record.UpdatedDate = DateTime.Now;
            }

                await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật điểm.";
            return RedirectToAction(nameof(EnterGrades), new { classId });
        }

        private string GetGrade(decimal score)
        {
            if (score >= 8) return "A";
            if (score >= 7) return "B";
            if (score >= 6) return "C";
            if (score >= 5) return "D";
            return "F";
        }
    }
}
