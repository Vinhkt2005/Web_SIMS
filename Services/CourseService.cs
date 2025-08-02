using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Repositories;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<CourseService> _logger;

        public CourseService(ICourseRepository courseRepository, ILogger<CourseService> logger)
        {
            _courseRepository = courseRepository;
            _logger = logger;
        }

        public async Task<PaginatedList<Course>> GetCoursesAsync(CourseFilterOptions options)
        {
            try
            {
                return await _courseRepository.GetCoursesAsync(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting courses with options: {@Options}", options);
                throw;
            }
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            try
            {
                return await _courseRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting course by id: {CourseId}", id);
                throw;
            }
        }

        public async Task<Course?> GetCourseWithDetailsAsync(int courseId)
        {
            try
            {
                return await _courseRepository.GetCourseWithDetailsAsync(courseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting course with details: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            try
            {
                // Validate business rules
                if (await _courseRepository.IsCourseCodeExistsAsync(course.CourseCode))
                {
                    throw new InvalidOperationException($"Course code '{course.CourseCode}' already exists.");
                }

                course.CreatedDate = DateTime.Now;
                return await _courseRepository.AddAsync(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating course: {@Course}", course);
                throw;
            }
        }

        public async Task<bool> UpdateCourseAsync(int id, Course course)
        {
            try
            {
                var existingCourse = await _courseRepository.GetByIdAsync(id);
                if (existingCourse == null)
                {
                    return false;
                }

                // Validate business rules
                if (await _courseRepository.IsCourseCodeExistsAsync(course.CourseCode, id))
                {
                    throw new InvalidOperationException($"Course code '{course.CourseCode}' already exists.");
                }

                // Update properties
                existingCourse.CourseCode = course.CourseCode;
                existingCourse.CourseName = course.CourseName;
                existingCourse.Description = course.Description;
                existingCourse.Credits = course.Credits;
                existingCourse.Semester = course.Semester;
                existingCourse.AcademicYear = course.AcademicYear;
                existingCourse.Instructor = course.Instructor;
                existingCourse.MaxStudents = course.MaxStudents;
                existingCourse.IsActive = course.IsActive;
                existingCourse.FacultyId = course.FacultyId;
                existingCourse.UpdatedDate = DateTime.Now;

                await _courseRepository.UpdateAsync(existingCourse);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating course: {CourseId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            try
            {
                var course = await _courseRepository.GetByIdAsync(id);
                if (course == null)
                {
                    return false;
                }

                await _courseRepository.DeleteAsync(course);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting course: {CourseId}", id);
                throw;
            }
        }

        public async Task<bool> IsCourseCodeExistsAsync(string courseCode, int? excludeCourseId = null)
        {
            return await _courseRepository.IsCourseCodeExistsAsync(courseCode, excludeCourseId);
        }

        public async Task<IEnumerable<Course>> GetCoursesByFacultyAsync(int facultyId)
        {
            try
            {
                return await _courseRepository.GetCoursesByFacultyAsync(facultyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting courses by faculty: {FacultyId}", facultyId);
                throw;
            }
        }


    }
}