using Microsoft.EntityFrameworkCore;
using Web_SIMS.Data;
using Web_SIMS.DTOs;
using Web_SIMS.Helpers;
using Web_SIMS.Interfaces.Repositories;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, AppDbContext context, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<PaginatedList<User>> GetUsersAsync(UserFilterOptions options)
        {
            try
            {
                return await _userRepository.GetUsersAsync(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting users with options: {@Options}", options);
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _userRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by id: {UserId}", id);
                throw;
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _userRepository.GetByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by username: {Username}", username);
                throw;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _userRepository.GetByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by email: {Email}", email);
                throw;
            }
        }

        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                // Validate business rules
                if (await _userRepository.IsUsernameExistsAsync(user.Username))
                {
                    throw new InvalidOperationException($"Username '{user.Username}' already exists.");
                }

                if (await _userRepository.IsEmailExistsAsync(user.Email))
                {
                    throw new InvalidOperationException($"Email '{user.Email}' already exists.");
                }

                // Hash password
                var (hashedPassword, salt) = PasswordHelper.HashPasswordWithSalt(user.Password);
                user.Password = hashedPassword;
                user.PasswordSalt = salt;
                user.CreatedDate = DateTime.Now;

                return await _userRepository.AddAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user: {@User}", user);
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(int id, User user)
        {
            try
            {
                var existingUser = await _userRepository.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return false;
                }

                // Validate business rules
                if (await _userRepository.IsUsernameExistsAsync(user.Username, id))
                {
                    throw new InvalidOperationException($"Username '{user.Username}' already exists.");
                }

                if (await _userRepository.IsEmailExistsAsync(user.Email, id))
                {
                    throw new InvalidOperationException($"Email '{user.Email}' already exists.");
                }

                // Update properties
                existingUser.Username = user.Username;
                existingUser.Email = user.Email;
                existingUser.FullName = user.FullName;
                existingUser.IsActive = user.IsActive;
                existingUser.RoleId = user.RoleId;
                existingUser.StudentId = user.StudentId;
                existingUser.FacultyId = user.FacultyId;
                existingUser.UpdatedDate = DateTime.Now;

                // Only update password if provided
                if (!string.IsNullOrEmpty(user.Password))
                {
                    var (hashedPassword, salt) = PasswordHelper.HashPasswordWithSalt(user.Password);
                    existingUser.Password = hashedPassword;
                    existingUser.PasswordSalt = salt;
                }

                await _userRepository.UpdateAsync(existingUser);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user: {UserId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                await _userRepository.DeleteAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user: {UserId}", id);
                throw;
            }
        }

        public async Task<bool> IsUsernameExistsAsync(string username, int? excludeUserId = null)
        {
            return await _userRepository.IsUsernameExistsAsync(username, excludeUserId);
        }

        public async Task<bool> IsEmailExistsAsync(string email, int? excludeUserId = null)
        {
            return await _userRepository.IsEmailExistsAsync(email, excludeUserId);
        }

        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || !user.IsActive)
                {
                    return false;
                }

                return PasswordHelper.VerifyPassword(password, user.Password, user.PasswordSalt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while validating user credentials for username: {Username}", username);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Verify current password
                if (!PasswordHelper.VerifyPassword(currentPassword, user.Password, user.PasswordSalt))
                {
                    return false;
                }

                // Update password
                var (hashedPassword, salt) = PasswordHelper.HashPasswordWithSalt(newPassword);
                user.Password = hashedPassword;
                user.PasswordSalt = salt;
                user.UpdatedDate = DateTime.Now;

                await _userRepository.UpdateAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing password for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<Role>> GetRolesAsync()
        {
            try
            {
                return await _context.Roles
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.RoleName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting roles");
                throw;
            }
        }

        public async Task<User?> GetUserByStudentIdAsync(int studentId)
        {
            try
            {
                return await _userRepository.GetUserByStudentIdAsync(studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by student ID: {StudentId}", studentId);
                throw;
            }
        }

        public async Task<User?> GetUserByFacultyIdAsync(int facultyId)
        {
            try
            {
                return await _userRepository.GetUserByFacultyIdAsync(facultyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by faculty ID: {FacultyId}", facultyId);
                throw;
            }
        }
    }
}