using Web_SIMS.Models;
using Web_SIMS.DTOs;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Interfaces.Repositories
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
        Task<PaginatedList<Student>> GetStudentsAsync(StudentFilterOptions options);
        Task<Student?> GetStudentWithDetailsAsync(int studentId);
        Task<bool> IsStudentCodeExistsAsync(string studentCode, int? excludeStudentId = null);
        Task<bool> IsEmailExistsAsync(string email, int? excludeStudentId = null);
        Task<IEnumerable<Student>> GetStudentsByMajorAsync(string major);
    }
}