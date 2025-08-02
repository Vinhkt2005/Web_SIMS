using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;
using Web_SIMS.Models;

namespace Web_SIMS.Tests
{
    /// <summary>
    /// Helper class chứa các phương thức tiện ích cho unit tests
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Tạo ClaimsPrincipal cho user test
        /// </summary>
        public static ClaimsPrincipal CreateTestUser(int userId, string username, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        /// <summary>
        /// Tạo User object cho test
        /// </summary>
        public static User CreateTestUserEntity(int userId, string username, string roleName, int roleId)
        {
            return new User
            {
                UserId = userId,
                Username = username,
                Email = $"{username}@sims.edu",
                FullName = $"Test User {userId}",
                IsActive = true,
                CreatedDate = DateTime.Now,
                RoleId = roleId,
                Role = new Role
                {
                    RoleId = roleId,
                    RoleName = roleName,
                    Description = $"Test {roleName} Role",
                    IsActive = true
                }
            };
        }

        /// <summary>
        /// Tạo Student object cho test
        /// </summary>
        public static Student CreateTestStudent(int studentId, string studentCode, string fullName)
        {
            return new Student
            {
                StudentId = studentId,
                StudentCode = studentCode,
                FullName = fullName,
                Email = $"{studentCode.ToLower()}@sims.edu",
                PhoneNumber = "0123456789",
                DateOfBirth = new DateTime(2000, 1, 1),
                Address = "Test Address",
                Gender = "Nam",
                Major = "Công nghệ thông tin",
                AcademicYear = "2024-2025",
                EnrollmentDate = DateTime.Now,
                IsActive = true,
                CreatedDate = DateTime.Now
            };
        }

        /// <summary>
        /// Tạo Faculty object cho test
        /// </summary>
        public static Faculty CreateTestFaculty(int facultyId, string facultyCode, string fullName)
        {
            return new Faculty
            {
                FacultyId = facultyId,
                FacultyCode = facultyCode,
                FullName = fullName,
                Email = $"{facultyCode.ToLower()}@university.edu.vn",
                PhoneNumber = "0123456789",
                Department = "Khoa Công nghệ Thông tin",
                Specialization = "Lập trình",
                IsActive = true,
                CreatedDate = DateTime.Now
            };
        }

        /// <summary>
        /// Tạo Course object cho test
        /// </summary>
        public static Course CreateTestCourse(int courseId, string courseCode, string courseName, int facultyId)
        {
            return new Course
            {
                CourseId = courseId,
                CourseCode = courseCode,
                CourseName = courseName,
                Description = $"Test course: {courseName}",
                Credits = 3,
                MaxStudents = 50,
                Semester = "Học kỳ 1",
                AcademicYear = "2024-2025",
                Instructor = "Test Instructor",
                FacultyId = facultyId,
                IsActive = true,
                CreatedDate = DateTime.Now
            };
        }

        /// <summary>
        /// Tạo Enrollment object cho test
        /// </summary>
        public static Enrollment CreateTestEnrollment(int enrollmentId, int studentId, int courseId, EnrollmentStatus status)
        {
            return new Enrollment
            {
                EnrollmentId = enrollmentId,
                StudentId = studentId,
                CourseId = courseId,
                EnrollmentDate = DateTime.Now,
                Status = status,
                Notes = "Test enrollment"
            };
        }

        /// <summary>
        /// Setup TempData cho controller test
        /// </summary>
        public static void SetupTempData(Controller controller)
        {
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Mock.Of<ITempDataProvider>();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);
        }

        /// <summary>
        /// Setup HttpContext cho controller test
        /// </summary>
        public static void SetupHttpContext(Controller controller, ClaimsPrincipal? user = null)
        {
            var httpContext = new DefaultHttpContext();
            if (user != null)
            {
                httpContext.User = user;
            }

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        /// <summary>
        /// Tạo danh sách test data cho pagination
        /// </summary>
        public static List<T> CreateTestList<T>(int count, Func<int, T> factory)
        {
            var list = new List<T>();
            for (int i = 1; i <= count; i++)
            {
                list.Add(factory(i));
            }
            return list;
        }

        /// <summary>
        /// Verify ModelState có lỗi với key và message cụ thể
        /// </summary>
        public static bool HasModelError(Controller controller, string key, string expectedMessage)
        {
            if (!controller.ModelState.ContainsKey(key))
                return false;

            var errors = controller.ModelState[key]?.Errors;
            return errors?.Any(e => e.ErrorMessage == expectedMessage) == true;
        }

        /// <summary>
        /// Tạo mock IUrlHelper
        /// </summary>
        public static Mock<IUrlHelper> CreateMockUrlHelper(bool isLocalUrl = true)
        {
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(x => x.IsLocalUrl(It.IsAny<string>())).Returns(isLocalUrl);
            return mockUrlHelper;
        }

        /// <summary>
        /// Tạo test data cho ViewModels
        /// </summary>
        public static class ViewModels
        {
            public static Web_SIMS.ViewModels.LoginViewModel CreateLoginViewModel(string username = "testuser", string password = "password123", string? returnUrl = null)
            {
                return new Web_SIMS.ViewModels.LoginViewModel
                {
                    Username = username,
                    Password = password,
                    ReturnUrl = returnUrl
                };
            }

            public static Web_SIMS.ViewModels.CreateStudentViewModel CreateStudentViewModel(string studentCode = "SV001", string fullName = "Test Student")
            {
                return new Web_SIMS.ViewModels.CreateStudentViewModel
                {
                    StudentCode = studentCode,
                    FullName = fullName,
                    Email = $"{studentCode.ToLower()}@sims.edu",
                    PhoneNumber = "0123456789",
                    Address = "Test Address",
                    DateOfBirth = new DateTime(2000, 1, 1),
                    Gender = "Nam",
                    Major = "Công nghệ thông tin",
                    AcademicYear = "2024-2025",
                    Notes = "Test student",
                    Username = $"user{studentCode}",
                    Password = "password123"
                };
            }

            public static Web_SIMS.ViewModels.CreateFacultyViewModel CreateFacultyViewModel(string facultyCode = "GV001", string fullName = "Test Faculty")
            {
                return new Web_SIMS.ViewModels.CreateFacultyViewModel
                {
                    FacultyCode = facultyCode,
                    FullName = fullName,
                    Email = $"{facultyCode.ToLower()}@university.edu.vn",
                    PhoneNumber = "0123456789",
                    Department = "Khoa Công nghệ Thông tin",
                    Specialization = "Lập trình",
                    Username = $"user{facultyCode}",
                    Password = "password123"
                };
            }

            public static Web_SIMS.ViewModels.ChangePasswordViewModel CreateChangePasswordViewModel(string currentPassword = "oldpassword", string newPassword = "newpassword123")
            {
                return new Web_SIMS.ViewModels.ChangePasswordViewModel
                {
                    CurrentPassword = currentPassword,
                    NewPassword = newPassword,
                    ConfirmPassword = newPassword
                };
            }
        }

        /// <summary>
        /// Constants cho test data
        /// </summary>
        public static class Constants
        {
            public const string AdminRole = "Admin";
            public const string FacultyRole = "Faculty";
            public const string StudentRole = "Student";

            public const string DefaultPassword = "password123";
            public const string DefaultEmail = "test@sims.edu";
            public const string DefaultPhoneNumber = "0123456789";

            public const int DefaultPageSize = 10;
            public const int DefaultPageNumber = 1;
        }
    }
}