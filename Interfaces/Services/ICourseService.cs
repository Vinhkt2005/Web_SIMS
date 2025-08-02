using Web_SIMS.Models;
using Web_SIMS.DTOs;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Interfaces.Services
{
    public interface ICourseService
    {
        Task<PaginatedList<Course>> GetCoursesAsync(CourseFilterOptions options);
        Task<Course?> GetCourseByIdAsync(int id);
        Task<Course?> GetCourseWithDetailsAsync(int courseId);
        Task<Course> CreateCourseAsync(Course course);
        Task<bool> UpdateCourseAsync(int id, Course course);
        Task<bool> DeleteCourseAsync(int id);
        Task<bool> IsCourseCodeExistsAsync(string courseCode, int? excludeCourseId = null);
        Task<IEnumerable<Course>> GetCoursesByFacultyAsync(int facultyId);
    }
}