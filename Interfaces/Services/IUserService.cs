using Web_SIMS.Models;
using Web_SIMS.DTOs;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Interfaces.Services
{
    public interface IUserService
    {
        Task<PaginatedList<User>> GetUsersAsync(UserFilterOptions options);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(int id, User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> IsUsernameExistsAsync(string username, int? excludeUserId = null);
        Task<bool> IsEmailExistsAsync(string email, int? excludeUserId = null);
        Task<bool> ValidateUserCredentialsAsync(string username, string password);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<IEnumerable<Role>> GetRolesAsync();
        Task<User?> GetUserByStudentIdAsync(int studentId);
        Task<User?> GetUserByFacultyIdAsync(int facultyId);
    }
}