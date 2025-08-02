using Web_SIMS.Models;
using Web_SIMS.DTOs;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Interfaces.Services
{
    public interface IEnrollmentService
    {
        Task<PaginatedList<Enrollment>> GetEnrollmentsAsync(EnrollmentFilterOptions options);
        Task<Enrollment?> GetEnrollmentByIdAsync(int id);
        Task<Enrollment?> GetEnrollmentWithDetailsAsync(int enrollmentId);
        Task<Enrollment> CreateEnrollmentAsync(Enrollment enrollment);
        Task<bool> UpdateEnrollmentAsync(int id, Enrollment enrollment);
        Task<bool> DeleteEnrollmentAsync(int id);
        Task<bool> IsStudentEnrolledInCourseAsync(int studentId, int courseId);
        Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(int studentId);
        Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseAsync(int courseId);
        Task<bool> CanEnrollStudentInCourseAsync(int studentId, int courseId);
        Task<Enrollment> EnrollStudentInCourseAsync(int studentId, int courseId, string? notes = null);
    }
}