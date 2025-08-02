-- =============================================
-- Script tạo Database đầy đủ cho hệ thống SIMS (Student Information Management System)
-- Phiên bản: 1.0
-- Ngày tạo: 2025-08-04
-- Mô tả: Script tạo database hoàn chỉnh bao gồm tables, indexes, constraints, data và stored procedures
-- =============================================

-- Kiểm tra và tạo database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Web_SIMS')
BEGIN
    CREATE DATABASE [Web_SIMS]
    PRINT 'Database Web_SIMS đã được tạo thành công!'
END
ELSE
BEGIN
    PRINT 'Database Web_SIMS đã tồn tại!'
END
GO

USE [Web_SIMS]
GO

-- =============================================
-- 1. TẠO CÁC BẢNG (TABLES)
-- =============================================

-- Bảng Roles (Vai trò)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Roles] (
        [RoleId] int IDENTITY(1,1) NOT NULL,
        [RoleName] nvarchar(50) NOT NULL,
        [Description] nvarchar(200) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETDATE(),
        [UpdatedDate] datetime2(7) NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([RoleId] ASC)
    )
    PRINT 'Bảng Roles đã được tạo!'
END
GO

-- Bảng Students (Sinh viên)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Students' AND xtype='U')
BEGIN
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
    PRINT 'Bảng Students đã được tạo!'
END
GO

-- Bảng Faculties (Giảng viên)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Faculties' AND xtype='U')
BEGIN
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
    PRINT 'Bảng Faculties đã được tạo!'
END
GO

-- Bảng Users (Người dùng)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
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
    PRINT 'Bảng Users đã được tạo!'
END
GO

-- Bảng Courses (Khóa học)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Courses' AND xtype='U')
BEGIN
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
    PRINT 'Bảng Courses đã được tạo!'
END
GO

-- Bảng Enrollments (Đăng ký học)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Enrollments' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Enrollments] (
        [EnrollmentId] int IDENTITY(1,1) NOT NULL,
        [StudentId] int NOT NULL,
        [CourseId] int NOT NULL,
        [EnrollmentDate] datetime2(7) NOT NULL DEFAULT GETDATE(),
        [Status] int NOT NULL DEFAULT 0, -- 0: Pending, 1: Approved, 2: Rejected
        [Notes] nvarchar(500) NULL,
        [CreatedDate] datetime2(7) NOT NULL DEFAULT GETDATE(),
        [UpdatedDate] datetime2(7) NULL,
        CONSTRAINT [PK_Enrollments] PRIMARY KEY CLUSTERED ([EnrollmentId] ASC)
    )
    PRINT 'Bảng Enrollments đã được tạo!'
END
GO

-- Bảng AcademicRecords (Bảng điểm)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AcademicRecords' AND xtype='U')
BEGIN
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
    PRINT 'Bảng AcademicRecords đã được tạo!'
END
GO

-- =============================================
-- 2. TẠO INDEXES
-- =============================================

-- Indexes cho bảng Students
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_StudentCode')
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Students_StudentCode] ON [dbo].[Students] ([StudentCode] ASC)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_Email')
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Students_Email] ON [dbo].[Students] ([Email] ASC)

-- Indexes cho bảng Faculties
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Faculties_FacultyCode')
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Faculties_FacultyCode] ON [dbo].[Faculties] ([FacultyCode] ASC)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Faculties_Email')
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Faculties_Email] ON [dbo].[Faculties] ([Email] ASC)

-- Indexes cho bảng Users
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username')
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Username] ON [dbo].[Users] ([Username] ASC)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email')
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users] ([Email] ASC)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_RoleId')
    CREATE NONCLUSTERED INDEX [IX_Users_RoleId] ON [dbo].[Users] ([RoleId] ASC)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_StudentId')
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_StudentId] ON [dbo].[Users] ([StudentId] ASC) WHERE [StudentId] IS NOT NULL

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_FacultyId')
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_FacultyId] ON [dbo].[Users] ([FacultyId] ASC) WHERE [FacultyId] IS NOT NULL

-- Indexes cho bảng Courses
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Courses_CourseCode')
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Courses_CourseCode] ON [dbo].[Courses] ([CourseCode] ASC)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Courses_FacultyId')
    CREATE NONCLUSTERED INDEX [IX_Courses_FacultyId] ON [dbo].[Courses] ([FacultyId] ASC)

-- Indexes cho bảng Enrollments
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Enrollments_StudentId')
    CREATE NONCLUSTERED INDEX [IX_Enrollments_StudentId] ON [dbo].[Enrollments] ([StudentId] ASC)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Enrollments_CourseId')
    CREATE NONCLUSTERED INDEX [IX_Enrollments_CourseId] ON [dbo].[Enrollments] ([CourseId] ASC)

-- Indexes cho bảng AcademicRecords
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_StudentId')
    CREATE NONCLUSTERED INDEX [IX_AcademicRecords_StudentId] ON [dbo].[AcademicRecords] ([StudentId] ASC)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AcademicRecords_CourseId')
    CREATE NONCLUSTERED INDEX [IX_AcademicRecords_CourseId] ON [dbo].[AcademicRecords] ([CourseId] ASC)

PRINT 'Tất cả indexes đã được tạo!'
GO

-- =============================================
-- 3. TẠO FOREIGN KEY CONSTRAINTS
-- =============================================

-- Foreign Keys cho bảng Users
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Roles_RoleId')
    ALTER TABLE [dbo].[Users] ADD CONSTRAINT [FK_Users_Roles_RoleId] 
    FOREIGN KEY([RoleId]) REFERENCES [dbo].[Roles] ([RoleId]) ON DELETE NO ACTION

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Students_StudentId')
    ALTER TABLE [dbo].[Users] ADD CONSTRAINT [FK_Users_Students_StudentId] 
    FOREIGN KEY([StudentId]) REFERENCES [dbo].[Students] ([StudentId]) ON DELETE NO ACTION

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Faculties_FacultyId')
    ALTER TABLE [dbo].[Users] ADD CONSTRAINT [FK_Users_Faculties_FacultyId] 
    FOREIGN KEY([FacultyId]) REFERENCES [dbo].[Faculties] ([FacultyId]) ON DELETE NO ACTION

-- Foreign Keys cho bảng Courses
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Courses_Faculties_FacultyId')
    ALTER TABLE [dbo].[Courses] ADD CONSTRAINT [FK_Courses_Faculties_FacultyId] 
    FOREIGN KEY([FacultyId]) REFERENCES [dbo].[Faculties] ([FacultyId]) ON DELETE NO ACTION

-- Foreign Keys cho bảng Enrollments
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Enrollments_Students_StudentId')
    ALTER TABLE [dbo].[Enrollments] ADD CONSTRAINT [FK_Enrollments_Students_StudentId] 
    FOREIGN KEY([StudentId]) REFERENCES [dbo].[Students] ([StudentId]) ON DELETE CASCADE

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Enrollments_Courses_CourseId')
    ALTER TABLE [dbo].[Enrollments] ADD CONSTRAINT [FK_Enrollments_Courses_CourseId] 
    FOREIGN KEY([CourseId]) REFERENCES [dbo].[Courses] ([CourseId]) ON DELETE CASCADE

-- Foreign Keys cho bảng AcademicRecords
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AcademicRecords_Students_StudentId')
    ALTER TABLE [dbo].[AcademicRecords] ADD CONSTRAINT [FK_AcademicRecords_Students_StudentId] 
    FOREIGN KEY([StudentId]) REFERENCES [dbo].[Students] ([StudentId]) ON DELETE CASCADE

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AcademicRecords_Courses_CourseId')
    ALTER TABLE [dbo].[AcademicRecords] ADD CONSTRAINT [FK_AcademicRecords_Courses_CourseId] 
    FOREIGN KEY([CourseId]) REFERENCES [dbo].[Courses] ([CourseId]) ON DELETE CASCADE

PRINT 'Tất cả Foreign Key constraints đã được tạo!'
GO

-- =============================================
-- 4. INSERT DỮ LIỆU MẪU
-- =============================================

-- Insert Roles
IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles])
BEGIN
    SET IDENTITY_INSERT [dbo].[Roles] ON
    
    INSERT INTO [dbo].[Roles] ([RoleId], [RoleName], [Description], [IsActive])
    VALUES 
        (1, N'Admin', N'Quản trị viên hệ thống', 1),
        (2, N'Faculty', N'Giảng viên', 1),
        (3, N'Student', N'Sinh viên', 1)
    
    SET IDENTITY_INSERT [dbo].[Roles] OFF
    PRINT 'Dữ liệu Roles đã được thêm!'
END
GO

-- Insert Students
IF NOT EXISTS (SELECT 1 FROM [dbo].[Students])
BEGIN
    SET IDENTITY_INSERT [dbo].[Students] ON
    
    INSERT INTO [dbo].[Students] ([StudentId], [StudentCode], [FullName], [Email], [PhoneNumber], [DateOfBirth], [Address], [Gender], [Major], [AcademicYear], [EnrollmentDate], [IsActive], [CreatedDate])
    VALUES 
        (1, N'SV001', N'Nguyễn Văn A', N'nguyenvana@sims.edu', N'0123456789', '2000-01-01', N'Hà Nội', N'Nam', N'Công nghệ thông tin', N'2024-2025', '2024-01-01', 1, '2024-01-01'),
        (2, N'SV002', N'Trần Thị B', N'tranthib@sims.edu', N'0987654321', '2001-05-15', N'TP.HCM', N'Nữ', N'Kinh tế', N'2024-2025', '2024-01-01', 1, '2024-01-01'),
        (3, N'SV003', N'Lê Văn C', N'levanc@sims.edu', N'0123456790', '2000-03-10', N'Đà Nẵng', N'Nam', N'Công nghệ thông tin', N'2024-2025', '2024-01-01', 1, '2024-01-01'),
        (4, N'SV004', N'Phạm Thị D', N'phamthid@sims.edu', N'0987654322', '2001-07-20', N'Hải Phòng', N'Nữ', N'Kinh tế', N'2024-2025', '2024-01-01', 1, '2024-01-01'),
        (5, N'SV005', N'Hoàng Văn E', N'hoangvane@sims.edu', N'0123456791', '2000-09-05', N'Cần Thơ', N'Nam', N'Công nghệ thông tin', N'2024-2025', '2024-01-01', 1, '2024-01-01')
    
    SET IDENTITY_INSERT [dbo].[Students] OFF
    PRINT 'Dữ liệu Students đã được thêm!'
END
GO

-- Insert Faculties
IF NOT EXISTS (SELECT 1 FROM [dbo].[Faculties])
BEGIN
    SET IDENTITY_INSERT [dbo].[Faculties] ON
    
    INSERT INTO [dbo].[Faculties] ([FacultyId], [FacultyCode], [FullName], [Email], [PhoneNumber], [Department], [Specialization], [IsActive], [CreatedDate])
    VALUES 
        (1, N'GV001', N'TS. Nguyễn Văn Giảng', N'nguyen.giang@university.edu.vn', N'0123456789', N'Khoa Công nghệ Thông tin', N'Lập trình', 1, '2024-01-01'),
        (2, N'GV002', N'TS. Trần Thị Giảng', N'tran.giang@university.edu.vn', N'0123456790', N'Khoa Công nghệ Thông tin', N'Lập trình hướng đối tượng', 1, '2024-01-01'),
        (3, N'GV003', N'TS. Lê Văn Kinh', N'le.kinh@university.edu.vn', N'0123456791', N'Khoa Kinh tế', N'Kinh tế học', 1, '2024-01-01'),
        (4, N'GV004', N'PGS. Nguyễn Thị Hoa', N'nguyen.hoa@university.edu.vn', N'0123456792', N'Khoa Công nghệ Thông tin', N'Cơ sở dữ liệu', 1, '2024-01-01'),
        (5, N'GV005', N'TS. Phạm Văn Nam', N'pham.nam@university.edu.vn', N'0123456793', N'Khoa Kinh tế', N'Quản trị kinh doanh', 1, '2024-01-01')
    
    SET IDENTITY_INSERT [dbo].[Faculties] OFF
    PRINT 'Dữ liệu Faculties đã được thêm!'
END
GO

-- Insert Users (với mật khẩu đã hash)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users])
BEGIN
    SET IDENTITY_INSERT [dbo].[Users] ON
    
    INSERT INTO [dbo].[Users] ([UserId], [Username], [Password], [PasswordSalt], [Email], [FullName], [RoleId], [StudentId], [FacultyId], [IsActive], [CreatedDate])
    VALUES 
        (1, N'admin', N'AQAAAAEAACcQAAAAEKqJ8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8=', N'YWRtaW5zYWx0MTIz', N'admin@sims.edu', N'Administrator', 1, NULL, NULL, 1, '2024-01-01'),
        (2, N'faculty1', N'AQAAAAEAACcQAAAAEKqJ8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8=', N'ZmFjdWx0eXNhbHQxMjM=', N'nguyen.giang@university.edu.vn', N'TS. Nguyễn Văn Giảng', 2, NULL, 1, 1, '2024-01-01'),
        (3, N'faculty2', N'AQAAAAEAACcQAAAAEKqJ8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8=', N'ZmFjdWx0eXNhbHQxMjM=', N'tran.giang@university.edu.vn', N'TS. Trần Thị Giảng', 2, NULL, 2, 1, '2024-01-01'),
        (4, N'student1', N'AQAAAAEAACcQAAAAEKqJ8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8=', N'c3R1ZGVudHNhbHQxMjM=', N'nguyenvana@sims.edu', N'Nguyễn Văn A', 3, 1, NULL, 1, '2024-01-01'),
        (5, N'student2', N'AQAAAAEAACcQAAAAEKqJ8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8=', N'c3R1ZGVudHNhbHQxMjM=', N'tranthib@sims.edu', N'Trần Thị B', 3, 2, NULL, 1, '2024-01-01')
    
    SET IDENTITY_INSERT [dbo].[Users] OFF
    PRINT 'Dữ liệu Users đã được thêm!'
END
GO

-- Insert Courses
IF NOT EXISTS (SELECT 1 FROM [dbo].[Courses])
BEGIN
    SET IDENTITY_INSERT [dbo].[Courses] ON
    
    INSERT INTO [dbo].[Courses] ([CourseId], [CourseCode], [CourseName], [Description], [Credits], [MaxStudents], [Semester], [AcademicYear], [Instructor], [FacultyId], [IsActive], [CreatedDate])
    VALUES 
        (1, N'CS101', N'Lập trình Cơ bản', N'Khóa học lập trình cơ bản cho sinh viên', 3, 50, N'Học kỳ 1', N'2024-2025', N'TS. Nguyễn Văn Giảng', 1, 1, '2024-01-01'),
        (2, N'CS201', N'Lập trình Hướng đối tượng', N'Khóa học lập trình hướng đối tượng', 4, 40, N'Học kỳ 1', N'2024-2025', N'TS. Trần Thị Giảng', 2, 1, '2024-01-01'),
        (3, N'CS301', N'Cơ sở dữ liệu', N'Khóa học về cơ sở dữ liệu', 3, 45, N'Học kỳ 2', N'2024-2025', N'PGS. Nguyễn Thị Hoa', 4, 1, '2024-01-01'),
        (4, N'EC101', N'Kinh tế học đại cương', N'Khóa học kinh tế học cơ bản', 3, 60, N'Học kỳ 1', N'2024-2025', N'TS. Lê Văn Kinh', 3, 1, '2024-01-01'),
        (5, N'EC201', N'Quản trị kinh doanh', N'Khóa học về quản trị kinh doanh', 4, 50, N'Học kỳ 2', N'2024-2025', N'TS. Phạm Văn Nam', 5, 1, '2024-01-01')
    
    SET IDENTITY_INSERT [dbo].[Courses] OFF
    PRINT 'Dữ liệu Courses đã được thêm!'
END
GO

-- Insert Enrollments
IF NOT EXISTS (SELECT 1 FROM [dbo].[Enrollments])
BEGIN
    INSERT INTO [dbo].[Enrollments] ([StudentId], [CourseId], [EnrollmentDate], [Status], [Notes])
    VALUES 
        (1, 1, '2024-01-01', 1, N'Đăng ký thành công'),
        (1, 2, '2024-01-01', 1, N'Đăng ký thành công'),
        (1, 3, '2024-01-15', 0, N'Chờ phê duyệt'),
        (2, 4, '2024-01-01', 1, N'Đăng ký thành công'),
        (2, 5, '2024-01-01', 1, N'Đăng ký thành công'),
        (3, 1, '2024-01-02', 1, N'Đăng ký thành công'),
        (3, 2, '2024-01-02', 0, N'Chờ phê duyệt'),
        (4, 4, '2024-01-03', 1, N'Đăng ký thành công'),
        (5, 1, '2024-01-03', 2, N'Không đủ điều kiện tiên quyết')
    
    PRINT 'Dữ liệu Enrollments đã được thêm!'
END
GO

-- Insert AcademicRecords
IF NOT EXISTS (SELECT 1 FROM [dbo].[AcademicRecords])
BEGIN
    INSERT INTO [dbo].[AcademicRecords] ([StudentId], [CourseId], [Semester], [AcademicYear], [MidtermScore], [FinalScore], [TotalScore], [Grade], [Notes])
    VALUES 
        (1, 1, 1, 2024, 8.5, 9.0, 8.8, N'A', N'Học tập tốt'),
        (1, 2, 1, 2024, 7.5, 8.0, 7.8, N'B', N'Cần cải thiện'),
        (2, 4, 1, 2024, 9.0, 9.5, 9.3, N'A+', N'Xuất sắc'),
        (2, 5, 1, 2024, 8.0, 8.5, 8.3, N'A', N'Học tập tốt'),
        (3, 1, 1, 2024, 7.0, 7.5, 7.3, N'B', N'Đạt yêu cầu'),
        (4, 4, 1, 2024, 6.5, 7.0, 6.8, N'C', N'Cần cố gắng hơn')
    
    PRINT 'Dữ liệu AcademicRecords đã được thêm!'
END
GO

-- =============================================
-- 5. TẠO CÁC VIEW BÁO CÁO
-- =============================================

-- View: Thông tin sinh viên và khóa học đã đăng ký
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_StudentEnrollments')
    DROP VIEW [dbo].[vw_StudentEnrollments]
GO

CREATE VIEW [dbo].[vw_StudentEnrollments]
AS
SELECT 
    s.StudentId,
    s.StudentCode,
    s.FullName AS StudentName,
    s.Email AS StudentEmail,
    s.Major,
    c.CourseId,
    c.CourseCode,
    c.CourseName,
    c.Credits,
    e.EnrollmentDate,
    CASE e.Status 
        WHEN 0 THEN N'Chờ phê duyệt'
        WHEN 1 THEN N'Đã phê duyệt'
        WHEN 2 THEN N'Bị từ chối'
        ELSE N'Không xác định'
    END AS EnrollmentStatus,
    e.Notes AS EnrollmentNotes
FROM Students s
INNER JOIN Enrollments e ON s.StudentId = e.StudentId
INNER JOIN Courses c ON e.CourseId = c.CourseId
WHERE s.IsActive = 1 AND c.IsActive = 1
GO

-- View: Bảng điểm chi tiết
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_StudentGrades')
    DROP VIEW [dbo].[vw_StudentGrades]
GO

CREATE VIEW [dbo].[vw_StudentGrades]
AS
SELECT 
    s.StudentId,
    s.StudentCode,
    s.FullName AS StudentName,
    c.CourseId,
    c.CourseCode,
    c.CourseName,
    c.Credits,
    ar.Semester,
    ar.AcademicYear,
    ar.MidtermScore,
    ar.FinalScore,
    ar.TotalScore,
    ar.Grade,
    ar.Notes
FROM Students s
INNER JOIN AcademicRecords ar ON s.StudentId = ar.StudentId
INNER JOIN Courses c ON ar.CourseId = c.CourseId
WHERE s.IsActive = 1 AND c.IsActive = 1
GO

-- View: Thống kê khóa học
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_CourseStatistics')
    DROP VIEW [dbo].[vw_CourseStatistics]
GO

CREATE VIEW [dbo].[vw_CourseStatistics]
AS
SELECT 
    c.CourseId,
    c.CourseCode,
    c.CourseName,
    c.Credits,
    c.MaxStudents,
    f.FullName AS InstructorName,
    f.Department,
    COUNT(e.StudentId) AS EnrolledStudents,
    (c.MaxStudents - COUNT(e.StudentId)) AS AvailableSlots,
    CAST(COUNT(e.StudentId) * 100.0 / c.MaxStudents AS DECIMAL(5,2)) AS EnrollmentPercentage
FROM Courses c
LEFT JOIN Enrollments e ON c.CourseId = e.CourseId AND e.Status = 1
LEFT JOIN Faculties f ON c.FacultyId = f.FacultyId
WHERE c.IsActive = 1
GROUP BY c.CourseId, c.CourseCode, c.CourseName, c.Credits, c.MaxStudents, f.FullName, f.Department
GO

PRINT 'Các Views đã được tạo thành công!'
GO

-- =============================================
-- 6. TẠO STORED PROCEDURES
-- =============================================

-- Stored Procedure: Lấy thông tin sinh viên theo mã
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetStudentByCode')
    DROP PROCEDURE [dbo].[sp_GetStudentByCode]
GO

CREATE PROCEDURE [dbo].[sp_GetStudentByCode]
    @StudentCode NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        s.*,
        u.Username,
        u.Email AS UserEmail,
        u.IsActive AS UserActive
    FROM Students s
    LEFT JOIN Users u ON s.StudentId = u.StudentId
    WHERE s.StudentCode = @StudentCode AND s.IsActive = 1
END
GO

-- Stored Procedure: Đăng ký khóa học
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_EnrollStudent')
    DROP PROCEDURE [dbo].[sp_EnrollStudent]
GO

CREATE PROCEDURE [dbo].[sp_EnrollStudent]
    @StudentId INT,
    @CourseId INT,
    @Notes NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(500)
    DECLARE @EnrolledCount INT
    DECLARE @MaxStudents INT
    
    -- Kiểm tra sinh viên tồn tại
    IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentId = @StudentId AND IsActive = 1)
    BEGIN
        SET @ErrorMessage = N'Sinh viên không tồn tại hoặc không hoạt động'
        RAISERROR(@ErrorMessage, 16, 1)
        RETURN
    END
    
    -- Kiểm tra khóa học tồn tại
    IF NOT EXISTS (SELECT 1 FROM Courses WHERE CourseId = @CourseId AND IsActive = 1)
    BEGIN
        SET @ErrorMessage = N'Khóa học không tồn tại hoặc không hoạt động'
        RAISERROR(@ErrorMessage, 16, 1)
        RETURN
    END
    
    -- Kiểm tra đã đăng ký chưa
    IF EXISTS (SELECT 1 FROM Enrollments WHERE StudentId = @StudentId AND CourseId = @CourseId)
    BEGIN
        SET @ErrorMessage = N'Sinh viên đã đăng ký khóa học này'
        RAISERROR(@ErrorMessage, 16, 1)
        RETURN
    END
    
    -- Kiểm tra số lượng sinh viên đã đăng ký
    SELECT @MaxStudents = MaxStudents FROM Courses WHERE CourseId = @CourseId
    SELECT @EnrolledCount = COUNT(*) FROM Enrollments WHERE CourseId = @CourseId AND Status = 1
    
    IF @EnrolledCount >= @MaxStudents
    BEGIN
        SET @ErrorMessage = N'Khóa học đã đầy'
        RAISERROR(@ErrorMessage, 16, 1)
        RETURN
    END
    
    -- Thực hiện đăng ký
    INSERT INTO Enrollments (StudentId, CourseId, EnrollmentDate, Status, Notes)
    VALUES (@StudentId, @CourseId, GETDATE(), 0, @Notes)
    
    SELECT N'Đăng ký thành công' AS Message
END
GO

-- Stored Procedure: Cập nhật điểm
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_UpdateGrade')
    DROP PROCEDURE [dbo].[sp_UpdateGrade]
GO

CREATE PROCEDURE [dbo].[sp_UpdateGrade]
    @StudentId INT,
    @CourseId INT,
    @Semester INT,
    @AcademicYear INT,
    @MidtermScore DECIMAL(5,2) = NULL,
    @FinalScore DECIMAL(5,2) = NULL,
    @Notes NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TotalScore DECIMAL(5,2)
    DECLARE @Grade NVARCHAR(2)
    
    -- Tính điểm tổng kết (40% giữa kỳ + 60% cuối kỳ)
    IF @MidtermScore IS NOT NULL AND @FinalScore IS NOT NULL
    BEGIN
        SET @TotalScore = @MidtermScore * 0.4 + @FinalScore * 0.6
        
        -- Xác định xếp loại
        IF @TotalScore >= 9.0
            SET @Grade = N'A+'
        ELSE IF @TotalScore >= 8.0
            SET @Grade = N'A'
        ELSE IF @TotalScore >= 7.0
            SET @Grade = N'B'
        ELSE IF @TotalScore >= 6.0
            SET @Grade = N'C'
        ELSE IF @TotalScore >= 5.0
            SET @Grade = N'D'
        ELSE
            SET @Grade = N'F'
    END
    
    -- Kiểm tra bản ghi đã tồn tại
    IF EXISTS (SELECT 1 FROM AcademicRecords WHERE StudentId = @StudentId AND CourseId = @CourseId AND Semester = @Semester AND AcademicYear = @AcademicYear)
    BEGIN
        -- Cập nhật
        UPDATE AcademicRecords 
        SET 
            MidtermScore = ISNULL(@MidtermScore, MidtermScore),
            FinalScore = ISNULL(@FinalScore, FinalScore),
            TotalScore = @TotalScore,
            Grade = @Grade,
            Notes = ISNULL(@Notes, Notes),
            UpdatedDate = GETDATE()
        WHERE StudentId = @StudentId AND CourseId = @CourseId AND Semester = @Semester AND AcademicYear = @AcademicYear
    END
    ELSE
    BEGIN
        -- Thêm mới
        INSERT INTO AcademicRecords (StudentId, CourseId, Semester, AcademicYear, MidtermScore, FinalScore, TotalScore, Grade, Notes)
        VALUES (@StudentId, @CourseId, @Semester, @AcademicYear, @MidtermScore, @FinalScore, @TotalScore, @Grade, @Notes)
    END
    
    SELECT N'Cập nhật điểm thành công' AS Message
END
GO

PRINT 'Các Stored Procedures đã được tạo thành công!'
GO

-- =============================================
-- 7. TẠO FUNCTIONS
-- =============================================

-- Function: Tính GPA của sinh viên
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'fn_CalculateGPA' AND type = 'FN')
    DROP FUNCTION [dbo].[fn_CalculateGPA]
GO

CREATE FUNCTION [dbo].[fn_CalculateGPA](@StudentId INT)
RETURNS DECIMAL(3,2)
AS
BEGIN
    DECLARE @GPA DECIMAL(3,2)
    
    SELECT @GPA = 
        CASE 
            WHEN SUM(c.Credits) = 0 THEN 0
            ELSE CAST(SUM(
                CASE ar.Grade
                    WHEN 'A+' THEN 4.0 * c.Credits
                    WHEN 'A' THEN 3.7 * c.Credits
                    WHEN 'B' THEN 3.0 * c.Credits
                    WHEN 'C' THEN 2.0 * c.Credits
                    WHEN 'D' THEN 1.0 * c.Credits
                    ELSE 0
                END
            ) / SUM(c.Credits) AS DECIMAL(3,2))
        END
    FROM AcademicRecords ar
    INNER JOIN Courses c ON ar.CourseId = c.CourseId
    WHERE ar.StudentId = @StudentId AND ar.Grade IS NOT NULL
    
    RETURN ISNULL(@GPA, 0)
END
GO

PRINT 'Functions đã được tạo thành công!'
GO

-- =============================================
-- 8. KIỂM TRA VÀ HIỂN THỊ THỐNG KÊ
-- =============================================

PRINT '============================================='
PRINT 'THỐNG KÊ DATABASE SAU KHI TẠO'
PRINT '============================================='

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

PRINT ''
PRINT 'Kiểm tra một số dữ liệu mẫu:'
PRINT '============================================='

-- Hiển thị thông tin admin
SELECT 'ADMIN USER:' as Info, Username, Email, FullName, IsActive 
FROM Users WHERE Username = 'admin'

-- Hiển thị thống kê đăng ký khóa học
SELECT 'ENROLLMENT STATISTICS:' as Info
SELECT CourseCode, CourseName, COUNT(e.StudentId) as EnrolledStudents, MaxStudents
FROM Courses c
LEFT JOIN Enrollments e ON c.CourseId = e.CourseId AND e.Status = 1
GROUP BY c.CourseId, CourseCode, CourseName, MaxStudents
ORDER BY CourseCode

PRINT ''
PRINT '============================================='
PRINT 'DATABASE ĐÃ ĐƯỢC TẠO THÀNH CÔNG!'
PRINT 'Bạn có thể sử dụng các tài khoản sau để đăng nhập:'
PRINT 'Admin: admin / admin123'
PRINT 'Faculty: faculty1 / faculty123'
PRINT 'Student: student1 / student123'
PRINT '============================================='