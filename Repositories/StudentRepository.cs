using Microsoft.EntityFrameworkCore;
using Web_SIMS.Data;
using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Repositories;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Repositories
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<PaginatedList<Student>> GetStudentsAsync(StudentFilterOptions options)
        {
            var students = _context.Students.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(options.SearchString))
            {
                students = students.Where(s => s.FullName.Contains(options.SearchString)
                                         || s.StudentCode.Contains(options.SearchString)
                                         || s.Email.Contains(options.SearchString)
                                         || (s.Major != null && s.Major.Contains(options.SearchString)));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(options.StatusFilter))
            {
                if (options.StatusFilter == "active")
                {
                    students = students.Where(s => s.IsActive);
                }
                else if (options.StatusFilter == "inactive")
                {
                    students = students.Where(s => !s.IsActive);
                }
            }

            // Apply sorting
            students = ApplySorting(students, options.SortOrder);

            return await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), options.PageNumber, options.PageSize);
        }

        public async Task<Student?> GetStudentWithDetailsAsync(int studentId)
        {
            return await _context.Students
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .Include(s => s.AcademicRecords)
                    .ThenInclude(ar => ar.Course)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public async Task<bool> IsStudentCodeExistsAsync(string studentCode, int? excludeStudentId = null)
        {
            var query = _context.Students.Where(s => s.StudentCode == studentCode);
            
            if (excludeStudentId.HasValue)
            {
                query = query.Where(s => s.StudentId != excludeStudentId.Value);
            }
            
            return await query.AnyAsync();
        }

        public async Task<bool> IsEmailExistsAsync(string email, int? excludeStudentId = null)
        {
            var query = _context.Students.Where(s => s.Email == email);
            
            if (excludeStudentId.HasValue)
            {
                query = query.Where(s => s.StudentId != excludeStudentId.Value);
            }
            
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByMajorAsync(string major)
        {
            return await _context.Students
                .Where(s => s.Major == major && s.IsActive)
                .Include(s => s.User)
                .ToListAsync();
        }

        private static IQueryable<Student> ApplySorting(IQueryable<Student> students, string? sortOrder)
        {
            return sortOrder switch
            {
                "name_desc" => students.OrderByDescending(s => s.FullName),
                "Code" => students.OrderBy(s => s.StudentCode),
                "code_desc" => students.OrderByDescending(s => s.StudentCode),
                _ => students.OrderBy(s => s.FullName)
            };
        }
    }
}