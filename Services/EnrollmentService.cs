using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Repositories;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ILogger<EnrollmentService> _logger;

        public EnrollmentService(
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            IStudentRepository studentRepository,
            ILogger<EnrollmentService> logger)
        {
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _studentRepository = studentRepository;
            _logger = logger;
        }

        public async Task<PaginatedList<Enrollment>> GetEnrollmentsAsync(EnrollmentFilterOptions options)
        {
            try
            {
                return await _enrollmentRepository.GetEnrollmentsAsync(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting enrollments with options: {@Options}", options);
                throw;
            }
        }

        public async Task<Enrollment?> GetEnrollmentByIdAsync(int id)
        {
            try
            {
                return await _enrollmentRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting enrollment by id: {EnrollmentId}", id);
                throw;
            }
        }

        public async Task<Enrollment?> GetEnrollmentWithDetailsAsync(int enrollmentId)
        {
            try
            {
                return await _enrollmentRepository.GetEnrollmentWithDetailsAsync(enrollmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting enrollment with details: {EnrollmentId}", enrollmentId);
                throw;
            }
        }

        public async Task<Enrollment> CreateEnrollmentAsync(Enrollment enrollment)
        {
            try
            {
                // Validate business rules
                if (await _enrollmentRepository.IsStudentEnrolledInCourseAsync(enrollment.StudentId, enrollment.CourseId))
                {
                    throw new InvalidOperationException("Student is already enrolled in this course.");
                }

                // Check if course has available slots
                var course = await _courseRepository.GetByIdAsync(enrollment.CourseId);
                if (course == null)
                {
                    throw new InvalidOperationException("Course not found.");
                }

                var currentEnrollmentCount = await _enrollmentRepository.GetEnrollmentCountByCourseAsync(enrollment.CourseId);
                if (currentEnrollmentCount >= course.MaxStudents)
                {
                    throw new InvalidOperationException("Course has reached maximum enrollment capacity.");
                }

                enrollment.EnrollmentDate = DateTime.Now;
                enrollment.Status = EnrollmentStatus.Pending;

                return await _enrollmentRepository.AddAsync(enrollment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating enrollment: {@Enrollment}", enrollment);
                throw;
            }
        }

        public async Task<bool> UpdateEnrollmentAsync(int id, Enrollment enrollment)
        {
            try
            {
                var existingEnrollment = await _enrollmentRepository.GetByIdAsync(id);
                if (existingEnrollment == null)
                {
                    return false;
                }

                // Update properties
                existingEnrollment.Notes = enrollment.Notes;
                existingEnrollment.Status = enrollment.Status;

                await _enrollmentRepository.UpdateAsync(existingEnrollment);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating enrollment: {EnrollmentId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteEnrollmentAsync(int id)
        {
            try
            {
                var enrollment = await _enrollmentRepository.GetByIdAsync(id);
                if (enrollment == null)
                {
                    return false;
                }

                await _enrollmentRepository.DeleteAsync(enrollment);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting enrollment: {EnrollmentId}", id);
                throw;
            }
        }

        public async Task<bool> IsStudentEnrolledInCourseAsync(int studentId, int courseId)
        {
            return await _enrollmentRepository.IsStudentEnrolledInCourseAsync(studentId, courseId);
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(int studentId)
        {
            try
            {
                return await _enrollmentRepository.GetEnrollmentsByStudentAsync(studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting enrollments by student: {StudentId}", studentId);
                throw;
            }
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseAsync(int courseId)
        {
            try
            {
                return await _enrollmentRepository.GetEnrollmentsByCourseAsync(courseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting enrollments by course: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<bool> CanEnrollStudentInCourseAsync(int studentId, int courseId)
        {
            try
            {
                // Check if student exists and is active
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null || !student.IsActive)
                {
                    return false;
                }

                // Check if course exists and is active
                var course = await _courseRepository.GetByIdAsync(courseId);
                if (course == null || !course.IsActive)
                {
                    return false;
                }

                // Check if student is already enrolled
                if (await _enrollmentRepository.IsStudentEnrolledInCourseAsync(studentId, courseId))
                {
                    return false;
                }

                // Check course capacity
                var currentEnrollmentCount = await _enrollmentRepository.GetEnrollmentCountByCourseAsync(courseId);
                if (currentEnrollmentCount >= course.MaxStudents)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking enrollment eligibility: StudentId={StudentId}, CourseId={CourseId}", studentId, courseId);
                return false;
            }
        }

        public async Task<Enrollment> EnrollStudentInCourseAsync(int studentId, int courseId, string? notes = null)
        {
            try
            {
                if (!await CanEnrollStudentInCourseAsync(studentId, courseId))
                {
                    throw new InvalidOperationException("Student cannot be enrolled in this course.");
                }

                var enrollment = new Enrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    EnrollmentDate = DateTime.Now,
                    Notes = notes,
                    Status = EnrollmentStatus.Pending
                };

                return await CreateEnrollmentAsync(enrollment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while enrolling student: StudentId={StudentId}, CourseId={CourseId}", studentId, courseId);
                throw;
            }
        }


    }
}