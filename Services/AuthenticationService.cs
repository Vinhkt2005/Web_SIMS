using System.Security.Claims;
using Web_SIMS.Helpers;
using Web_SIMS.Interfaces.Repositories;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly Dictionary<string, DateTime> _lockedAccounts = new();

        public AuthenticationService(IUserRepository userRepository, ILogger<AuthenticationService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            try
            {
                // Check if account is locked
                if (await IsAccountLockedAsync(username))
                {
                    _logger.LogWarning("Login attempt for locked account: {Username}", username);
                    return null;
                }

                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Login attempt for non-existent or inactive user: {Username}", username);
                    return null;
                }

                // Debug logging
                _logger.LogInformation("Debug - User found: {Username}, IsActive: {IsActive}, HasPassword: {HasPassword}, HasSalt: {HasSalt}", 
                    user.Username, user.IsActive, !string.IsNullOrEmpty(user.Password), !string.IsNullOrEmpty(user.PasswordSalt));

                // Verify password
                bool passwordValid = false;
                
                // Try new method with salt first (for users created after SOLID refactor)
                if (!string.IsNullOrEmpty(user.PasswordSalt))
                {
                    passwordValid = PasswordHelper.VerifyPassword(password, user.Password, user.PasswordSalt);
                    _logger.LogInformation("Debug - New password verification with salt result: {IsValid}", passwordValid);
                }
                else
                {
                    // Fallback: Try old methods for users created before SOLID refactor
                    _logger.LogInformation("Debug - User has no salt, trying fallback methods");
                    
                    // Method 1: Try hashing with empty salt
                    var hashedInputPassword = PasswordHelper.HashPassword(password, "");
                    passwordValid = hashedInputPassword == user.Password;
                    _logger.LogInformation("Debug - Old password verification (hashed with empty salt) result: {IsValid}", passwordValid);
                    
                    // Method 2: Try direct comparison (for very old accounts with plain text passwords)
                    if (!passwordValid)
                    {
                        passwordValid = password == user.Password;
                        _logger.LogInformation("Debug - Direct password comparison result: {IsValid}", passwordValid);
                        
                        // If direct comparison works, update the user with proper hash and salt
                        if (passwordValid)
                        {
                            _logger.LogInformation("Debug - Updating user {Username} with proper password hash and salt", user.Username);
                            var (newHashedPassword, newSalt) = PasswordHelper.HashPasswordWithSalt(password);
                            user.Password = newHashedPassword;
                            user.PasswordSalt = newSalt;
                            user.UpdatedDate = DateTime.Now;
                            
                            // Save the updated user with proper hash and salt
                            try
                            {
                                await _userRepository.UpdateAsync(user);
                                _logger.LogInformation("Successfully updated password hash for user: {Username}", user.Username);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to update password hash for user: {Username}", user.Username);
                                // Continue with login even if update fails
                            }
                        }
                    }
                }
                
                if (!passwordValid)
                {
                    _logger.LogWarning("Invalid password attempt for user: {Username}", username);
                    return null;
                }

                _logger.LogInformation("Successful login for user: {Username}", username);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user validation for username: {Username}", username);
                return null;
            }
        }

        public Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(User user)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new(ClaimTypes.Name, user.Username),
                    new(ClaimTypes.Email, user.Email)
                };

                // Add full name if available
                if (!string.IsNullOrEmpty(user.FullName))
                {
                    claims.Add(new Claim("FullName", user.FullName));
                }

                // Add role claim
                if (user.Role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleName));
                }

                // Add student/faculty specific claims
                if (user.StudentId.HasValue)
                {
                    claims.Add(new Claim("StudentId", user.StudentId.Value.ToString()));
                }

                if (user.FacultyId.HasValue)
                {
                    claims.Add(new Claim("FacultyId", user.FacultyId.Value.ToString()));
                }

                var identity = new ClaimsIdentity(claims, "Cookie");
                return Task.FromResult(new ClaimsPrincipal(identity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating claims principal for user: {UserId}", user.UserId);
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
                    _logger.LogWarning("Invalid current password for password change attempt: UserId={UserId}", userId);
                    return false;
                }

                // Update password
                var (hashedPassword, salt) = PasswordHelper.HashPasswordWithSalt(newPassword);
                user.Password = hashedPassword;
                user.PasswordSalt = salt;
                user.UpdatedDate = DateTime.Now;

                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing password for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    return false;
                }

                // Update password
                var (hashedPassword, salt) = PasswordHelper.HashPasswordWithSalt(newPassword);
                user.Password = hashedPassword;
                user.PasswordSalt = salt;
                user.UpdatedDate = DateTime.Now;

                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Password reset successfully for user: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting password for email: {Email}", email);
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

        public async Task<bool> IsAccountLockedAsync(string username)
        {
            await Task.CompletedTask; // For async consistency
            
            if (_lockedAccounts.TryGetValue(username, out var lockExpiry))
            {
                if (DateTime.Now < lockExpiry)
                {
                    return true;
                }
                else
                {
                    // Lock has expired, remove it
                    _lockedAccounts.Remove(username);
                }
            }
            
            return false;
        }

        public async Task LockAccountAsync(string username, TimeSpan lockDuration)
        {
            await Task.CompletedTask; // For async consistency
            
            var lockExpiry = DateTime.Now.Add(lockDuration);
            _lockedAccounts[username] = lockExpiry;
            
            _logger.LogWarning("Account locked for user: {Username} until {LockExpiry}", username, lockExpiry);
        }

        public async Task UnlockAccountAsync(string username)
        {
            await Task.CompletedTask; // For async consistency
            
            if (_lockedAccounts.Remove(username))
            {
                _logger.LogInformation("Account unlocked for user: {Username}", username);
            }
        }
    }
}