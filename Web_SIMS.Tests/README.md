# Unit Tests cho Hệ thống SIMS

## Tổng quan

Bộ unit tests này được viết cho hệ thống Web_SIMS (Student Information Management System) với **solution riêng** (`Web_SIMS.Tests.sln`) để tách biệt rõ ràng giữa main project và test project.

**Framework & Tools sử dụng**:
- **xUnit**: Framework testing chính
- **Moq**: Mock framework cho dependency injection
- **FluentAssertions**: Assertion library để viết test dễ đọc hơn
- **Microsoft.AspNetCore.Mvc.Testing**: Testing utilities cho ASP.NET Core

## Cấu trúc Tests

### 1. AccountControllerTests.cs
**Chức năng**: Test các tính năng authentication và authorization
**Bao gồm**:
- ✅ Đăng nhập với thông tin hợp lệ
- ✅ Đăng nhập với thông tin không hợp lệ
- ✅ Đăng nhập với tài khoản bị khóa
- ✅ Redirect theo role (Admin, Student, Faculty)
- ✅ Đăng xuất
- ✅ Đổi mật khẩu
- ✅ Xử lý returnUrl an toàn

### 2. StudentControllerTests.cs
**Chức năng**: Test quản lý sinh viên
**Bao gồm**:
- ✅ Tạo sinh viên mới với tài khoản
- ✅ Tạo sinh viên mới không có tài khoản
- ✅ Xử lý lỗi trùng mã sinh viên
- ✅ Validation dữ liệu đầu vào
- ✅ Xem chi tiết sinh viên
- ✅ Tìm kiếm và phân trang

### 3. FacultyManagementControllerTests.cs
**Chức năng**: Test quản lý giảng viên
**Bao gồm**:
- ✅ Tạo giảng viên mới với tài khoản
- ✅ Tạo giảng viên mới không có tài khoản
- ✅ Xử lý lỗi trùng mã giảng viên
- ✅ Xử lý lỗi trùng email
- ✅ Validation dữ liệu đầu vào
- ✅ Xem chi tiết giảng viên
- ✅ Tìm kiếm và lọc dữ liệu

### 4. EnrollmentControllerTests.cs
**Chức năng**: Test đăng ký khóa học
**Bao gồm**:
- ✅ Đăng ký khóa học thành công
- ✅ Xử lý lỗi đăng ký trùng lặp
- ✅ Xử lý lỗi khóa học đã đầy
- ✅ Validation sinh viên và khóa học
- ✅ Load dropdown data
- ✅ Tìm kiếm và lọc đăng ký

### 5. TestHelper.cs
**Chức năng**: Utilities và helper methods
**Bao gồm**:
- 🔧 Tạo test data objects
- 🔧 Setup HttpContext và TempData
- 🔧 Tạo ClaimsPrincipal cho authentication
- 🔧 Helper methods cho ViewModels
- 🔧 Constants cho test data

## Cách chạy Tests

### 1. Từ Visual Studio
```
1. Mở solution Web_SIMS.Tests.sln (solution riêng cho tests)
2. Build solution (Ctrl + Shift + B)
3. Mở Test Explorer (Test > Test Explorer)
4. Click "Run All Tests"
```

### 2. Từ Command Line - Sử dụng Scripts
```bash
# Windows
build-and-test.bat

# Linux/Mac
chmod +x build-and-test.sh
./build-and-test.sh
```

### 3. Từ Command Line - Manual
```bash
# Chạy tất cả tests với solution riêng
dotnet test Web_SIMS.Tests.sln

# Chạy tests với verbose output
dotnet test Web_SIMS.Tests.sln --verbosity normal

# Chạy tests với code coverage
dotnet test Web_SIMS.Tests.sln --collect:"XPlat Code Coverage"

# Chạy tests của một class cụ thể
dotnet test Web_SIMS.Tests.sln --filter "ClassName=AccountControllerTests"

# Chạy tests của một method cụ thể
dotnet test Web_SIMS.Tests.sln --filter "MethodName=Login_Post_WithValidCredentials_RedirectsToCorrectPage"
```

### 3. Từ VS Code
```
1. Cài đặt extension "C# Dev Kit"
2. Mở Command Palette (Ctrl + Shift + P)
3. Chọn ".NET: Run Tests"
```

## Kết quả mong đợi

### Tổng số tests: **40+ tests**

#### AccountControllerTests: 10 tests
- Login_Get_ReturnsViewWithModel ✅
- Login_Get_WithInvalidReturnUrl_SetsReturnUrlToNull ✅
- Login_Post_WithValidCredentials_RedirectsToCorrectPage ✅
- Login_Post_WithInvalidCredentials_ReturnsViewWithError ✅
- Login_Post_WithLockedAccount_ReturnsViewWithError ✅
- Login_Post_WithStudentRole_RedirectsToStudentPortal ✅
- Logout_Post_SignsOutAndRedirectsToLogin ✅
- ChangePassword_Post_WithValidData_ReturnsSuccessRedirect ✅
- ChangePassword_Post_WithInvalidCurrentPassword_ReturnsViewWithError ✅

#### StudentControllerTests: 10 tests
- Create_Get_ReturnsView ✅
- Create_Post_WithValidModel_CreatesStudentAndRedirects ✅
- Create_Post_WithValidModelButNoCredentials_CreatesStudentWithoutUserAccount ✅
- Create_Post_WithDuplicateStudentCode_ReturnsViewWithError ✅
- Create_Post_WithInvalidModel_ReturnsViewWithModel ✅
- Details_WithValidId_ReturnsViewWithStudent ✅
- Details_WithInvalidId_ReturnsNotFound ✅
- Details_WithNullId_ReturnsNotFound ✅
- Index_WithSearchFilter_ReturnsFilteredStudents ✅
- Create_Post_WithServiceException_ReturnsViewWithError ✅

#### FacultyManagementControllerTests: 11 tests
- Create_Get_ReturnsView ✅
- Create_Post_WithValidModel_CreatesFacultyAndRedirects ✅
- Create_Post_WithValidModelButNoCredentials_CreatesFacultyWithoutUserAccount ✅
- Create_Post_WithDuplicateFacultyCode_ReturnsViewWithError ✅
- Create_Post_WithDuplicateEmail_ReturnsViewWithError ✅
- Create_Post_WithInvalidModel_ReturnsViewWithModel ✅
- Details_WithValidId_ReturnsViewWithFaculty ✅
- Details_WithInvalidId_ReturnsNotFound ✅
- Details_WithNullId_ReturnsNotFound ✅
- Index_WithSearchFilter_ReturnsFilteredFaculties ✅
- Create_Post_WithMinimalValidData_CreatesFacultySuccessfully ✅

#### EnrollmentControllerTests: 12 tests
- Create_Get_ReturnsViewWithDropdownData ✅
- Create_Post_WithValidModel_CreatesEnrollmentAndRedirects ✅
- Create_Post_WithDuplicateEnrollment_ReturnsViewWithError ✅
- Create_Post_WithFullCourse_ReturnsViewWithError ✅
- Create_Post_WithInvalidModel_ReturnsViewWithModel ✅
- Index_WithFilters_ReturnsFilteredEnrollments ✅
- Edit_WithValidId_ReturnsViewWithEnrollment ✅
- Edit_WithInvalidId_ReturnsNotFound ✅
- Edit_WithNullId_ReturnsNotFound ✅
- Create_Post_WithServiceException_ReturnsViewWithError ✅
- Create_Post_WithInactiveStudent_ReturnsViewWithError ✅

## Debugging Tests

### 1. Test thất bại
```bash
# Chạy test cụ thể với debug info
dotnet test --filter "MethodName=TestMethodName" --verbosity diagnostic
```

### 2. Breakpoints trong tests
- Đặt breakpoint trong test method
- Right-click test trong Test Explorer
- Chọn "Debug Selected Tests"

### 3. Xem test output
```bash
# Chạy với logger console
dotnet test --logger "console;verbosity=detailed"
```

## Best Practices

### 1. Naming Convention
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    // Act  
    // Assert
}
```

### 2. Test Structure (AAA Pattern)
```csharp
[Fact]
public async Task Create_Post_WithValidModel_CreatesStudentAndRedirects()
{
    // Arrange - Setup test data và mocks
    var model = TestHelper.ViewModels.CreateStudentViewModel();
    _mockService.Setup(x => x.Method()).Returns(Task.CompletedTask);

    // Act - Thực hiện action cần test
    var result = await _controller.Create(model);

    // Assert - Verify kết quả
    result.Should().BeOfType<RedirectToActionResult>();
    _mockService.Verify(x => x.Method(), Times.Once);
}
```

### 3. Mock Setup
```csharp
// Setup mock return value
_mockService.Setup(x => x.GetAsync(It.IsAny<int>())).ReturnsAsync(testData);

// Setup mock exception
_mockService.Setup(x => x.CreateAsync(It.IsAny<Model>()))
           .ThrowsAsync(new InvalidOperationException("Error message"));

// Verify mock was called
_mockService.Verify(x => x.Method(It.IsAny<Parameter>()), Times.Once);
```

## Troubleshooting

### 1. Tests không chạy
- Kiểm tra project reference đúng
- Build solution trước khi chạy tests
- Kiểm tra package references

### 2. Mock không hoạt động
- Đảm bảo interface được mock đúng
- Kiểm tra setup parameters match với actual call
- Sử dụng `It.IsAny<T>()` nếu cần

### 3. Authentication tests thất bại
- Kiểm tra ClaimsPrincipal được setup đúng
- Verify HttpContext được mock properly
- Đảm bảo authentication scheme match

## Mở rộng Tests

### Thêm test mới:
1. Tạo test method với `[Fact]` attribute
2. Follow AAA pattern
3. Sử dụng TestHelper cho common setup
4. Verify với FluentAssertions

### Test Integration:
- Tạo `IntegrationTests` project riêng
- Sử dụng `WebApplicationFactory<Program>`
- Test với real database (In-Memory hoặc TestContainers)

---

**Lưu ý**: Đảm bảo tất cả tests pass trước khi commit code!