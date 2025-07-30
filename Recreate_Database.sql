-- Script tạo lại database Web SIMS
-- Chạy script này nếu database bị xóa

-- 1. Tạo database nếu chưa có
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Web_SIMS')
BEGIN
    CREATE DATABASE Web_SIMS;
    PRINT 'Database Web_SIMS đã được tạo!';
END
ELSE
BEGIN
    PRINT 'Database Web_SIMS đã tồn tại!';
END
GO

USE Web_SIMS;
GO

-- 2. Xóa bảng cũ nếu có (để tạo lại sạch)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicRecords')
    DROP TABLE AcademicRecords;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Enrollments')
    DROP TABLE Enrollments;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
    DROP TABLE Users;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
    DROP TABLE Students;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Courses')
    DROP TABLE Courses;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
    DROP TABLE Roles;
GO

-- 3. Tạo bảng Roles
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200),
    IsActive BIT DEFAULT 1
);
GO

-- 4. Tạo bảng Students
CREATE TABLE Students (
    StudentId INT PRIMARY KEY IDENTITY(1,1),
    StudentCode NVARCHAR(20) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20),
    DateOfBirth DATETIME,
    Address NVARCHAR(200),
    Gender NVARCHAR(10),
    Major NVARCHAR(100),
    AcademicYear NVARCHAR(20),
    EnrollmentDate DATETIME,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME,
    Notes NVARCHAR(500)
);
GO

-- 5. Tạo bảng Courses
CREATE TABLE Courses (
    CourseId INT PRIMARY KEY IDENTITY(1,1),
    CourseCode NVARCHAR(20) NOT NULL UNIQUE,
    CourseName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    Credits INT DEFAULT 3,
    MaxStudents INT DEFAULT 50,
    Semester NVARCHAR(20),
    AcademicYear NVARCHAR(20),
    Instructor NVARCHAR(100),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME
);
GO

-- 6. Tạo bảng Users
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(100) NOT NULL,
    PasswordSalt NVARCHAR(50),
    Email NVARCHAR(100) NOT NULL UNIQUE,
    FullName NVARCHAR(100),
    RoleId INT,
    StudentId INT,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    FOREIGN KEY (StudentId) REFERENCES Students(StudentId)
);
GO

-- 7. Tạo bảng Enrollments
CREATE TABLE Enrollments (
    EnrollmentId INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT,
    CourseId INT,
    EnrollmentDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Pending',
    Notes NVARCHAR(500),
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME,
    FOREIGN KEY (StudentId) REFERENCES Students(StudentId),
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
);
GO

-- 8. Tạo bảng AcademicRecords
CREATE TABLE AcademicRecords (
    AcademicRecordId INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT,
    CourseId INT,
    MidtermScore DECIMAL(5,2),
    FinalScore DECIMAL(5,2),
    TotalScore DECIMAL(5,2),
    Grade NVARCHAR(2),
    AcademicYear INT,
    Semester INT,
    Notes NVARCHAR(500),
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME,
    FOREIGN KEY (StudentId) REFERENCES Students(StudentId),
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
);
GO

-- 9. Thêm dữ liệu mẫu cho Roles
INSERT INTO Roles (RoleId, RoleName, Description, IsActive) VALUES 
(1, 'Admin', 'Quản trị viên hệ thống', 1),
(2, 'Faculty', 'Giảng viên', 1),
(3, 'Student', 'Sinh viên', 1);
GO

-- 10. Thêm dữ liệu mẫu cho Students
INSERT INTO Students (StudentId, StudentCode, FullName, Email, PhoneNumber, DateOfBirth, Address, Gender, Major, AcademicYear, EnrollmentDate, IsActive, CreatedDate) VALUES 
(1, 'SV001', 'Nguyễn Văn A', 'nguyenvana@sims.edu', '0123456789', '2000-01-01', 'Hà Nội', 'Nam', 'Công nghệ thông tin', '2024-2025', GETDATE(), 1, GETDATE()),
(2, 'SV002', 'Trần Thị B', 'tranthib@sims.edu', '0987654321', '2001-05-15', 'TP.HCM', 'Nữ', 'Kinh tế', '2024-2025', GETDATE(), 1, GETDATE());
GO

-- 11. Thêm dữ liệu mẫu cho Courses
INSERT INTO Courses (CourseId, CourseCode, CourseName, Description, Credits, MaxStudents, Semester, AcademicYear, Instructor, IsActive, CreatedDate) VALUES 
(1, 'CS101', 'Lập trình Cơ bản', 'Khóa học lập trình cơ bản cho sinh viên', 3, 50, 'Học kỳ 1', '2024-2025', 'TS. Nguyễn Văn Giảng', 1, GETDATE()),
(2, 'CS102', 'Cơ sở dữ liệu', 'Khóa học về cơ sở dữ liệu', 3, 40, 'Học kỳ 1', '2024-2025', 'TS. Trần Thị Giảng', 1, GETDATE());
GO

-- 12. Thêm dữ liệu mẫu cho Users
INSERT INTO Users (UserId, Username, Password, Email, FullName, RoleId, IsActive, CreatedDate) VALUES 
(1, 'admin', 'admin123', 'admin@sims.edu', 'Administrator', 1, 1, GETDATE()),
(2, 'faculty1', 'faculty123', 'faculty1@sims.edu', 'Giảng viên 1', 2, 1, GETDATE()),
(3, 'student1', 'student123', 'student1@sims.edu', 'Sinh viên 1', 3, 1, GETDATE());
GO

-- 13. Cập nhật StudentId cho user student1
UPDATE Users SET StudentId = 1 WHERE Username = 'student1';
GO

-- 14. Thêm dữ liệu mẫu cho Enrollments
INSERT INTO Enrollments (StudentId, CourseId, EnrollmentDate, Status, CreatedDate) VALUES 
(1, 1, GETDATE(), 'Approved', GETDATE()),
(1, 2, GETDATE(), 'Approved', GETDATE());
GO

-- 15. Kiểm tra dữ liệu
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

-- 16. Hiển thị thông tin đăng nhập
SELECT '=== THÔNG TIN ĐĂNG NHẬP ===' as Info;
SELECT 
    u.Username, 
    u.Password, 
    u.FullName, 
    r.RoleName, 
    u.IsActive,
    CASE 
        WHEN u.IsActive = 1 THEN '✓ Hoạt động'
        ELSE '✗ Không hoạt động'
    END as Status
FROM Users u 
JOIN Roles r ON u.RoleId = r.RoleId 
ORDER BY u.UserId;

PRINT '=== DATABASE ĐÃ ĐƯỢC TẠO LẠI THÀNH CÔNG ===';
PRINT 'Bây giờ bạn có thể test đăng nhập với:';
PRINT 'Username: faculty1, Password: faculty123';
PRINT 'Username: admin, Password: admin123';
PRINT 'Username: student1, Password: student123'; 