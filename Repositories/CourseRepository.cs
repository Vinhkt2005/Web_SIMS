using Microsoft.EntityFrameworkCore;
using Web_SIMS.Data;
using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Repositories;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Repositories
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<PaginatedList<Course>> GetCoursesAsync(CourseFilterOptions options)
        {
            var courses = _context.Courses
                .Include(c => c.Faculty)
                .Include(c => c.Enrollments)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(options.SearchString))
            {
                courses = courses.Where(c => c.CourseName.Contains(options.SearchString)
                                       || c.CourseCode.Contains(options.SearchString)
                                       || c.Instructor.Contains(options.SearchString)
                                       || (c.Faculty != null && c.Faculty.FullName.Contains(options.SearchString)));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(options.StatusFilter))
            {
                if (options.StatusFilter == "active")
                {
                    courses = courses.Where(c => c.IsActive);
                }
                else if (options.StatusFilter == "inactive")
                {
                    courses = courses.Where(c => !c.IsActive);
                }
            }

            // Apply sorting
            courses = ApplySorting(courses, options.SortOrder);

            return await PaginatedList<Course>.CreateAsync(courses.AsNoTracking(), options.PageNumber, options.PageSize);
        }

        public async Task<Course?> GetCourseWithDetailsAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(c => c.AcademicRecords)
                    .ThenInclude(ar => ar.Student)
                .Include(c => c.Faculty)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<bool> IsCourseCodeExistsAsync(string courseCode, int? excludeCourseId = null)
        {
            var query = _context.Courses.Where(c => c.CourseCode == courseCode);
            
            if (excludeCourseId.HasValue)
            {
                query = query.Where(c => c.CourseId != excludeCourseId.Value);
            }
            
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByFacultyAsync(int facultyId)
        {
            return await _context.Courses
                .Where(c => c.FacultyId == facultyId)
                .Include(c => c.Enrollments)
                .ToListAsync();
        }

        private static IQueryable<Course> ApplySorting(IQueryable<Course> courses, string? sortOrder)
        {
            return sortOrder switch
            {
                "name_desc" => courses.OrderByDescending(c => c.CourseName),
                "Code" => courses.OrderBy(c => c.CourseCode),
                "code_desc" => courses.OrderByDescending(c => c.CourseCode),
                _ => courses.OrderBy(c => c.CourseName)
            };
        }
    }
}