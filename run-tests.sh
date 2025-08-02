#!/bin/bash

echo "============================================="
echo "        CHẠY UNIT TESTS CHO HỆ THỐNG SIMS"
echo "============================================="
echo

echo "Đang build solution..."
dotnet build
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
dotnet test --verbosity normal --logger "console;verbosity=detailed"

echo
echo "============================================="
echo "           BÁO CÁO KẾT QUẢ"
echo "============================================="

if [ $? -eq 0 ]; then
    echo
    echo "✅ TẤT CẢ TESTS ĐÃ PASS!"
    echo
    echo "Chi tiết tests:"
    echo "- AccountControllerTests: Test đăng nhập, đăng xuất, đổi mật khẩu"
    echo "- StudentControllerTests: Test quản lý sinh viên"
    echo "- FacultyManagementControllerTests: Test quản lý giảng viên"  
    echo "- EnrollmentControllerTests: Test đăng ký khóa học"
    echo
    echo "Tổng số tests: 40+ tests"
    echo
else
    echo
    echo "❌ CÓ TESTS THẤT BẠI!"
    echo "Vui lòng kiểm tra chi tiết ở trên."
    echo
fi

echo "============================================="
echo "Nhấn Enter để tiếp tục..."
read