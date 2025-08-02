using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Repositories;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Services
{
    public class FacultyService : IFacultyService
    {
        private readonly IFacultyRepository _facultyRepository;
        private readonly IUserService _userService;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<FacultyService> _logger;

        public FacultyService(IFacultyRepository facultyRepository, IUserService userService, ICourseRepository courseRepository, ILogger<FacultyService> logger)
        {
            _facultyRepository = facultyRepository;
            _userService = userService;
            _courseRepository = courseRepository;
            _logger = logger;
        }

        public async Task<PaginatedList<Faculty>> GetFacultiesAsync(FacultyFilterOptions options)
        {
            try
            {
                return await _facultyRepository.GetFacultiesAsync(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting faculties with options: {@Options}", options);
                throw;
            }
        }

        public async Task<Faculty?> GetFacultyByIdAsync(int id)
        {
            try
            {
                return await _facultyRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting faculty by id: {FacultyId}", id);
                throw;
            }
        }

        public async Task<Faculty?> GetFacultyWithDetailsAsync(int facultyId)
        {
            try
            {
                return await _facultyRepository.GetFacultyWithDetailsAsync(facultyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting faculty with details: {FacultyId}", facultyId);
                throw;
            }
        }

        public async Task<Faculty> CreateFacultyAsync(Faculty faculty)
        {
            // Use default username and password for backward compatibility
            return await CreateFacultyAsync(faculty, faculty.FacultyCode, "123456");
        }

        public async Task<Faculty> CreateFacultyAsync(Faculty faculty, string username, string password)
        {
            try
            {
                // Validate business rules
                if (await _facultyRepository.IsFacultyCodeExistsAsync(faculty.FacultyCode))
                {
                    throw new InvalidOperationException($"Faculty code '{faculty.FacultyCode}' already exists.");
                }

                if (await _facultyRepository.IsEmailExistsAsync(faculty.Email))
                {
                    throw new InvalidOperationException($"Email '{faculty.Email}' already exists in faculty records.");
                }

                // Also check if email exists in User table
                if (await _userService.IsEmailExistsAsync(faculty.Email))
                {
                    throw new InvalidOperationException($"Email '{faculty.Email}' already exists in user accounts.");
                }

                // Check if username exists in User table
                if (await _userService.IsUsernameExistsAsync(username))
                {
                    throw new InvalidOperationException($"Username '{username}' already exists.");
                }

                faculty.CreatedDate = DateTime.Now;
                var createdFaculty = await _facultyRepository.AddAsync(faculty);

                // Create User account for the faculty with provided credentials
                var user = new User
                {
                    Username = username,
                    Password = password, // Will be hashed by UserService
                    Email = faculty.Email,
                    FullName = faculty.FullName,
                    IsActive = faculty.IsActive,
                    RoleId = 2, // Faculty role ID
                    FacultyId = createdFaculty.FacultyId,
                    CreatedDate = DateTime.Now
                };

                await _userService.CreateUserAsync(user);
                _logger.LogInformation("Created User account for Faculty: {FacultyCode} with username: {Username}", faculty.FacultyCode, username);

                return createdFaculty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating faculty: {@Faculty}", faculty);
                throw;
            }
        }

        public async Task<bool> UpdateFacultyAsync(int id, Faculty faculty)
        {
            try
            {
                var existingFaculty = await _facultyRepository.GetByIdAsync(id);
                if (existingFaculty == null)
                {
                    return false;
                }

                // Validate business rules
                if (await _facultyRepository.IsFacultyCodeExistsAsync(faculty.FacultyCode, id))
                {
                    throw new InvalidOperationException($"Faculty code '{faculty.FacultyCode}' already exists.");
                }

                if (await _facultyRepository.IsEmailExistsAsync(faculty.Email, id))
                {
                    throw new InvalidOperationException($"Email '{faculty.Email}' already exists in faculty records.");
                }

                // Check if email exists in User table (excluding current faculty's user if exists)
                var currentUser = await _userService.GetUserByFacultyIdAsync(id);
                int? excludeUserId = currentUser?.UserId;
                
                if (await _userService.IsEmailExistsAsync(faculty.Email, excludeUserId))
                {
                    throw new InvalidOperationException($"Email '{faculty.Email}' already exists in user accounts.");
                }

                // Update properties
                existingFaculty.FacultyCode = faculty.FacultyCode;
                existingFaculty.FullName = faculty.FullName;
                existingFaculty.Email = faculty.Email;
                existingFaculty.PhoneNumber = faculty.PhoneNumber;
                existingFaculty.Address = faculty.Address;
                existingFaculty.DateOfBirth = faculty.DateOfBirth;
                existingFaculty.Gender = faculty.Gender;
                existingFaculty.Department = faculty.Department;
                existingFaculty.Specialization = faculty.Specialization;
                existingFaculty.IsActive = faculty.IsActive;
                existingFaculty.UpdatedDate = DateTime.Now;

                await _facultyRepository.UpdateAsync(existingFaculty);

                // Update corresponding User account if exists
                if (currentUser != null)
                {
                    // Update user information to match faculty
                    var updatedUser = new User
                    {
                        UserId = currentUser.UserId,
                        Username = currentUser.Username, // Keep existing username
                        Password = currentUser.Password, // Keep existing password
                        PasswordSalt = currentUser.PasswordSalt, // Keep existing salt
                        Email = existingFaculty.Email,
                        FullName = existingFaculty.FullName,
                        IsActive = existingFaculty.IsActive,
                        RoleId = currentUser.RoleId,
                        StudentId = currentUser.StudentId,
                        FacultyId = currentUser.FacultyId,
                        CreatedDate = currentUser.CreatedDate,
                        UpdatedDate = DateTime.Now
                    };

                    await _userService.UpdateUserAsync(currentUser.UserId, updatedUser);
                    _logger.LogInformation("Updated User account for Faculty: {FacultyCode}", existingFaculty.FacultyCode);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating faculty: {FacultyId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteFacultyAsync(int id)
        {
            try
            {
                var faculty = await _facultyRepository.GetByIdAsync(id);
                if (faculty == null)
                {
                    return false;
                }

                // Check if faculty has courses
                var courses = await _courseRepository.GetCoursesByFacultyAsync(id);
                if (courses.Any())
                {
                    throw new InvalidOperationException("Cannot delete faculty with existing courses. Please reassign or remove courses first.");
                }

                // Delete associated User account if exists
                var associatedUser = await _userService.GetUserByFacultyIdAsync(id);
                if (associatedUser != null)
                {
                    await _userService.DeleteUserAsync(associatedUser.UserId);
                    _logger.LogInformation("Deleted associated User account for Faculty: {FacultyId}", id);
                }

                await _facultyRepository.DeleteAsync(faculty);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting faculty: {FacultyId}", id);
                throw;
            }
        }

        public async Task<bool> IsFacultyCodeExistsAsync(string facultyCode, int? excludeFacultyId = null)
        {
            return await _facultyRepository.IsFacultyCodeExistsAsync(facultyCode, excludeFacultyId);
        }

        public async Task<bool> IsEmailExistsAsync(string email, int? excludeFacultyId = null)
        {
            return await _facultyRepository.IsEmailExistsAsync(email, excludeFacultyId);
        }

        public async Task<IEnumerable<Faculty>> GetFacultiesByDepartmentAsync(string department)
        {
            try
            {
                return await _facultyRepository.GetFacultiesByDepartmentAsync(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting faculties by department: {Department}", department);
                throw;
            }
        }
    }
}