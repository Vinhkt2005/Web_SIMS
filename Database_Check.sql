-- Script kiểm tra và sửa database Web SIMS
-- Chạy script này để đảm bảo database hoạt động đúng

-- 1. Kiểm tra database có tồn tại không
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Web_SIMS')
BEGIN
    CREATE DATABASE Web_SIMS;
END
GO

USE Web_SIMS;
GO

-- 2. Kiểm tra và tạo bảng Roles nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        RoleId INT PRIMARY KEY IDENTITY(1,1),
        RoleName NVARCHAR(50) NOT NULL,
        Description NVARCHAR(200),
        IsActive BIT DEFAULT 1,
        CreatedDate DATETIME DEFAULT GETDATE(),
        UpdatedDate DATETIME
    );
END
GO

-- 3. Kiểm tra và tạo bảng Users nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
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
        FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
    );
END
GO

-- 4. Kiểm tra và tạo bảng Students nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
BEGIN
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
        UpdatedDate DATETIME
    );
END
GO

-- 5. Kiểm tra và tạo bảng Courses nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Courses')
BEGIN
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
END
GO

-- 6. Kiểm tra và tạo bảng Enrollments nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Enrollments')
BEGIN
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
END
GO

-- 7. Kiểm tra và tạo bảng AcademicRecords nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AcademicRecords')
BEGIN
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
END
GO

-- 8. Thêm dữ liệu mẫu cho Roles
IF NOT EXISTS (SELECT * FROM Roles WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO Roles (RoleName, Description, IsActive) VALUES ('Admin', 'Quản trị viên hệ thống', 1);
END

IF NOT EXISTS (SELECT * FROM Roles WHERE RoleName = 'Faculty')
BEGIN
    INSERT INTO Roles (RoleName, Description, IsActive) VALUES ('Faculty', 'Giảng viên', 1);
END

IF NOT EXISTS (SELECT * FROM Roles WHERE RoleName = 'Student')
BEGIN
    INSERT INTO Roles (RoleName, Description, IsActive) VALUES ('Student', 'Sinh viên', 1);
END

-- 9. Thêm dữ liệu mẫu cho Users
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, Password, Email, FullName, RoleId, IsActive) 
    VALUES ('admin', 'admin123', 'admin@sims.edu', 'Administrator', 1, 1);
END

IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'faculty1')
BEGIN
    INSERT INTO Users (Username, Password, Email, FullName, RoleId, IsActive) 
    VALUES ('faculty1', 'faculty123', 'faculty1@sims.edu', 'Giảng viên 1', 2, 1);
END

IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'student1')
BEGIN
    INSERT INTO Users (Username, Password, Email, FullName, RoleId, StudentId, IsActive) 
    VALUES ('student1', 'student123', 'student1@sims.edu', 'Sinh viên 1', 3, 1, 1);
END

-- 10. Thêm dữ liệu mẫu cho Students
IF NOT EXISTS (SELECT * FROM Students WHERE StudentCode = 'SV001')
BEGIN
    INSERT INTO Students (StudentCode, FullName, Email, PhoneNumber, DateOfBirth, Address, Gender, Major, AcademicYear, IsActive) 
    VALUES ('SV001', 'Nguyễn Văn A', 'nguyenvana@sims.edu', '0123456789', '2000-01-01', 'Hà Nội', 'Nam', 'Công nghệ thông tin', '2024-2025', 1);
END

IF NOT EXISTS (SELECT * FROM Students WHERE StudentCode = 'SV002')
BEGIN
    INSERT INTO Students (StudentCode, FullName, Email, PhoneNumber, DateOfBirth, Address, Gender, Major, AcademicYear, IsActive) 
    VALUES ('SV002', 'Trần Thị B', 'tranthib@sims.edu', '0987654321', '2001-05-15', 'TP.HCM', 'Nữ', 'Kinh tế', '2024-2025', 1);
END

-- 11. Thêm dữ liệu mẫu cho Courses
IF NOT EXISTS (SELECT * FROM Courses WHERE CourseCode = 'CS101')
BEGIN
    INSERT INTO Courses (CourseCode, CourseName, Description, Credits, MaxStudents, Semester, AcademicYear, Instructor, IsActive) 
    VALUES ('CS101', 'Lập trình Cơ bản', 'Khóa học lập trình cơ bản cho sinh viên', 3, 50, 'Học kỳ 1', '2024-2025', 'TS. Nguyễn Văn Giảng', 1);
END

IF NOT EXISTS (SELECT * FROM Courses WHERE CourseCode = 'CS102')
BEGIN
    INSERT INTO Courses (CourseCode, CourseName, Description, Credits, MaxStudents, Semester, AcademicYear, Instructor, IsActive) 
    VALUES ('CS102', 'Cơ sở dữ liệu', 'Khóa học về cơ sở dữ liệu', 3, 40, 'Học kỳ 1', '2024-2025', 'TS. Trần Thị Giảng', 1);
END

-- 12. Thêm dữ liệu mẫu cho Enrollments
IF NOT EXISTS (SELECT * FROM Enrollments WHERE StudentId = 1 AND CourseId = 1)
BEGIN
    INSERT INTO Enrollments (StudentId, CourseId, Status) VALUES (1, 1, 'Approved');
END

IF NOT EXISTS (SELECT * FROM Enrollments WHERE StudentId = 1 AND CourseId = 2)
BEGIN
    INSERT INTO Enrollments (StudentId, CourseId, Status) VALUES (1, 2, 'Approved');
END

-- 13. Kiểm tra dữ liệu
SELECT 'Roles' as TableName, COUNT(*) as RecordCount FROM Roles
UNION ALL
SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM Users
UNION ALL
SELECT 'Students' as TableName, COUNT(*) as RecordCount FROM Students
UNION ALL
SELECT 'Courses' as TableName, COUNT(*) as RecordCount FROM Courses
UNION ALL
SELECT 'Enrollments' as TableName, COUNT(*) as RecordCount FROM Enrollments
UNION ALL
SELECT 'AcademicRecords' as TableName, COUNT(*) as RecordCount FROM AcademicRecords;

-- 14. Hiển thị thông tin đăng nhập
SELECT '=== THÔNG TIN ĐĂNG NHẬP ===' as Info;
SELECT Username, Password, FullName, r.RoleName 
FROM Users u 
JOIN Roles r ON u.RoleId = r.RoleId 
WHERE u.IsActive = 1; 