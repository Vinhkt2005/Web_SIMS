using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Web_SIMS.Controllers;
using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;
using Xunit;
using FluentAssertions;

namespace Web_SIMS.Tests.Controllers
{
    public class StudentControllerTests
    {
        private readonly Mock<IStudentService> _mockStudentService;
        private readonly Mock<IEnrollmentService> _mockEnrollmentService;
        private readonly Mock<ILogger<StudentController>> _mockLogger;
        private readonly StudentController _controller;

        public StudentControllerTests()
        {
            _mockStudentService = new Mock<IStudentService>();
            _mockEnrollmentService = new Mock<IEnrollmentService>();
            _mockLogger = new Mock<ILogger<StudentController>>();

            _controller = new StudentController(
                _mockStudentService.Object,
                _mockEnrollmentService.Object,
                _mockLogger.Object);

            // Setup TempData
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            // Act
            var result = _controller.Create();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Create_Post_WithValidModel_CreatesStudentAndRedirects()
        {
            // Arrange
            var model = new CreateStudentViewModel
            {
                StudentCode = "SV001",
                FullName = "Nguyễn Văn A",
                Email = "nguyenvana@sims.edu",
                PhoneNumber = "0123456789",
                Address = "Hà Nội",
                DateOfBirth = new DateTime(2000, 1, 1),
                Gender = "Nam",
                Major = "Công nghệ thông tin",
                AcademicYear = "2024-2025",
                Notes = "Sinh viên mới",
                Username = "student001",
                Password = "password123"
            };

            _mockStudentService.Setup(x => x.CreateStudentAsync(
                It.IsAny<Student>(), 
                model.Username, 
                model.Password))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _controller.TempData["SuccessMessage"].Should().Be("Tạo sinh viên thành công.");

            _mockStudentService.Verify(x => x.CreateStudentAsync(
                It.Is<Student>(s => 
                    s.StudentCode == model.StudentCode &&
                    s.FullName == model.FullName &&
                    s.Email == model.Email &&
                    s.PhoneNumber == model.PhoneNumber &&
                    s.Address == model.Address &&
                    s.Gender == model.Gender &&
                    s.Major == model.Major &&
                    s.AcademicYear == model.AcademicYear &&
                    s.Notes == model.Notes &&
                    s.IsActive == true),
                model.Username,
                model.Password), Times.Once);
        }

        [Fact]
        public async Task Create_Post_WithValidModelButNoCredentials_CreatesStudentWithoutUserAccount()
        {
            // Arrange
            var model = new CreateStudentViewModel
            {
                StudentCode = "SV002",
                FullName = "Trần Thị B",
                Email = "tranthib@sims.edu",
                PhoneNumber = "0987654321",
                Address = "TP.HCM",
                DateOfBirth = new DateTime(2001, 5, 15),
                Gender = "Nữ",
                Major = "Kinh tế",
                AcademicYear = "2024-2025",
                Notes = "Sinh viên chuyển trường",
                Username = "", // No username
                Password = ""  // No password
            };

            _mockStudentService.Setup(x => x.CreateStudentAsync(It.IsAny<Student>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _controller.TempData["SuccessMessage"].Should().Be("Tạo sinh viên thành công.");

            _mockStudentService.Verify(x => x.CreateStudentAsync(
                It.Is<Student>(s => 
                    s.StudentCode == model.StudentCode &&
                    s.FullName == model.FullName &&
                    s.Email == model.Email)), Times.Once);

            // Verify that the overload with username/password was NOT called
            _mockStudentService.Verify(x => x.CreateStudentAsync(
                It.IsAny<Student>(), 
                It.IsAny<string>(), 
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_WithDuplicateStudentCode_ReturnsViewWithError()
        {
            // Arrange
            var model = new CreateStudentViewModel
            {
                StudentCode = "SV001", // Duplicate code
                FullName = "Nguyễn Văn C",
                Email = "nguyenvanc@sims.edu",
                PhoneNumber = "0123456790",
                Username = "student003",
                Password = "password123"
            };

            _mockStudentService.Setup(x => x.CreateStudentAsync(
                It.IsAny<Student>(), 
                model.Username, 
                model.Password))
                .ThrowsAsync(new InvalidOperationException("Mã sinh viên đã tồn tại."));

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(model);
            _controller.ModelState.Should().ContainKey("");
            _controller.ModelState[""]!.Errors.Should().Contain(e => 
                e.ErrorMessage == "Mã sinh viên đã tồn tại.");
        }

        [Fact]
        public async Task Create_Post_WithInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new CreateStudentViewModel
            {
                StudentCode = "", // Invalid - required field
                FullName = "Nguyễn Văn D",
                Email = "invalid-email", // Invalid email format
                Username = "student004",
                Password = "password123"
            };

            _controller.ModelState.AddModelError("StudentCode", "Mã sinh viên là bắt buộc.");
            _controller.ModelState.AddModelError("Email", "Email không hợp lệ.");

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(model);
            
            // Verify service was not called
            _mockStudentService.Verify(x => x.CreateStudentAsync(
                It.IsAny<Student>(), 
                It.IsAny<string>(), 
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Details_WithValidId_ReturnsViewWithStudent()
        {
            // Arrange
            int studentId = 1;
            var student = new Student
            {
                StudentId = studentId,
                StudentCode = "SV001",
                FullName = "Nguyễn Văn A",
                Email = "nguyenvana@sims.edu",
                PhoneNumber = "0123456789",
                Major = "Công nghệ thông tin",
                IsActive = true
            };

            _mockStudentService.Setup(x => x.GetStudentWithDetailsAsync(studentId))
                .ReturnsAsync(student);

            // Act
            var result = await _controller.Details(studentId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<Student>().Subject;
            model.Should().Be(student);
        }

        [Fact]
        public async Task Details_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int invalidId = 999;

            _mockStudentService.Setup(x => x.GetStudentWithDetailsAsync(invalidId))
                .ReturnsAsync((Student?)null);

            // Act
            var result = await _controller.Details(invalidId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Details_WithNullId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Index_WithSearchFilter_ReturnsFilteredStudents()
        {
            // Arrange
            string searchString = "Nguyễn";
            string statusFilter = "active";
            string sortOrder = "name_desc";
            int pageNumber = 1;

            var students = new List<Student>
            {
                new Student { StudentId = 1, StudentCode = "SV001", FullName = "Nguyễn Văn A", IsActive = true },
                new Student { StudentId = 2, StudentCode = "SV002", FullName = "Nguyễn Thị B", IsActive = true }
            };

            var paginatedList = new PaginatedList<Student>(students, students.Count, pageNumber, 10);

            _mockStudentService.Setup(x => x.GetStudentsAsync(It.Is<StudentFilterOptions>(f =>
                f.SearchString == searchString &&
                f.StatusFilter == statusFilter &&
                f.SortOrder == sortOrder &&
                f.PageNumber == pageNumber &&
                f.PageSize == 10)))
                .ReturnsAsync(paginatedList);

            // Act
            var result = await _controller.Index(searchString, statusFilter, sortOrder, pageNumber);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<PaginatedList<Student>>().Subject;
            model.Should().Be(paginatedList);
            
            _controller.ViewData["CurrentFilter"].Should().Be(searchString);
            _controller.ViewData["StatusFilter"].Should().Be(statusFilter);
            _controller.ViewData["CurrentSort"].Should().Be(sortOrder);
        }

        [Fact]
        public async Task Create_Post_WithServiceException_ReturnsViewWithError()
        {
            // Arrange
            var model = new CreateStudentViewModel
            {
                StudentCode = "SV005",
                FullName = "Test Student",
                Email = "test@sims.edu",
                Username = "testuser",
                Password = "password123"
            };

            _mockStudentService.Setup(x => x.CreateStudentAsync(
                It.IsAny<Student>(), 
                model.Username, 
                model.Password))
                .ThrowsAsync(new Exception("Database connection error"));

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(model);
            _controller.ModelState.Should().ContainKey("");
            _controller.ModelState[""]!.Errors.Should().Contain(e => 
                e.ErrorMessage == "Có lỗi xảy ra khi tạo sinh viên.");
        }
    }
}