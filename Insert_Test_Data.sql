-- Thêm dữ liệu test thủ công
USE Web_SIMS;

-- Thêm Roles
INSERT INTO Roles (RoleName, Description, IsActive) VALUES 
('Admin', 'Quản trị viên hệ thống', 1),
('Faculty', 'Giảng viên', 1),
('Student', 'Sinh viên', 1);

-- Thêm Students
INSERT INTO Students (StudentCode, FullName, Email, PhoneNumber, DateOfBirth, Address, Gender, Major, AcademicYear, EnrollmentDate, IsActive, CreatedDate) VALUES 
('SV001', 'Nguyễn Văn A', 'nguyenvana@sims.edu', '0123456789', '2000-01-01', 'Hà Nội', 'Nam', 'Công nghệ thông tin', '2024-2025', GETDATE(), 1, GETDATE()),
('SV002', 'Trần Thị B', 'tranthib@sims.edu', '0987654321', '2001-05-15', 'TP.HCM', 'Nữ', 'Kinh tế', '2024-2025', GETDATE(), 1, GETDATE());

-- Thêm Users (plain text passwords để test)
INSERT INTO Users (Username, Password, Email, FullName, RoleId, StudentId, IsActive, CreatedDate) VALUES 
('admin', 'admin123', 'admin@sims.edu', 'Administrator', 1, NULL, 1, GETDATE()),
('faculty1', 'faculty123', 'faculty1@sims.edu', 'Giảng viên 1', 2, NULL, 1, GETDATE()),
('student1', 'student123', 'student1@sims.edu', 'Sinh viên 1', 3, 1, 1, GETDATE());

-- Kiểm tra dữ liệu đã thêm
SELECT 'Roles' as TableName, COUNT(*) as Count FROM Roles
UNION ALL
SELECT 'Students', COUNT(*) FROM Students  
UNION ALL
SELECT 'Users', COUNT(*) FROM Users;

-- Kiểm tra user admin
SELECT Username, Password, RoleId, IsActive FROM Users WHERE Username = 'admin';