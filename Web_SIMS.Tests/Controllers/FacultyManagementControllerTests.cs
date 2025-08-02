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
    public class FacultyManagementControllerTests
    {
        private readonly Mock<IFacultyService> _mockFacultyService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ILogger<FacultyManagementController>> _mockLogger;
        private readonly FacultyManagementController _controller;

        public FacultyManagementControllerTests()
        {
            _mockFacultyService = new Mock<IFacultyService>();
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<FacultyManagementController>>();

            _controller = new FacultyManagementController(
                _mockFacultyService.Object,
                _mockUserService.Object,
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
        public async Task Create_Post_WithValidModel_CreatesFacultyAndRedirects()
        {
            // Arrange
            var model = new CreateFacultyViewModel
            {
                FacultyCode = "GV001",
                FullName = "TS. Nguyễn Văn Giảng",
                Email = "nguyen.giang@university.edu.vn",
                PhoneNumber = "0123456789",
                Address = "Hà Nội",
                Department = "Khoa Công nghệ Thông tin",
                Specialization = "Lập trình",
                DateOfBirth = new DateTime(1980, 1, 1),
                Gender = "Nam",
                Username = "faculty001",
                Password = "password123"
            };

            _mockFacultyService.Setup(x => x.CreateFacultyAsync(
                It.IsAny<Faculty>(), 
                model.Username, 
                model.Password))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _controller.TempData["SuccessMessage"].Should().Be("Tạo giảng viên thành công.");

            _mockFacultyService.Verify(x => x.CreateFacultyAsync(
                It.Is<Faculty>(f => 
                    f.FacultyCode == model.FacultyCode &&
                    f.FullName == model.FullName &&
                    f.Email == model.Email &&
                    f.PhoneNumber == model.PhoneNumber &&
                    f.Address == model.Address &&
                    f.Department == model.Department &&
                    f.Specialization == model.Specialization &&
                    f.DateOfBirth == model.DateOfBirth &&
                    f.Gender == model.Gender &&
                    f.IsActive == true),
                model.Username,
                model.Password), Times.Once);
        }

        [Fact]
        public async Task Create_Post_WithValidModelButNoCredentials_CreatesFacultyWithoutUserAccount()
        {
            // Arrange
            var model = new CreateFacultyViewModel
            {
                FacultyCode = "GV002",
                FullName = "TS. Trần Thị Giảng",
                Email = "tran.giang@university.edu.vn",
                PhoneNumber = "0123456790",
                Department = "Khoa Công nghệ Thông tin",
                Specialization = "Cơ sở dữ liệu",
                Username = "", // No username
                Password = ""  // No password
            };

            _mockFacultyService.Setup(x => x.CreateFacultyAsync(It.IsAny<Faculty>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _controller.TempData["SuccessMessage"].Should().Be("Tạo giảng viên thành công.");

            _mockFacultyService.Verify(x => x.CreateFacultyAsync(
                It.Is<Faculty>(f => 
                    f.FacultyCode == model.FacultyCode &&
                    f.FullName == model.FullName &&
                    f.Email == model.Email)), Times.Once);

            // Verify that the overload with username/password was NOT called
            _mockFacultyService.Verify(x => x.CreateFacultyAsync(
                It.IsAny<Faculty>(), 
                It.IsAny<string>(), 
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_WithDuplicateFacultyCode_ReturnsViewWithError()
        {
            // Arrange
            var model = new CreateFacultyViewModel
            {
                FacultyCode = "GV001", // Duplicate code
                FullName = "TS. Lê Văn Kinh",
                Email = "le.kinh@university.edu.vn",
                PhoneNumber = "0123456791",
                Department = "Khoa Kinh tế",
                Specialization = "Kinh tế học",
                Username = "faculty003",
                Password = "password123"
            };

            _mockFacultyService.Setup(x => x.CreateFacultyAsync(
                It.IsAny<Faculty>(), 
                model.Username, 
                model.Password))
                .ThrowsAsync(new InvalidOperationException("Mã giảng viên đã tồn tại."));

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(model);
            _controller.ModelState.Should().ContainKey("");
            _controller.ModelState[""]!.Errors.Should().Contain(e => 
                e.ErrorMessage == "Mã giảng viên đã tồn tại.");
        }

        [Fact]
        public async Task Create_Post_WithDuplicateEmail_ReturnsViewWithError()
        {
            // Arrange
            var model = new CreateFacultyViewModel
            {
                FacultyCode = "GV004",
                FullName = "TS. Phạm Văn Nam",
                Email = "nguyen.giang@university.edu.vn", // Duplicate email
                PhoneNumber = "0123456792",
                Department = "Khoa Kinh tế",
                Specialization = "Quản trị kinh doanh",
                Username = "faculty004",
                Password = "password123"
            };

            _mockFacultyService.Setup(x => x.CreateFacultyAsync(
                It.IsAny<Faculty>(), 
                model.Username, 
                model.Password))
                .ThrowsAsync(new InvalidOperationException("Email đã được sử dụng."));

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(model);
            _controller.ModelState.Should().ContainKey("");
            _controller.ModelState[""]!.Errors.Should().Contain(e => 
                e.ErrorMessage == "Email đã được sử dụng.");
        }

        [Fact]
        public async Task Create_Post_WithInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new CreateFacultyViewModel
            {
                FacultyCode = "", // Invalid - required field
                FullName = "TS. Test Faculty",
                Email = "invalid-email", // Invalid email format
                Department = "Test Department",
                Username = "faculty005",
                Password = "password123"
            };

            _controller.ModelState.AddModelError("FacultyCode", "Mã giảng viên là bắt buộc.");
            _controller.ModelState.AddModelError("Email", "Email không hợp lệ.");

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(model);
            
            // Verify service was not called
            _mockFacultyService.Verify(x => x.CreateFacultyAsync(
                It.IsAny<Faculty>(), 
                It.IsAny<string>(), 
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Details_WithValidId_ReturnsViewWithFaculty()
        {
            // Arrange
            int facultyId = 1;
            var faculty = new Faculty
            {
                FacultyId = facultyId,
                FacultyCode = "GV001",
                FullName = "TS. Nguyễn Văn Giảng",
                Email = "nguyen.giang@university.edu.vn",
                PhoneNumber = "0123456789",
                Department = "Khoa Công nghệ Thông tin",
                Specialization = "Lập trình",
                IsActive = true
            };

            _mockFacultyService.Setup(x => x.GetFacultyWithDetailsAsync(facultyId))
                .ReturnsAsync(faculty);

            // Act
            var result = await _controller.Details(facultyId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<Faculty>().Subject;
            model.Should().Be(faculty);
        }

        [Fact]
        public async Task Details_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int invalidId = 999;

            _mockFacultyService.Setup(x => x.GetFacultyWithDetailsAsync(invalidId))
                .ReturnsAsync((Faculty?)null);

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
        public async Task Index_WithSearchFilter_ReturnsFilteredFaculties()
        {
            // Arrange
            string searchString = "Nguyễn";
            string statusFilter = "active";
            string accountFilter = "with_account";
            string sortOrder = "name_desc";
            int pageNumber = 1;

            var faculties = new List<Faculty>
            {
                new Faculty { FacultyId = 1, FacultyCode = "GV001", FullName = "TS. Nguyễn Văn Giảng", IsActive = true },
                new Faculty { FacultyId = 2, FacultyCode = "GV002", FullName = "TS. Nguyễn Thị Hoa", IsActive = true }
            };

            var paginatedList = new PaginatedList<Faculty>(faculties, faculties.Count, pageNumber, 10);

            _mockFacultyService.Setup(x => x.GetFacultiesAsync(It.Is<FacultyFilterOptions>(f =>
                f.SearchString == searchString &&
                f.StatusFilter == statusFilter &&
                f.SortOrder == sortOrder &&
                f.PageNumber == pageNumber &&
                f.PageSize == 10)))
                .ReturnsAsync(paginatedList);

            // Act
            var result = await _controller.Index(searchString, statusFilter, accountFilter, sortOrder, pageNumber);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<PaginatedList<Faculty>>().Subject;
            model.Should().Be(paginatedList);
            
            _controller.ViewData["CurrentFilter"].Should().Be(searchString);
            _controller.ViewData["StatusFilter"].Should().Be(statusFilter);
            _controller.ViewData["AccountFilter"].Should().Be(accountFilter);
            _controller.ViewData["CurrentSort"].Should().Be(sortOrder);
        }

        [Fact]
        public async Task Create_Post_WithServiceException_ReturnsViewWithError()
        {
            // Arrange
            var model = new CreateFacultyViewModel
            {
                FacultyCode = "GV005",
                FullName = "Test Faculty",
                Email = "test@university.edu.vn",
                Department = "Test Department",
                Username = "testfaculty",
                Password = "password123"
            };

            _mockFacultyService.Setup(x => x.CreateFacultyAsync(
                It.IsAny<Faculty>(), 
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
                e.ErrorMessage == "Có lỗi xảy ra khi tạo giảng viên.");
        }

        [Fact]
        public async Task Create_Post_WithMinimalValidData_CreatesFacultySuccessfully()
        {
            // Arrange
            var model = new CreateFacultyViewModel
            {
                FacultyCode = "GV006",
                FullName = "TS. Minimal Faculty",
                Email = "minimal@university.edu.vn",
                Department = "Test Department",
                // Optional fields left empty
                PhoneNumber = null,
                Address = null,
                Specialization = null,
                DateOfBirth = null,
                Gender = null,
                Username = "minimal",
                Password = "password123"
            };

            _mockFacultyService.Setup(x => x.CreateFacultyAsync(
                It.IsAny<Faculty>(), 
                model.Username, 
                model.Password))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _controller.TempData["SuccessMessage"].Should().Be("Tạo giảng viên thành công.");

            _mockFacultyService.Verify(x => x.CreateFacultyAsync(
                It.Is<Faculty>(f => 
                    f.FacultyCode == model.FacultyCode &&
                    f.FullName == model.FullName &&
                    f.Email == model.Email &&
                    f.Department == model.Department &&
                    f.PhoneNumber == null &&
                    f.Address == null &&
                    f.Specialization == null &&
                    f.DateOfBirth == null &&
                    f.Gender == null),
                model.Username,
                model.Password), Times.Once);
        }
    }
}