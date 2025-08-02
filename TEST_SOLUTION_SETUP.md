# Hướng dẫn Setup Solution Test riêng cho Web_SIMS

## 📁 Cấu trúc Solution Test

```
📦 Web_SIMS.Tests.sln (Solution riêng cho testing)
├── 🎯 Web_SIMS (Main project - reference)
└── 🧪 Web_SIMS.Tests (Test project)
    ├── 📂 Controllers/
    │   ├── AccountControllerTests.cs
    │   ├── StudentControllerTests.cs
    │   ├── FacultyManagementControllerTests.cs
    │   └── EnrollmentControllerTests.cs
    ├── 🔧 TestHelper.cs
    ├── 📖 README.md
    └── 📋 Web_SIMS.Tests.csproj
```

## 🚀 Cách sử dụng Solution Test

### 1. Mở Solution Test trong Visual Studio

```bash
# Mở Visual Studio và load solution test
start Web_SIMS.Tests.sln

# Hoặc từ command line
devenv Web_SIMS.Tests.sln
```

### 2. Build và Test từ Visual Studio

1. **Build Solution**: `Ctrl + Shift + B`
2. **Mở Test Explorer**: `Test > Test Explorer`
3. **Run All Tests**: Click "Run All Tests" trong Test Explorer
4. **Debug Tests**: Right-click test → "Debug Selected Tests"

### 3. Build và Test từ Command Line

#### Windows:
```bash
# Chạy script tự động
build-and-test.bat

# Hoặc manual
dotnet clean Web_SIMS.Tests.sln
dotnet restore Web_SIMS.Tests.sln
dotnet build Web_SIMS.Tests.sln
dotnet test Web_SIMS.Tests.sln
```

#### Linux/Mac:
```bash
# Chạy script tự động
chmod +x build-and-test.sh
./build-and-test.sh

# Hoặc manual
dotnet clean Web_SIMS.Tests.sln
dotnet restore Web_SIMS.Tests.sln
dotnet build Web_SIMS.Tests.sln
dotnet test Web_SIMS.Tests.sln
```

## 🎯 Lợi ích của Solution Test riêng

### ✅ **Tách biệt rõ ràng**
- Main solution (`Web_SIMS.sln`) chỉ chứa production code
- Test solution (`Web_SIMS.Tests.sln`) chứa cả main project và test project
- Dễ quản lý và maintain

### ✅ **CI/CD friendly**
- Build pipeline có thể chạy riêng tests
- Không ảnh hưởng đến main build process
- Dễ setup automated testing

### ✅ **Development workflow**
- Developers có thể focus vào main code hoặc tests
- Test-driven development (TDD) dễ dàng hơn
- Parallel development giữa feature và tests

### ✅ **Performance**
- Build nhanh hơn khi chỉ cần main project
- Test execution độc lập
- Không load unnecessary dependencies

## 🔧 Commands hữu ích

### Build commands:
```bash
# Build chỉ main project
dotnet build Web_SIMS.csproj

# Build test solution (bao gồm cả main project)
dotnet build Web_SIMS.Tests.sln

# Build với configuration cụ thể
dotnet build Web_SIMS.Tests.sln --configuration Release
```

### Test commands:
```bash
# Chạy tất cả tests
dotnet test Web_SIMS.Tests.sln

# Chạy tests với verbosity
dotnet test Web_SIMS.Tests.sln --verbosity detailed

# Chạy tests cụ thể
dotnet test --filter "ClassName=AccountControllerTests"
dotnet test --filter "MethodName=Login_Post_WithValidCredentials"

# Chạy tests với code coverage
dotnet test Web_SIMS.Tests.sln --collect:"XPlat Code Coverage"
```

### Package management:
```bash
# Add package vào test project
dotnet add Web_SIMS.Tests/Web_SIMS.Tests.csproj package PackageName

# Restore packages cho solution
dotnet restore Web_SIMS.Tests.sln
```

## 📊 Test Coverage Report

### Tổng số tests: **41+ tests**

| Test Class | Tests | Chức năng |
|------------|-------|-----------|
| AccountControllerTests | 9 | Đăng nhập, đăng xuất, đổi mật khẩu |
| StudentControllerTests | 10 | Quản lý sinh viên |
| FacultyManagementControllerTests | 11 | Quản lý giảng viên |
| EnrollmentControllerTests | 11 | Đăng ký khóa học |

### Test scenarios được cover:
- ✅ **Happy path**: Các trường hợp thành công
- ✅ **Validation errors**: Lỗi dữ liệu đầu vào
- ✅ **Business logic errors**: Lỗi nghiệp vụ
- ✅ **Exception handling**: Xử lý ngoại lệ
- ✅ **Security scenarios**: Bảo mật và authorization
- ✅ **Edge cases**: Các trường hợp biên

## 🔍 Debugging Tests

### 1. Debug trong Visual Studio:
1. Đặt breakpoint trong test method
2. Right-click test trong Test Explorer
3. Chọn "Debug Selected Tests"
4. Step through code như bình thường

### 2. Debug từ Command Line:
```bash
# Chạy test với debug info
dotnet test --filter "MethodName=TestName" --verbosity diagnostic

# Chạy với logger
dotnet test --logger "console;verbosity=detailed"
```

### 3. Common debugging scenarios:
- **Mock không hoạt động**: Kiểm tra setup parameters
- **Authentication tests fail**: Verify ClaimsPrincipal setup
- **Controller tests fail**: Check HttpContext và TempData setup

## 📝 Thêm Tests mới

### 1. Tạo test class mới:
```csharp
namespace Web_SIMS.Tests.Controllers
{
    public class NewControllerTests
    {
        private readonly Mock<IService> _mockService;
        private readonly NewController _controller;

        public NewControllerTests()
        {
            _mockService = new Mock<IService>();
            _controller = new NewController(_mockService.Object);
            TestHelper.SetupTempData(_controller);
        }

        [Fact]
        public async Task Method_Scenario_ExpectedResult()
        {
            // Arrange
            // Act
            // Assert
        }
    }
}
```

### 2. Sử dụng TestHelper:
```csharp
// Tạo test data
var student = TestHelper.CreateTestStudent(1, "SV001", "Test Student");
var user = TestHelper.CreateTestUserEntity(1, "testuser", "Student", 3);

// Setup authentication
var claimsPrincipal = TestHelper.CreateTestUser(1, "testuser", "Student");
TestHelper.SetupHttpContext(_controller, claimsPrincipal);
```

## 🚨 Troubleshooting

### Problem: Tests không chạy
**Solution**:
```bash
# Kiểm tra build
dotnet build Web_SIMS.Tests.sln

# Kiểm tra dependencies
dotnet restore Web_SIMS.Tests.sln

# Kiểm tra test discovery
dotnet test --list-tests
```

### Problem: Mock không hoạt động
**Solution**:
```csharp
// Đảm bảo setup đúng
_mockService.Setup(x => x.Method(It.IsAny<Parameter>()))
           .ReturnsAsync(expectedResult);

// Verify được gọi
_mockService.Verify(x => x.Method(It.IsAny<Parameter>()), Times.Once);
```

### Problem: Authentication tests fail
**Solution**:
```csharp
// Setup ClaimsPrincipal đúng
var user = TestHelper.CreateTestUser(1, "testuser", "Admin");
TestHelper.SetupHttpContext(_controller, user);
```

## 📈 Best Practices

### 1. **Test Naming**:
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
```

### 2. **AAA Pattern**:
```csharp
// Arrange - Setup
// Act - Execute
// Assert - Verify
```

### 3. **Use TestHelper**:
```csharp
// Tái sử dụng common setup
var model = TestHelper.ViewModels.CreateStudentViewModel();
```

### 4. **Mock Verification**:
```csharp
// Verify service calls
_mockService.Verify(x => x.Method(), Times.Once);
```

---

## 🎉 Kết luận

Solution test riêng giúp:
- **Tách biệt** concerns rõ ràng
- **Dễ maintain** và scale
- **CI/CD** friendly
- **Better developer experience**

Chạy `build-and-test.bat` (Windows) hoặc `build-and-test.sh` (Linux/Mac) để bắt đầu!