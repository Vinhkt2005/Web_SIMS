using Microsoft.EntityFrameworkCore;
using Web_SIMS.Data;
using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Repositories;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<PaginatedList<User>> GetUsersAsync(UserFilterOptions options)
        {
            var users = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Student)
                .Include(u => u.Faculty)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(options.SearchString))
            {
                users = users.Where(u => (u.FullName != null && u.FullName.Contains(options.SearchString))
                                   || u.Username.Contains(options.SearchString)
                                   || u.Email.Contains(options.SearchString));
            }

            // Apply role filter
            if (!string.IsNullOrEmpty(options.RoleFilter))
            {
                users = users.Where(u => u.Role != null && u.Role.RoleName == options.RoleFilter);
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(options.StatusFilter))
            {
                if (options.StatusFilter == "active")
                {
                    users = users.Where(u => u.IsActive);
                }
                else if (options.StatusFilter == "inactive")
                {
                    users = users.Where(u => !u.IsActive);
                }
            }

            // Apply sorting
            users = ApplySorting(users, options.SortOrder);

            return await PaginatedList<User>.CreateAsync(users.AsNoTracking(), options.PageNumber, options.PageSize);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Student)
                .Include(u => u.Faculty)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Student)
                .Include(u => u.Faculty)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> IsUsernameExistsAsync(string username, int? excludeUserId = null)
        {
            var query = _context.Users.Where(u => u.Username == username);
            
            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.UserId != excludeUserId.Value);
            }
            
            return await query.AnyAsync();
        }

        public async Task<bool> IsEmailExistsAsync(string email, int? excludeUserId = null)
        {
            var query = _context.Users.Where(u => u.Email == email);
            
            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.UserId != excludeUserId.Value);
            }
            
            return await query.AnyAsync();
        }

        public async Task<User?> GetUserByStudentIdAsync(int studentId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.StudentId == studentId);
        }

        public async Task<User?> GetUserByFacultyIdAsync(int facultyId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.FacultyId == facultyId);
        }

        private static IQueryable<User> ApplySorting(IQueryable<User> users, string? sortOrder)
        {
            return sortOrder switch
            {
                "name_desc" => users.OrderByDescending(u => u.FullName),
                "Username" => users.OrderBy(u => u.Username),
                "username_desc" => users.OrderByDescending(u => u.Username),
                _ => users.OrderBy(u => u.FullName)
            };
        }
    }
}