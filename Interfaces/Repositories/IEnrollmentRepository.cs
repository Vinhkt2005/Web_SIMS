using Web_SIMS.Models;
using Web_SIMS.DTOs;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Interfaces.Repositories
{
    public interface IEnrollmentRepository : IGenericRepository<Enrollment>
    {
        Task<PaginatedList<Enrollment>> GetEnrollmentsAsync(EnrollmentFilterOptions options);
        Task<Enrollment?> GetEnrollmentWithDetailsAsync(int enrollmentId);
        Task<bool> IsStudentEnrolledInCourseAsync(int studentId, int courseId);
        Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(int studentId);
        Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseAsync(int courseId);
        Task<int> GetEnrollmentCountByCourseAsync(int courseId);
    }
}