using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Moq;
using Web_SIMS.Controllers;
using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.Models;
using Xunit;
using FluentAssertions;

namespace Web_SIMS.Tests.Controllers
{
    public class EnrollmentControllerTests
    {
        private readonly Mock<IEnrollmentService> _mockEnrollmentService;
        private readonly Mock<ICourseService> _mockCourseService;
        private readonly Mock<IStudentService> _mockStudentService;
        private readonly Mock<ILogger<EnrollmentController>> _mockLogger;
        private readonly EnrollmentController _controller;

        public EnrollmentControllerTests()
        {
            _mockEnrollmentService = new Mock<IEnrollmentService>();
            _mockCourseService = new Mock<ICourseService>();
            _mockStudentService = new Mock<IStudentService>();
            _mockLogger = new Mock<ILogger<EnrollmentController>>();

            _controller = new EnrollmentController(
                _mockEnrollmentService.Object,
                _mockCourseService.Object,
                _mockStudentService.Object,
                _mockLogger.Object);

            // Setup TempData
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());
        }

        [Fact]
        public async Task Create_Get_ReturnsViewWithDropdownData()
        {
            // Arrange
            var courses = new List<Course>
            {
                new Course { CourseId = 1, CourseCode = "CS101", CourseName = "Lập trình Cơ bản", IsActive = true },
                new Course { CourseId = 2, CourseCode = "CS201", CourseName = "Lập trình Hướng đối tượng", IsActive = true }
            };

            var students = new List<Student>
            {
                new Student { StudentId = 1, StudentCode = "SV001", FullName = "Nguyễn Văn A", IsActive = true },
                new Student { StudentId = 2, StudentCode = "SV002", FullName = "Trần Thị B", IsActive = true }
            };

            var coursePaginatedList = new PaginatedList<Course>(courses, courses.Count, 1, 1000);
            var studentPaginatedList = new PaginatedList<Student>(students, students.Count, 1, 1000);

            _mockCourseService.Setup(x => x.GetCoursesAsync(It.IsAny<CourseFilterOptions>()))
                .ReturnsAsync(coursePaginatedList);
            _mockStudentService.Setup(x => x.GetStudentsAsync(It.IsAny<StudentFilterOptions>()))
                .ReturnsAsync(studentPaginatedList);

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            
            // Verify ViewBag contains dropdown data
            var courseItems = _controller.ViewBag.Courses as List<SelectListItem>;
            courseItems.Should().NotBeNull();
            courseItems.Should().HaveCount(2);
            courseItems![0].Text.Should().Be("CS101 - Lập trình Cơ bản");
            courseItems[0].Value.Should().Be("1");

            var studentItems = _controller.ViewBag.Students as List<SelectListItem>;
            studentItems.Should().NotBeNull();
            studentItems.Should().HaveCount(2);
            studentItems![0].Text.Should().Be("SV001 - Nguyễn Văn A");
            studentItems[0].Value.Should().Be("1");
        }

        [Fact]
        public async Task Create_Post_WithValidModel_CreatesEnrollmentAndRedirects()
        {
            // Arrange
            var enrollment = new Enrollment
            {
                StudentId = 1,
                CourseId = 1,
                Status = EnrollmentStatus.Pending,
                Notes = "Đăng ký bình thường"
            };

            _mockEnrollmentService.Setup(x => x.CreateEnrollmentAsync(It.IsAny<Enrollment>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(enrollment);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _controller.TempData["SuccessMessage"].Should().Be("Tạo đăng ký thành công.");

            _mockEnrollmentService.Verify(x => x.CreateEnrollmentAsync(
                It.Is<Enrollment>(e => 
                    e.StudentId == enrollment.StudentId &&
                    e.CourseId == enrollment.CourseId &&
                    e.Status == enrollment.Status &&
                    e.Notes == enrollment.Notes &&
                    e.EnrollmentDate != default(DateTime))), Times.Once);
        }

        [Fact]
        public async Task Create_Post_WithDuplicateEnrollment_ReturnsViewWithError()
        {
            // Arrange
            var enrollment = new Enrollment
            {
                StudentId = 1,
                CourseId = 1,
                Status = EnrollmentStatus.Pending,
                Notes = "Đăng ký trùng lặp"
            };

            _mockEnrollmentService.Setup(x => x.CreateEnrollmentAsync(It.IsAny<Enrollment>()))
                .ThrowsAsync(new InvalidOperationException("Sinh viên đã đăng ký khóa học này."));

            // Setup dropdown data for view reload
            SetupDropdownData();

            // Act
            var result = await _controller.Create(enrollment);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(enrollment);
            _controller.ModelState.Should().ContainKey("");
            _controller.ModelState[""]!.Errors.Should().Contain(e => 
                e.ErrorMessage == "Sinh viên đã đăng ký khóa học này.");
        }

        [Fact]
        public async Task Create_Post_WithFullCourse_ReturnsViewWithError()
        {
            // Arrange
            var enrollment = new Enrollment
            {
                StudentId = 1,
                CourseId = 1,
                Status = EnrollmentStatus.Pending,
                Notes = "Khóa học đã đầy"
            };

            _mockEnrollmentService.Setup(x => x.CreateEnrollmentAsync(It.IsAny<Enrollment>()))
                .ThrowsAsync(new InvalidOperationException("Khóa học đã đạt số lượng sinh viên tối đa."));

            // Setup dropdown data for view reload
            SetupDropdownData();

            // Act
            var result = await _controller.Create(enrollment);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(enrollment);
            _controller.ModelState.Should().ContainKey("");
            _controller.ModelState[""]!.Errors.Should().Contain(e => 
                e.ErrorMessage == "Khóa học đã đạt số lượng sinh viên tối đa.");
        }

        [Fact]
        public async Task Create_Post_WithInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var enrollment = new Enrollment
            {
                StudentId = 0, // Invalid - required
                CourseId = 0,  // Invalid - required
                Status = EnrollmentStatus.Pending
            };

            _controller.ModelState.AddModelError("StudentId", "Sinh viên là bắt buộc.");
            _controller.ModelState.AddModelError("CourseId", "Khóa học là bắt buộc.");

            // Setup dropdown data for view reload
            SetupDropdownData();

            // Act
            var result = await _controller.Create(enrollment);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(enrollment);
            
            // Verify service was not called
            _mockEnrollmentService.Verify(x => x.CreateEnrollmentAsync(It.IsAny<Enrollment>()), Times.Never);
        }

        [Fact]
        public async Task Index_WithFilters_ReturnsFilteredEnrollments()
        {
            // Arrange
            string searchString = "SV001";
            string statusFilter = "approved";
            string courseFilter = "CS101";
            int pageNumber = 1;

            var enrollments = new List<Enrollment>
            {
                new Enrollment 
                { 
                    EnrollmentId = 1, 
                    StudentId = 1, 
                    CourseId = 1, 
                    Status = EnrollmentStatus.Approved,
                    Student = new Student { StudentCode = "SV001", FullName = "Nguyễn Văn A" },
                    Course = new Course { CourseCode = "CS101", CourseName = "Lập trình Cơ bản" }
                }
            };

            var paginatedList = new PaginatedList<Enrollment>(enrollments, enrollments.Count, pageNumber, 10);

            _mockEnrollmentService.Setup(x => x.GetEnrollmentsAsync(It.Is<EnrollmentFilterOptions>(f =>
                f.SearchString == searchString &&
                f.StatusFilter == statusFilter &&
                f.CourseFilter == courseFilter &&
                f.PageNumber == pageNumber &&
                f.PageSize == 10)))
                .ReturnsAsync(paginatedList);

            // Act
            var result = await _controller.Index(searchString, statusFilter, courseFilter, pageNumber);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<PaginatedList<Enrollment>>().Subject;
            model.Should().Be(paginatedList);
            
            _controller.ViewData["CurrentFilter"].Should().Be(searchString);
            _controller.ViewData["StatusFilter"].Should().Be(statusFilter);
            _controller.ViewData["CourseFilter"].Should().Be(courseFilter);
        }

        [Fact]
        public async Task Edit_WithValidId_ReturnsViewWithEnrollment()
        {
            // Arrange
            int enrollmentId = 1;
            var enrollment = new Enrollment
            {
                EnrollmentId = enrollmentId,
                StudentId = 1,
                CourseId = 1,
                Status = EnrollmentStatus.Pending,
                Notes = "Test enrollment"
            };

            _mockEnrollmentService.Setup(x => x.GetEnrollmentByIdAsync(enrollmentId))
                .ReturnsAsync(enrollment);

            // Setup dropdown data
            SetupDropdownData();

            // Act
            var result = await _controller.Edit(enrollmentId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<Enrollment>().Subject;
            model.Should().Be(enrollment);
        }

        [Fact]
        public async Task Edit_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int invalidId = 999;

            _mockEnrollmentService.Setup(x => x.GetEnrollmentByIdAsync(invalidId))
                .ReturnsAsync((Enrollment?)null);

            // Act
            var result = await _controller.Edit(invalidId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Edit_WithNullId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Edit(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Create_Post_WithServiceException_ReturnsViewWithError()
        {
            // Arrange
            var enrollment = new Enrollment
            {
                StudentId = 1,
                CourseId = 1,
                Status = EnrollmentStatus.Pending
            };

            _mockEnrollmentService.Setup(x => x.CreateEnrollmentAsync(It.IsAny<Enrollment>()))
                .ThrowsAsync(new Exception("Database connection error"));

            // Setup dropdown data for view reload
            SetupDropdownData();

            // Act
            var result = await _controller.Create(enrollment);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(enrollment);
            _controller.ModelState.Should().ContainKey("");
            _controller.ModelState[""]!.Errors.Should().Contain(e => 
                e.ErrorMessage == "Có lỗi xảy ra khi tạo đăng ký.");
        }

        [Fact]
        public async Task Create_Post_WithInactiveStudent_ReturnsViewWithError()
        {
            // Arrange
            var enrollment = new Enrollment
            {
                StudentId = 1,
                CourseId = 1,
                Status = EnrollmentStatus.Pending,
                Notes = "Sinh viên không hoạt động"
            };

            _mockEnrollmentService.Setup(x => x.CreateEnrollmentAsync(It.IsAny<Enrollment>()))
                .ThrowsAsync(new InvalidOperationException("Sinh viên không hoạt động hoặc không tồn tại."));

            // Setup dropdown data for view reload
            SetupDropdownData();

            // Act
            var result = await _controller.Create(enrollment);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(enrollment);
            _controller.ModelState.Should().ContainKey("");
            _controller.ModelState[""]!.Errors.Should().Contain(e => 
                e.ErrorMessage == "Sinh viên không hoạt động hoặc không tồn tại.");
        }

        private void SetupDropdownData()
        {
            var courses = new List<Course>
            {
                new Course { CourseId = 1, CourseCode = "CS101", CourseName = "Lập trình Cơ bản", IsActive = true }
            };

            var students = new List<Student>
            {
                new Student { StudentId = 1, StudentCode = "SV001", FullName = "Nguyễn Văn A", IsActive = true }
            };

            var coursePaginatedList = new PaginatedList<Course>(courses, courses.Count, 1, 1000);
            var studentPaginatedList = new PaginatedList<Student>(students, students.Count, 1, 1000);

            _mockCourseService.Setup(x => x.GetCoursesAsync(It.IsAny<CourseFilterOptions>()))
                .ReturnsAsync(coursePaginatedList);
            _mockStudentService.Setup(x => x.GetStudentsAsync(It.IsAny<StudentFilterOptions>()))
                .ReturnsAsync(studentPaginatedList);
        }
    }
}