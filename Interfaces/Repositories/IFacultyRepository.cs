using Web_SIMS.Models;
using Web_SIMS.DTOs;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Interfaces.Repositories
{
    public interface IFacultyRepository : IGenericRepository<Faculty>
    {
        Task<PaginatedList<Faculty>> GetFacultiesAsync(FacultyFilterOptions options);
        Task<Faculty?> GetFacultyWithDetailsAsync(int facultyId);
        Task<bool> IsFacultyCodeExistsAsync(string facultyCode, int? excludeFacultyId = null);
        Task<bool> IsEmailExistsAsync(string email, int? excludeFacultyId = null);
        Task<IEnumerable<Faculty>> GetFacultiesByDepartmentAsync(string department);
    }
}