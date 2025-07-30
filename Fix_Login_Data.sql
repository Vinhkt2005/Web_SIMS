-- Script sửa dữ liệu đăng nhập Web SIMS
USE Web_SIMS;
GO

-- 1. Xóa dữ liệu cũ nếu có
DELETE FROM AcademicRecords;
DELETE FROM Enrollments;
DELETE FROM Users;
DELETE FROM Students;
DELETE FROM Courses;
DELETE FROM Roles;
GO

-- 2. Thêm lại dữ liệu Roles
INSERT INTO Roles (RoleId, RoleName, Description, IsActive, CreatedDate) VALUES 
(1, 'Admin', 'Quản trị viên hệ thống', 1, GETDATE()),
(2, 'Faculty', 'Giảng viên', 1, GETDATE()),
(3, 'Student', 'Sinh viên', 1, GETDATE());
GO

-- 3. Thêm lại dữ liệu Students
INSERT INTO Students (StudentId, StudentCode, FullName, Email, PhoneNumber, DateOfBirth, Address, Gender, Major, AcademicYear, EnrollmentDate, IsActive, CreatedDate) VALUES 
(1, 'SV001', 'Nguyễn Văn A', 'nguyenvana@sims.edu', '0123456789', '2000-01-01', 'Hà Nội', 'Nam', 'Công nghệ thông tin', '2024-2025', GETDATE(), 1, GETDATE()),
(2, 'SV002', 'Trần Thị B', 'tranthib@sims.edu', '0987654321', '2001-05-15', 'TP.HCM', 'Nữ', 'Kinh tế', '2024-2025', GETDATE(), 1, GETDATE());
GO

-- 4. Thêm lại dữ liệu Users (với password plain text)
INSERT INTO Users (UserId, Username, Password, Email, FullName, RoleId, IsActive, CreatedDate) VALUES 
(1, 'admin', 'admin123', 'admin@sims.edu', 'Administrator', 1, 1, GETDATE()),
(2, 'faculty1', 'faculty123', 'faculty1@sims.edu', 'Giảng viên 1', 2, 1, GETDATE()),
(3, 'student1', 'student123', 'student1@sims.edu', 'Sinh viên 1', 3, 1, GETDATE());
GO

-- 5. Cập nhật StudentId cho user student1
UPDATE Users SET StudentId = 1 WHERE Username = 'student1';
GO

-- 6. Thêm lại dữ liệu Courses
INSERT INTO Courses (CourseId, CourseCode, CourseName, Description, Credits, MaxStudents, Semester, AcademicYear, Instructor, IsActive, CreatedDate) VALUES 
(1, 'CS101', 'Lập trình Cơ bản', 'Khóa học lập trình cơ bản cho sinh viên', 3, 50, 'Học kỳ 1', '2024-2025', 'TS. Nguyễn Văn Giảng', 1, GETDATE()),
(2, 'CS102', 'Cơ sở dữ liệu', 'Khóa học về cơ sở dữ liệu', 3, 40, 'Học kỳ 1', '2024-2025', 'TS. Trần Thị Giảng', 1, GETDATE());
GO

-- 7. Thêm lại dữ liệu Enrollments
INSERT INTO Enrollments (StudentId, CourseId, EnrollmentDate, Status, CreatedDate) VALUES 
(1, 1, GETDATE(), 'Approved', GETDATE()),
(1, 2, GETDATE(), 'Approved', GETDATE());
GO

-- 8. Kiểm tra dữ liệu
SELECT '=== KIỂM TRA DỮ LIỆU ===' as Info;
SELECT 'Roles' as TableName, COUNT(*) as RecordCount FROM Roles
UNION ALL
SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM Users
UNION ALL
SELECT 'Students' as TableName, COUNT(*) as RecordCount FROM Students
UNION ALL
SELECT 'Courses' as TableName, COUNT(*) as RecordCount FROM Courses
UNION ALL
SELECT 'Enrollments' as TableName, COUNT(*) as RecordCount FROM Enrollments;

-- 9. Hiển thị thông tin đăng nhập
SELECT '=== THÔNG TIN ĐĂNG NHẬP ===' as Info;
SELECT Username, Password, FullName, r.RoleName, u.IsActive
FROM Users u 
JOIN Roles r ON u.RoleId = r.RoleId 
WHERE u.IsActive = 1
ORDER BY u.UserId; 