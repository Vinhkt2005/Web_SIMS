-- =============================================
-- Script Reset và Recreate Database cho hệ thống SIMS
-- Phiên bản: 1.0
-- Ngày tạo: 2025-08-04
-- Mô tả: Script để reset hoàn toàn database và tạo lại từ đầu
-- CẢNH BÁO: Script này sẽ XÓA HOÀN TOÀN database và tất cả dữ liệu!
-- =============================================

USE master
GO

-- =============================================
-- 1. BACKUP DATABASE TRƯỚC KHI XÓA (TÙY CHỌN)
-- =============================================

-- Uncomment phần này nếu muốn backup trước khi xóa
/*
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Web_SIMS')
BEGIN
    DECLARE @BackupPath NVARCHAR(500) = 'C:\DatabaseBackups\'
    DECLARE @BackupFileName NVARCHAR(500)
    DECLARE @DateTime NVARCHAR(20) = REPLACE(REPLACE(REPLACE(CONVERT(VARCHAR, GETDATE(), 120), '-', ''), ':', ''), ' ', '_')
    
    -- Tạo thư mục backup nếu chưa tồn tại
    EXEC xp_create_subdir @BackupPath
    
    SET @BackupFileName = @BackupPath + 'Web_SIMS_BeforeReset_' + @DateTime + '.bak'
    
    PRINT 'Đang backup database trước khi reset...'
    PRINT 'File backup: ' + @BackupFileName
    
    BACKUP DATABASE [Web_SIMS] 
    TO DISK = @BackupFileName
    WITH 
        FORMAT,
        INIT,
        NAME = 'Web_SIMS Backup Before Reset',
        COMPRESSION,
        CHECKSUM
    
    PRINT 'Backup hoàn thành!'
END
*/

-- =============================================
-- 2. ĐÓNG TẤT CẢ KẾT NỐI ĐẾN DATABASE
-- =============================================

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Web_SIMS')
BEGIN
    PRINT 'Đang đóng tất cả kết nối đến database Web_SIMS...'
    
    ALTER DATABASE [Web_SIMS] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
    
    PRINT 'Đã đóng tất cả kết nối!'
END

-- =============================================
-- 3. XÓA DATABASE
-- =============================================

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Web_SIMS')
BEGIN
    PRINT 'Đang xóa database Web_SIMS...'
    
    DROP DATABASE [Web_SIMS]
    
    PRINT 'Database đã được xóa!'
END
ELSE
BEGIN
    PRINT 'Database Web_SIMS không tồn tại!'
END

-- =============================================
-- 4. TẠO LẠI DATABASE MỚI
-- =============================================

PRINT 'Đang tạo database mới...'

CREATE DATABASE [Web_SIMS]
ON 
( NAME = N'Web_SIMS', 
  FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\Web_SIMS.mdf',
  SIZE = 100MB,
  MAXSIZE = 1GB,
  FILEGROWTH = 10MB )
LOG ON 
( NAME = N'Web_SIMS_Log',
  FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\Web_SIMS_Log.ldf',
  SIZE = 10MB,
  MAXSIZE = 100MB,
  FILEGROWTH = 5MB )
GO

-- Thiết lập collation và các tùy chọn database
ALTER DATABASE [Web_SIMS] SET RECOVERY SIMPLE
ALTER DATABASE [Web_SIMS] SET AUTO_CLOSE OFF
ALTER DATABASE [Web_SIMS] SET AUTO_SHRINK OFF
ALTER DATABASE [Web_SIMS] SET AUTO_CREATE_STATISTICS ON
ALTER DATABASE [Web_SIMS] SET AUTO_UPDATE_STATISTICS ON
GO

PRINT 'Database mới đã được tạo!'

-- =============================================
-- 5. CHẠY SCRIPT TẠO CẤU TRÚC HOÀN CHỈNH
-- =============================================

USE [Web_SIMS]
GO

PRINT 'Đang tạo cấu trúc database...'

-- Bảng Roles
CREATE TABLE [dbo].[Roles] (
    [RoleId] int IDENTITY(1,1) NOT NULL,
    [RoleName] nvarchar(50) NOT NULL,
    [Description] nvarchar(200) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2(7) NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] datetime2(7) NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([RoleId] ASC)
)

-- Bảng Students
CREATE TABLE [dbo].[Students] (
    [StudentId] int IDENTITY(1,1) NOT NULL,
    [StudentCode] nvarchar(20) NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [DateOfBirth] datetime2(7) NULL,
    [Address] nvarchar(200) NULL,
    [Gender] nvarchar(10) NULL,
    [Major] nvarchar(100) NULL,
    [AcademicYear] nvarchar(20) NULL,
    [EnrollmentDate] datetime2(7) NOT NULL DEFAULT GETDATE(),
    [IsActive] bit NOT NULL DEFAULT 1,
    [Notes] nvarchar(500) NULL,
    [CreatedDate] datetime2(7) NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] datetime2(7) NULL,
    CONSTRAINT [PK_Students] PRIMARY KEY CLUSTERED ([StudentId] ASC)
)

-- Bảng Faculties
CREATE TABLE [dbo].[Faculties] (
    [FacultyId] int IDENTITY(1,1) NOT NULL,
    [FacultyCode] nvarchar(20) NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [PhoneNumber] nvarchar(15) NULL,
    [Address] nvarchar(200) NULL,
    [Department] nvarchar(100) NULL,
    [Specialization] nvarchar(100) NULL,
    [DateOfBirth] datetime2(7) NULL,
    [Gender] nvarchar(10) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2(7) NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] datetime2(7) NULL,
    CONSTRAINT [PK_Faculties] PRIMARY KEY CLUSTERED ([FacultyId] ASC)
)

-- Bảng Users
CREATE TABLE [dbo].[Users] (
    [UserId] int IDENTITY(1,1) NOT NULL,
    [Username] nvarchar(50) NOT NULL,
    [Password] nvarchar(100) NOT NULL,
    [PasswordSalt] nvarchar(50) NULL,
    [Email] nvarchar(100) NOT NULL,
    [FullName] nvarchar(100) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2(7) NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] datetime2(7) NULL,
    [RoleId] int NOT NULL,
    [StudentId] int NULL,
    [FacultyId] int NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserId] ASC)
)

-- Bảng Courses
CREATE TABLE [dbo].[Courses] (
    [CourseId] int IDENTITY(1,1) NOT NULL,
    [CourseCode] nvarchar(20) NOT NULL,
    [CourseName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Credits] int NOT NULL DEFAULT 0,
    [MaxStudents] int NOT NULL DEFAULT 0,
    [Semester] nvarchar(20) NULL,
    [AcademicYear] nvarchar(20) NULL,
    [Instructor] nvarchar(100) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedDate] datetime2(7) NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] datetime2(7) NULL,
    [FacultyId] int NOT NULL,
    CONSTRAINT [PK_Courses] PRIMARY KEY CLUSTERED ([CourseId] ASC)
)

-- Bảng Enrollments
CREATE TABLE [dbo].[Enrollments] (
    [EnrollmentId] int IDENTITY(1,1) NOT NULL,
    [StudentId] int NOT NULL,
    [CourseId] int NOT NULL,
    [EnrollmentDate] datetime2(7) NOT NULL DEFAULT GETDATE(),
    [Status] int NOT NULL DEFAULT 0,
    [Notes] nvarchar(500) NULL,
    [CreatedDate] datetime2(7) NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] datetime2(7) NULL,
    CONSTRAINT [PK_Enrollments] PRIMARY KEY CLUSTERED ([EnrollmentId] ASC)
)

-- Bảng AcademicRecords
CREATE TABLE [dbo].[AcademicRecords] (
    [AcademicRecordId] int IDENTITY(1,1) NOT NULL,
    [StudentId] int NOT NULL,
    [CourseId] int NOT NULL,
    [Semester] int NOT NULL,
    [AcademicYear] int NOT NULL,
    [MidtermScore] decimal(5,2) NULL,
    [FinalScore] decimal(5,2) NULL,
    [TotalScore] decimal(5,2) NULL,
    [Grade] nvarchar(2) NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedDate] datetime2(7) NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] datetime2(7) NULL,
    CONSTRAINT [PK_AcademicRecords] PRIMARY KEY CLUSTERED ([AcademicRecordId] ASC)
)

-- Tạo Indexes
CREATE UNIQUE NONCLUSTERED INDEX [IX_Students_StudentCode] ON [dbo].[Students] ([StudentCode] ASC)
CREATE UNIQUE NONCLUSTERED INDEX [IX_Students_Email] ON [dbo].[Students] ([Email] ASC)
CREATE UNIQUE NONCLUSTERED INDEX [IX_Faculties_FacultyCode] ON [dbo].[Faculties] ([FacultyCode] ASC)
CREATE UNIQUE NONCLUSTERED INDEX [IX_Faculties_Email] ON [dbo].[Faculties] ([Email] ASC)
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Username] ON [dbo].[Users] ([Username] ASC)
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users] ([Email] ASC)
CREATE NONCLUSTERED INDEX [IX_Users_RoleId] ON [dbo].[Users] ([RoleId] ASC)
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_StudentId] ON [dbo].[Users] ([StudentId] ASC) WHERE [StudentId] IS NOT NULL
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_FacultyId] ON [dbo].[Users] ([FacultyId] ASC) WHERE [FacultyId] IS NOT NULL
CREATE UNIQUE NONCLUSTERED INDEX [IX_Courses_CourseCode] ON [dbo].[Courses] ([CourseCode] ASC)
CREATE NONCLUSTERED INDEX [IX_Courses_FacultyId] ON [dbo].[Courses] ([FacultyId] ASC)
CREATE NONCLUSTERED INDEX [IX_Enrollments_StudentId] ON [dbo].[Enrollments] ([StudentId] ASC)
CREATE NONCLUSTERED INDEX [IX_Enrollments_CourseId] ON [dbo].[Enrollments] ([CourseId] ASC)
CREATE NONCLUSTERED INDEX [IX_AcademicRecords_StudentId] ON [dbo].[AcademicRecords] ([StudentId] ASC)
CREATE NONCLUSTERED INDEX [IX_AcademicRecords_CourseId] ON [dbo].[AcademicRecords] ([CourseId] ASC)

-- Tạo Foreign Keys
ALTER TABLE [dbo].[Users] ADD CONSTRAINT [FK_Users_Roles_RoleId] 
    FOREIGN KEY([RoleId]) REFERENCES [dbo].[Roles] ([RoleId]) ON DELETE NO ACTION

ALTER TABLE [dbo].[Users] ADD CONSTRAINT [FK_Users_Students_StudentId] 
    FOREIGN KEY([StudentId]) REFERENCES [dbo].[Students] ([StudentId]) ON DELETE NO ACTION

ALTER TABLE [dbo].[Users] ADD CONSTRAINT [FK_Users_Faculties_FacultyId] 
    FOREIGN KEY([FacultyId]) REFERENCES [dbo].[Faculties] ([FacultyId]) ON DELETE NO ACTION

ALTER TABLE [dbo].[Courses] ADD CONSTRAINT [FK_Courses_Faculties_FacultyId] 
    FOREIGN KEY([FacultyId]) REFERENCES [dbo].[Faculties] ([FacultyId]) ON DELETE NO ACTION

ALTER TABLE [dbo].[Enrollments] ADD CONSTRAINT [FK_Enrollments_Students_StudentId] 
    FOREIGN KEY([StudentId]) REFERENCES [dbo].[Students] ([StudentId]) ON DELETE CASCADE

ALTER TABLE [dbo].[Enrollments] ADD CONSTRAINT [FK_Enrollments_Courses_CourseId] 
    FOREIGN KEY([CourseId]) REFERENCES [dbo].[Courses] ([CourseId]) ON DELETE CASCADE

ALTER TABLE [dbo].[AcademicRecords] ADD CONSTRAINT [FK_AcademicRecords_Students_StudentId] 
    FOREIGN KEY([StudentId]) REFERENCES [dbo].[Students] ([StudentId]) ON DELETE CASCADE

ALTER TABLE [dbo].[AcademicRecords] ADD CONSTRAINT [FK_AcademicRecords_Courses_CourseId] 
    FOREIGN KEY([CourseId]) REFERENCES [dbo].[Courses] ([CourseId]) ON DELETE CASCADE

PRINT 'Cấu trúc database đã được tạo!'

-- =============================================
-- 6. INSERT DỮ LIỆU MẪU CƠ BẢN
-- =============================================

PRINT 'Đang thêm dữ liệu mẫu...'

-- Insert Roles
SET IDENTITY_INSERT [dbo].[Roles] ON
INSERT INTO [dbo].[Roles] ([RoleId], [RoleName], [Description], [IsActive])
VALUES 
    (1, N'Admin', N'Quản trị viên hệ thống', 1),
    (2, N'Faculty', N'Giảng viên', 1),
    (3, N'Student', N'Sinh viên', 1)
SET IDENTITY_INSERT [dbo].[Roles] OFF

-- Insert Students
SET IDENTITY_INSERT [dbo].[Students] ON
INSERT INTO [dbo].[Students] ([StudentId], [StudentCode], [FullName], [Email], [PhoneNumber], [DateOfBirth], [Address], [Gender], [Major], [AcademicYear], [EnrollmentDate], [IsActive], [CreatedDate])
VALUES 
    (1, N'SV001', N'Nguyễn Văn A', N'nguyenvana@sims.edu', N'0123456789', '2000-01-01', N'Hà Nội', N'Nam', N'Công nghệ thông tin', N'2024-2025', '2024-01-01', 1, '2024-01-01'),
    (2, N'SV002', N'Trần Thị B', N'tranthib@sims.edu', N'0987654321', '2001-05-15', N'TP.HCM', N'Nữ', N'Kinh tế', N'2024-2025', '2024-01-01', 1, '2024-01-01')
SET IDENTITY_INSERT [dbo].[Students] OFF

-- Insert Faculties
SET IDENTITY_INSERT [dbo].[Faculties] ON
INSERT INTO [dbo].[Faculties] ([FacultyId], [FacultyCode], [FullName], [Email], [PhoneNumber], [Department], [Specialization], [IsActive], [CreatedDate])
VALUES 
    (1, N'GV001', N'TS. Nguyễn Văn Giảng', N'nguyen.giang@university.edu.vn', N'0123456789', N'Khoa Công nghệ Thông tin', N'Lập trình', 1, '2024-01-01'),
    (2, N'GV002', N'TS. Trần Thị Giảng', N'tran.giang@university.edu.vn', N'0123456790', N'Khoa Công nghệ Thông tin', N'Lập trình hướng đối tượng', 1, '2024-01-01')
SET IDENTITY_INSERT [dbo].[Faculties] OFF

-- Insert Users (mật khẩu đơn giản cho test)
SET IDENTITY_INSERT [dbo].[Users] ON
INSERT INTO [dbo].[Users] ([UserId], [Username], [Password], [PasswordSalt], [Email], [FullName], [RoleId], [StudentId], [FacultyId], [IsActive], [CreatedDate])
VALUES 
    (1, N'admin', N'admin123', N'', N'admin@sims.edu', N'Administrator', 1, NULL, NULL, 1, '2024-01-01'),
    (2, N'faculty1', N'faculty123', N'', N'nguyen.giang@university.edu.vn', N'TS. Nguyễn Văn Giảng', 2, NULL, 1, 1, '2024-01-01'),
    (3, N'student1', N'student123', N'', N'nguyenvana@sims.edu', N'Nguyễn Văn A', 3, 1, NULL, 1, '2024-01-01')
SET IDENTITY_INSERT [dbo].[Users] OFF

-- Insert Courses
SET IDENTITY_INSERT [dbo].[Courses] ON
INSERT INTO [dbo].[Courses] ([CourseId], [CourseCode], [CourseName], [Description], [Credits], [MaxStudents], [Semester], [AcademicYear], [Instructor], [FacultyId], [IsActive], [CreatedDate])
VALUES 
    (1, N'CS101', N'Lập trình Cơ bản', N'Khóa học lập trình cơ bản cho sinh viên', 3, 50, N'Học kỳ 1', N'2024-2025', N'TS. Nguyễn Văn Giảng', 1, 1, '2024-01-01'),
    (2, N'CS201', N'Lập trình Hướng đối tượng', N'Khóa học lập trình hướng đối tượng', 4, 40, N'Học kỳ 1', N'2024-2025', N'TS. Trần Thị Giảng', 2, 1, '2024-01-01')
SET IDENTITY_INSERT [dbo].[Courses] OFF

PRINT 'Dữ liệu mẫu đã được thêm!'

-- =============================================
-- 7. KIỂM TRA KẾT QUẢ
-- =============================================

PRINT '============================================='
PRINT 'RESET DATABASE HOÀN THÀNH!'
PRINT '============================================='

-- Hiển thị thống kê
SELECT 'Roles' as TableName, COUNT(*) as RecordCount FROM Roles
UNION ALL
SELECT 'Students', COUNT(*) FROM Students
UNION ALL
SELECT 'Faculties', COUNT(*) FROM Faculties
UNION ALL
SELECT 'Users', COUNT(*) FROM Users
UNION ALL
SELECT 'Courses', COUNT(*) FROM Courses
UNION ALL
SELECT 'Enrollments', COUNT(*) FROM Enrollments
UNION ALL
SELECT 'AcademicRecords', COUNT(*) FROM AcademicRecords

-- Hiển thị thông tin đăng nhập
PRINT ''
PRINT 'Tài khoản đăng nhập:'
PRINT 'Admin: admin / admin123'
PRINT 'Faculty: faculty1 / faculty123'
PRINT 'Student: student1 / student123'
PRINT ''
PRINT 'Database đã sẵn sàng sử dụng!'
PRINT '============================================='

GO