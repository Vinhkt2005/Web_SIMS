using Web_SIMS.Models;
using Web_SIMS.DTOs;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Interfaces.Services
{
    public interface IFacultyService
    {
        Task<PaginatedList<Faculty>> GetFacultiesAsync(FacultyFilterOptions options);
        Task<Faculty?> GetFacultyByIdAsync(int id);
        Task<Faculty?> GetFacultyWithDetailsAsync(int facultyId);
        Task<Faculty> CreateFacultyAsync(Faculty faculty);
        Task<Faculty> CreateFacultyAsync(Faculty faculty, string username, string password);
        Task<bool> UpdateFacultyAsync(int id, Faculty faculty);
        Task<bool> DeleteFacultyAsync(int id);
        Task<bool> IsFacultyCodeExistsAsync(string facultyCode, int? excludeFacultyId = null);
        Task<bool> IsEmailExistsAsync(string email, int? excludeFacultyId = null);
        Task<IEnumerable<Faculty>> GetFacultiesByDepartmentAsync(string department);
    }
}