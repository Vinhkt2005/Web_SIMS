@echo off
echo =============================================
echo        CHAY UNIT TESTS CHO HE THONG SIMS
echo =============================================
echo.

echo Dang build solution...
dotnet build
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
dotnet test --verbosity normal --logger "console;verbosity=detailed"

echo.
echo =============================================
echo           BAO CAO KET QUA
echo =============================================

if %errorlevel% equ 0 (
    echo.
    echo ✅ TAT CA TESTS DA PASS!
    echo.
    echo Chi tiet tests:
    echo - AccountControllerTests: Test dang nhap, dang xuat, doi mat khau
    echo - StudentControllerTests: Test quan ly sinh vien
    echo - FacultyManagementControllerTests: Test quan ly giang vien  
    echo - EnrollmentControllerTests: Test dang ky khoa hoc
    echo.
    echo Tong so tests: 40+ tests
    echo.
) else (
    echo.
    echo ❌ CO TESTS THAT BAI!
    echo Vui long kiem tra chi tiet o tren.
    echo.
)

echo =============================================
echo Nhan phim bat ky de dong...
pause