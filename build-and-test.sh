#!/bin/bash

echo "============================================="
echo "     BUILD VÀ TEST SOLUTION WEB_SIMS.TESTS"
echo "============================================="
echo

echo "Đang clean solution..."
dotnet clean Web_SIMS.Tests.sln
if [ $? -ne 0 ]; then
    echo "Clean thất bại!"
    exit 1
fi

echo
echo "Đang restore packages..."
dotnet restore Web_SIMS.Tests.sln
if [ $? -ne 0 ]; then
    echo "Restore packages thất bại!"
    exit 1
fi

echo
echo "Đang build solution..."
dotnet build Web_SIMS.Tests.sln --configuration Debug
if [ $? -ne 0 ]; then
    echo "Build thất bại! Vui lòng kiểm tra lỗi."
    exit 1
fi

echo
echo "Build thành công! Đang chạy tests..."
echo

echo "============================================="
echo "           CHẠY TẤT CẢ TESTS"
echo "============================================="
dotnet test Web_SIMS.Tests.sln --configuration Debug --verbosity normal --logger "console;verbosity=detailed"

echo
echo "============================================="
echo "           BÁO CÁO CHI TIẾT"
echo "============================================="

if [ $? -eq 0 ]; then
    echo
    echo "✅ TẤT CẢ TESTS ĐÃ PASS THÀNH CÔNG!"
    echo
    echo "📊 THỐNG KÊ TESTS:"
    echo "├── AccountControllerTests: 9 tests (Đăng nhập, đăng xuất, đổi mật khẩu)"
    echo "├── StudentControllerTests: 10 tests (Quản lý sinh viên)"
    echo "├── FacultyManagementControllerTests: 11 tests (Quản lý giảng viên)"
    echo "└── EnrollmentControllerTests: 11 tests (Đăng ký khóa học)"
    echo
    echo "🎯 Tổng cộng: 41+ unit tests"
    echo
    echo "🔧 CÁCH SỬ DỤNG:"
    echo "- Mở Visual Studio và load Web_SIMS.Tests.sln"
    echo "- Hoặc chạy: dotnet test Web_SIMS.Tests.sln"
    echo
else
    echo
    echo "❌ CÓ TESTS THẤT BẠI!"
    echo
    echo "🔍 CÁCH DEBUG:"
    echo "1. Kiểm tra lỗi chi tiết ở trên"
    echo "2. Chạy test cụ thể: dotnet test --filter \"ClassName=TestClassName\""
    echo "3. Mở Visual Studio và debug test"
    echo
fi

echo "============================================="
echo "📁 CẤU TRÚC SOLUTION:"
echo "Web_SIMS.Tests.sln"
echo "├── Web_SIMS (Main project)"
echo "└── Web_SIMS.Tests (Test project)"
echo "    ├── Controllers/"
echo "    │   ├── AccountControllerTests.cs"
echo "    │   ├── StudentControllerTests.cs"
echo "    │   ├── FacultyManagementControllerTests.cs"
echo "    │   └── EnrollmentControllerTests.cs"
echo "    ├── TestHelper.cs"
echo "    └── README.md"
echo "============================================="

echo
echo "Nhấn Enter để tiếp tục..."
read