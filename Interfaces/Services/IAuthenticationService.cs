using System.Security.Claims;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Interfaces.Services
{
    public interface IAuthenticationService
    {
        Task<User?> ValidateUserAsync(string username, string password);
        Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(User user);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email, string newPassword);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> IsAccountLockedAsync(string username);
        Task LockAccountAsync(string username, TimeSpan lockDuration);
        Task UnlockAccountAsync(string username);
    }
}