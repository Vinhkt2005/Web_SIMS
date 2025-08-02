using Web_SIMS.Models;
using Web_SIMS.DTOs;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Interfaces.Services
{
    public interface IStudentService
    {
        Task<PaginatedList<Student>> GetStudentsAsync(StudentFilterOptions options);
        Task<Student?> GetStudentByIdAsync(int id);
        Task<Student?> GetStudentWithDetailsAsync(int studentId);
        Task<Student> CreateStudentAsync(Student student);
        Task<Student> CreateStudentAsync(Student student, string username, string password);
        Task<bool> UpdateStudentAsync(int id, Student student);
        Task<bool> DeleteStudentAsync(int id);
        Task<bool> IsStudentCodeExistsAsync(string studentCode, int? excludeStudentId = null);
        Task<bool> IsEmailExistsAsync(string email, int? excludeStudentId = null);
        Task<IEnumerable<Student>> GetStudentsByMajorAsync(string major);
    }
}