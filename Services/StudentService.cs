using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Repositories;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserService _userService;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ILogger<StudentService> _logger;

        public StudentService(IStudentRepository studentRepository, IUserService userService, IEnrollmentRepository enrollmentRepository, ILogger<StudentService> logger)
        {
            _studentRepository = studentRepository;
            _userService = userService;
            _enrollmentRepository = enrollmentRepository;
            _logger = logger;
        }

        public async Task<PaginatedList<Student>> GetStudentsAsync(StudentFilterOptions options)
        {
            try
            {
                return await _studentRepository.GetStudentsAsync(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting students with options: {@Options}", options);
                throw;
            }
        }

        public async Task<Student?> GetStudentByIdAsync(int id)
        {
            try
            {
                return await _studentRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting student by id: {StudentId}", id);
                throw;
            }
        }

        public async Task<Student?> GetStudentWithDetailsAsync(int studentId)
        {
            try
            {
                return await _studentRepository.GetStudentWithDetailsAsync(studentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting student with details: {StudentId}", studentId);
                throw;
            }
        }

        public async Task<Student> CreateStudentAsync(Student student)
        {
            // Use default username and password for backward compatibility
            return await CreateStudentAsync(student, student.StudentCode, "123456");
        }

        public async Task<Student> CreateStudentAsync(Student student, string username, string password)
        {
            try
            {
                // Validate business rules
                if (await _studentRepository.IsStudentCodeExistsAsync(student.StudentCode))
                {
                    throw new InvalidOperationException($"Student code '{student.StudentCode}' already exists.");
                }

                if (await _studentRepository.IsEmailExistsAsync(student.Email))
                {
                    throw new InvalidOperationException($"Email '{student.Email}' already exists in student records.");
                }

                // Also check if email exists in User table
                if (await _userService.IsEmailExistsAsync(student.Email))
                {
                    throw new InvalidOperationException($"Email '{student.Email}' already exists in user accounts.");
                }

                // Check if username exists in User table
                if (await _userService.IsUsernameExistsAsync(username))
                {
                    throw new InvalidOperationException($"Username '{username}' already exists.");
                }

                student.CreatedDate = DateTime.Now;
                var createdStudent = await _studentRepository.AddAsync(student);

                // Create User account for the student with provided credentials
                var user = new User
                {
                    Username = username,
                    Password = password, // Will be hashed by UserService
                    Email = student.Email,
                    FullName = student.FullName,
                    IsActive = student.IsActive,
                    RoleId = 3, // Student role ID
                    StudentId = createdStudent.StudentId,
                    CreatedDate = DateTime.Now
                };

                await _userService.CreateUserAsync(user);
                _logger.LogInformation("Created User account for Student: {StudentCode} with username: {Username}", student.StudentCode, username);

                return createdStudent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating student: {@Student}", student);
                throw;
            }
        }

        public async Task<bool> UpdateStudentAsync(int id, Student student)
        {
            try
            {
                var existingStudent = await _studentRepository.GetByIdAsync(id);
                if (existingStudent == null)
                {
                    return false;
                }

                // Validate business rules
                if (await _studentRepository.IsStudentCodeExistsAsync(student.StudentCode, id))
                {
                    throw new InvalidOperationException($"Student code '{student.StudentCode}' already exists.");
                }

                if (await _studentRepository.IsEmailExistsAsync(student.Email, id))
                {
                    throw new InvalidOperationException($"Email '{student.Email}' already exists in student records.");
                }

                // Check if email exists in User table (excluding current student's user if exists)
                var currentUser = await _userService.GetUserByStudentIdAsync(id);
                int? excludeUserId = currentUser?.UserId;
                
                if (await _userService.IsEmailExistsAsync(student.Email, excludeUserId))
                {
                    throw new InvalidOperationException($"Email '{student.Email}' already exists in user accounts.");
                }

                // Update properties
                existingStudent.StudentCode = student.StudentCode;
                existingStudent.FullName = student.FullName;
                existingStudent.Email = student.Email;
                existingStudent.PhoneNumber = student.PhoneNumber;
                existingStudent.Address = student.Address;
                existingStudent.DateOfBirth = student.DateOfBirth;
                existingStudent.Gender = student.Gender;
                existingStudent.Major = student.Major;
                existingStudent.AcademicYear = student.AcademicYear;
                existingStudent.IsActive = student.IsActive;
                existingStudent.Notes = student.Notes;
                existingStudent.UpdatedDate = DateTime.Now;

                await _studentRepository.UpdateAsync(existingStudent);

                // Update corresponding User account if exists
                if (currentUser != null)
                {
                    // Update user information to match student
                    var updatedUser = new User
                    {
                        UserId = currentUser.UserId,
                        Username = currentUser.Username, // Keep existing username
                        Password = currentUser.Password, // Keep existing password
                        PasswordSalt = currentUser.PasswordSalt, // Keep existing salt
                        Email = existingStudent.Email,
                        FullName = existingStudent.FullName,
                        IsActive = existingStudent.IsActive,
                        RoleId = currentUser.RoleId,
                        StudentId = currentUser.StudentId,
                        FacultyId = currentUser.FacultyId,
                        CreatedDate = currentUser.CreatedDate,
                        UpdatedDate = DateTime.Now
                    };

                    await _userService.UpdateUserAsync(currentUser.UserId, updatedUser);
                    _logger.LogInformation("Updated User account for Student: {StudentCode}", existingStudent.StudentCode);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating student: {StudentId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(id);
                if (student == null)
                {
                    return false;
                }

                // Check if student has enrollments
                var enrollments = await _enrollmentRepository.GetEnrollmentsByStudentAsync(id);
                if (enrollments.Any())
                {
                    var enrollmentCount = enrollments.Count();
                    var courseNames = enrollments.Take(3).Select(e => e.Course?.CourseName ?? "Unknown").ToList();
                    var courseList = string.Join(", ", courseNames);
                    if (enrollmentCount > 3)
                    {
                        courseList += $" và {enrollmentCount - 3} khóa học khác";
                    }
                    
                    throw new InvalidOperationException($"Không thể xóa sinh viên '{student.FullName}' vì đang có {enrollmentCount} đăng ký khóa học: {courseList}. Vui lòng hủy đăng ký trước khi xóa sinh viên.");
                }

                // Delete associated User account if exists
                var associatedUser = await _userService.GetUserByStudentIdAsync(id);
                if (associatedUser != null)
                {
                    await _userService.DeleteUserAsync(associatedUser.UserId);
                    _logger.LogInformation("Deleted associated User account for Student: {StudentId}", id);
                }

                await _studentRepository.DeleteAsync(student);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting student: {StudentId}", id);
                throw;
            }
        }

        public async Task<bool> IsStudentCodeExistsAsync(string studentCode, int? excludeStudentId = null)
        {
            return await _studentRepository.IsStudentCodeExistsAsync(studentCode, excludeStudentId);
        }

        public async Task<bool> IsEmailExistsAsync(string email, int? excludeStudentId = null)
        {
            return await _studentRepository.IsEmailExistsAsync(email, excludeStudentId);
        }

        public async Task<IEnumerable<Student>> GetStudentsByMajorAsync(string major)
        {
            try
            {
                return await _studentRepository.GetStudentsByMajorAsync(major);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting students by major: {Major}", major);
                throw;
            }
        }
    }
}