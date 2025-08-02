using Web_SIMS.Models;
using Web_SIMS.DTOs;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Interfaces.Repositories
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<PaginatedList<Course>> GetCoursesAsync(CourseFilterOptions options);
        Task<Course?> GetCourseWithDetailsAsync(int courseId);
        Task<bool> IsCourseCodeExistsAsync(string courseCode, int? excludeCourseId = null);
        Task<IEnumerable<Course>> GetCoursesByFacultyAsync(int facultyId);
    }
}