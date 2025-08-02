using Microsoft.EntityFrameworkCore;
using Web_SIMS.Data;
using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Repositories;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Repositories
{
    public class FacultyRepository : GenericRepository<Faculty>, IFacultyRepository
    {
        public FacultyRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<PaginatedList<Faculty>> GetFacultiesAsync(FacultyFilterOptions options)
        {
            var faculties = _context.Faculties.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(options.SearchString))
            {
                faculties = faculties.Where(f => f.FullName.Contains(options.SearchString)
                                           || f.FacultyCode.Contains(options.SearchString)
                                           || f.Email.Contains(options.SearchString)
                                           || (f.Department != null && f.Department.Contains(options.SearchString))
                                           || (f.Specialization != null && f.Specialization.Contains(options.SearchString)));
            }

            // Apply department filter
            if (!string.IsNullOrEmpty(options.DepartmentFilter))
            {
                faculties = faculties.Where(f => f.Department == options.DepartmentFilter);
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(options.StatusFilter))
            {
                if (options.StatusFilter == "active")
                {
                    faculties = faculties.Where(f => f.IsActive);
                }
                else if (options.StatusFilter == "inactive")
                {
                    faculties = faculties.Where(f => !f.IsActive);
                }
            }

            // Apply sorting
            faculties = ApplySorting(faculties, options.SortOrder);

            return await PaginatedList<Faculty>.CreateAsync(faculties.AsNoTracking(), options.PageNumber, options.PageSize);
        }

        public async Task<Faculty?> GetFacultyWithDetailsAsync(int facultyId)
        {
            return await _context.Faculties
                .Include(f => f.Courses)
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.FacultyId == facultyId);
        }

        public async Task<bool> IsFacultyCodeExistsAsync(string facultyCode, int? excludeFacultyId = null)
        {
            var query = _context.Faculties.Where(f => f.FacultyCode == facultyCode);
            
            if (excludeFacultyId.HasValue)
            {
                query = query.Where(f => f.FacultyId != excludeFacultyId.Value);
            }
            
            return await query.AnyAsync();
        }

        public async Task<bool> IsEmailExistsAsync(string email, int? excludeFacultyId = null)
        {
            var query = _context.Faculties.Where(f => f.Email == email);
            
            if (excludeFacultyId.HasValue)
            {
                query = query.Where(f => f.FacultyId != excludeFacultyId.Value);
            }
            
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Faculty>> GetFacultiesByDepartmentAsync(string department)
        {
            return await _context.Faculties
                .Where(f => f.Department == department && f.IsActive)
                .Include(f => f.User)
                .ToListAsync();
        }

        private static IQueryable<Faculty> ApplySorting(IQueryable<Faculty> faculties, string? sortOrder)
        {
            return sortOrder switch
            {
                "name_desc" => faculties.OrderByDescending(f => f.FullName),
                "Code" => faculties.OrderBy(f => f.FacultyCode),
                "code_desc" => faculties.OrderByDescending(f => f.FacultyCode),
                "Department" => faculties.OrderBy(f => f.Department),
                "department_desc" => faculties.OrderByDescending(f => f.Department),
                _ => faculties.OrderBy(f => f.FullName)
            };
        }
    }
}