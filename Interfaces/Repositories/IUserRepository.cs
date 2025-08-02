using Web_SIMS.Models;
using Web_SIMS.DTOs;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<PaginatedList<User>> GetUsersAsync(UserFilterOptions options);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> IsUsernameExistsAsync(string username, int? excludeUserId = null);
        Task<bool> IsEmailExistsAsync(string email, int? excludeUserId = null);
        Task<User?> GetUserByStudentIdAsync(int studentId);
        Task<User?> GetUserByFacultyIdAsync(int facultyId);
    }
}