@echo off
echo =============================================
echo     BUILD VA TEST SOLUTION WEB_SIMS.TESTS
echo =============================================
echo.

echo Dang clean solution...
dotnet clean Web_SIMS.Tests.sln
if %errorlevel% neq 0 (
    echo Clean that bai!
    pause
    exit /b %errorlevel%
)

echo.
echo Dang restore packages...
dotnet restore Web_SIMS.Tests.sln
if %errorlevel% neq 0 (
    echo Restore packages that bai!
    pause
    exit /b %errorlevel%
)

echo.
echo Dang build solution...
dotnet build Web_SIMS.Tests.sln --configuration Debug
if %errorlevel% neq 0 (
    echo Build that bai! Vui long kiem tra loi.
    pause
    exit /b %errorlevel%
)

echo.
echo Build thanh cong! Dang chay tests...
echo.

echo =============================================
echo           CHAY TAT CA TESTS
echo =============================================
dotnet test Web_SIMS.Tests.sln --configuration Debug --verbosity normal --logger "console;verbosity=detailed"

echo.
echo =============================================
echo           BAO CAO CHI TIET
echo =============================================

if %errorlevel% equ 0 (
    echo.
    echo ✅ TAT CA TESTS DA PASS THANH CONG!
    echo.
    echo 📊 THONG KE TESTS:
    echo ├── AccountControllerTests: 9 tests (Dang nhap, dang xuat, doi mat khau)
    echo ├── StudentControllerTests: 10 tests (Quan ly sinh vien)
    echo ├── FacultyManagementControllerTests: 11 tests (Quan ly giang vien)
    echo └── EnrollmentControllerTests: 11 tests (Dang ky khoa hoc)
    echo.
    echo 🎯 Tong cong: 41+ unit tests
    echo.
    echo 🔧 CACH SU DUNG:
    echo - Mo Visual Studio va load Web_SIMS.Tests.sln
    echo - Hoac chay: dotnet test Web_SIMS.Tests.sln
    echo.
) else (
    echo.
    echo ❌ CO TESTS THAT BAI!
    echo.
    echo 🔍 CACH DEBUG:
    echo 1. Kiem tra loi chi tiet o tren
    echo 2. Chay test cu the: dotnet test --filter "ClassName=TestClassName"
    echo 3. Mo Visual Studio va debug test
    echo.
)

echo =============================================
echo 📁 CAU TRUC SOLUTION:
echo Web_SIMS.Tests.sln
echo ├── Web_SIMS (Main project)
echo └── Web_SIMS.Tests (Test project)
echo     ├── Controllers/
echo     │   ├── AccountControllerTests.cs
echo     │   ├── StudentControllerTests.cs
echo     │   ├── FacultyManagementControllerTests.cs
echo     │   └── EnrollmentControllerTests.cs
echo     ├── TestHelper.cs
echo     └── README.md
echo =============================================

echo.
echo Nhan phim bat ky de dong...
pause