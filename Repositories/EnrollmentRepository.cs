using Microsoft.EntityFrameworkCore;
using Web_SIMS.Data;
using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Repositories;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Repositories
{
    public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
    {

        public EnrollmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<PaginatedList<Enrollment>> GetEnrollmentsAsync(EnrollmentFilterOptions options)
        {
            var enrollments = _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Faculty)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(options.SearchString))
            {
                enrollments = enrollments.Where(e => 
                    (e.Student != null && e.Student.FullName.Contains(options.SearchString)) ||
                    (e.Student != null && e.Student.StudentCode.Contains(options.SearchString)) ||
                    (e.Course != null && e.Course.CourseName.Contains(options.SearchString)) ||
                    (e.Course != null && e.Course.CourseCode.Contains(options.SearchString)));
            }

            // Apply course filter
            if (!string.IsNullOrEmpty(options.CourseFilter))
            {
                if (int.TryParse(options.CourseFilter, out int courseId))
                {
                    enrollments = enrollments.Where(e => e.CourseId == courseId);
                }
            }

            // Apply student filter
            if (!string.IsNullOrEmpty(options.StudentFilter))
            {
                if (int.TryParse(options.StudentFilter, out int studentId))
                {
                    enrollments = enrollments.Where(e => e.StudentId == studentId);
                }
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(options.StatusFilter))
            {
                if (Enum.TryParse<EnrollmentStatus>(options.StatusFilter, out var status))
                {
                    enrollments = enrollments.Where(e => e.Status == status);
                }
            }

            // Apply sorting
            enrollments = options.SortOrder switch
            {
                "name_desc" => enrollments.OrderByDescending(e => e.Student != null ? e.Student.FullName : ""),
                "course" => enrollments.OrderBy(e => e.Course != null ? e.Course.CourseName : ""),
                "course_desc" => enrollments.OrderByDescending(e => e.Course != null ? e.Course.CourseName : ""),
                "status" => enrollments.OrderBy(e => e.Status),
                "status_desc" => enrollments.OrderByDescending(e => e.Status),
                _ => enrollments.OrderBy(e => e.Student != null ? e.Student.FullName : ""),
            };

            return await PaginatedList<Enrollment>.CreateAsync(enrollments, options.PageNumber, options.PageSize);
        }

        public async Task<Enrollment?> GetEnrollmentByIdAsync(int enrollmentId)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Faculty)
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
        }

        public async Task<Enrollment?> GetEnrollmentWithDetailsAsync(int enrollmentId)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Faculty)
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(int studentId)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Faculty)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseAsync(int courseId)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == courseId)
                .ToListAsync();
        }



        public async Task<bool> IsStudentEnrolledInCourseAsync(int studentId, int courseId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
        }

        public async Task<int> GetEnrollmentCountByCourseAsync(int courseId)
        {
            return await _context.Enrollments
                .CountAsync(e => e.CourseId == courseId && e.Status != EnrollmentStatus.Cancelled);
        }
    }
}