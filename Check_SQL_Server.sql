-- Script kiểm tra SQL Server và Database Web SIMS
-- Chạy script này để kiểm tra tình trạng

-- 1. Kiểm tra SQL Server có hoạt động không
SELECT '=== KIỂM TRA SQL SERVER ===' as Info;
SELECT @@VERSION as SQLServerVersion;
SELECT GETDATE() as CurrentTime;
GO

-- 2. Kiểm tra tất cả databases
SELECT '=== DANH SÁCH DATABASES ===' as Info;
SELECT 
    name as DatabaseName,
    database_id,
    create_date,
    state_desc as Status
FROM sys.databases
ORDER BY name;
GO

-- 3. Kiểm tra database Web_SIMS cụ thể
SELECT '=== KIỂM TRA DATABASE WEB_SIMS ===' as Info;

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Web_SIMS')
BEGIN
    PRINT '✓ Database Web_SIMS TỒN TẠI';
    
    -- Kiểm tra bảng trong Web_SIMS
    USE Web_SIMS;
    SELECT '=== BẢNG TRONG WEB_SIMS ===' as Info;
    SELECT 
        TABLE_NAME as TableName,
        TABLE_TYPE as TableType
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_TYPE = 'BASE TABLE'
    ORDER BY TABLE_NAME;
    
    -- Kiểm tra dữ liệu trong các bảng chính
    SELECT '=== KIỂM TRA DỮ LIỆU ===' as Info;
    
    -- Kiểm tra Roles
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
    BEGIN
        SELECT 'Roles' as TableName, COUNT(*) as RecordCount FROM Roles;
        SELECT 'Dữ liệu Roles:' as Info;
        SELECT RoleId, RoleName, Description, IsActive FROM Roles;
    END
    ELSE
        PRINT '✗ Bảng Roles KHÔNG tồn tại';
    
    -- Kiểm tra Users
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
    BEGIN
        SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM Users;
        SELECT 'Dữ liệu Users:' as Info;
        SELECT UserId, Username, Password, FullName, IsActive FROM Users;
    END
    ELSE
        PRINT '✗ Bảng Users KHÔNG tồn tại';
    
    -- Kiểm tra Students
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
    BEGIN
        SELECT 'Students' as TableName, COUNT(*) as RecordCount FROM Students;
        SELECT 'Dữ liệu Students:' as Info;
        SELECT StudentId, StudentCode, FullName, Email, IsActive FROM Students;
    END
    ELSE
        PRINT '✗ Bảng Students KHÔNG tồn tại';
    
    -- Kiểm tra Courses
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Courses')
    BEGIN
        SELECT 'Courses' as TableName, COUNT(*) as RecordCount FROM Courses;
        SELECT 'Dữ liệu Courses:' as Info;
        SELECT CourseId, CourseCode, CourseName, Instructor, IsActive FROM Courses;
    END
    ELSE
        PRINT '✗ Bảng Courses KHÔNG tồn tại';
    
    -- Kiểm tra Enrollments
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Enrollments')
    BEGIN
        SELECT 'Enrollments' as TableName, COUNT(*) as RecordCount FROM Enrollments;
    END
    ELSE
        PRINT '✗ Bảng Enrollments KHÔNG tồn tại';
    
    -- Kiểm tra thông tin đăng nhập
    SELECT '=== THÔNG TIN ĐĂNG NHẬP ===' as Info;
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users') AND EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
    BEGIN
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
    END
    ELSE
        PRINT '✗ Không thể kiểm tra thông tin đăng nhập (thiếu bảng Users hoặc Roles)';
END
ELSE
BEGIN
    PRINT '✗ Database Web_SIMS KHÔNG TỒN TẠI';
    PRINT 'Cần tạo database bằng cách:';
    PRINT '1. Chạy script Recreate_Database.sql';
    PRINT '2. Hoặc chạy dotnet run để tự động tạo';
END
GO

-- 4. Kiểm tra connection string
SELECT '=== KIỂM TRA CONNECTION ===' as Info;
SELECT 
    name as DatabaseName,
    database_id,
    state_desc as Status,
    recovery_model_desc as RecoveryModel
FROM sys.databases 
WHERE name = 'Web_SIMS';
GO

-- 5. Kiểm tra quyền truy cập
SELECT '=== KIỂM TRA QUYỀN TRUY CẬP ===' as Info;
SELECT 
    dp.name as DatabaseUser,
    dp.type_desc as UserType,
    sp.name as LoginName
FROM sys.database_principals dp
LEFT JOIN sys.server_principals sp ON dp.sid = sp.sid
WHERE dp.name IN ('dbo', 'public')
ORDER BY dp.name;
GO

PRINT '=== KẾT QUẢ KIỂM TRA ===';
PRINT 'Nếu database không tồn tại, hãy chạy script Recreate_Database.sql';
PRINT 'Nếu database tồn tại nhưng thiếu dữ liệu, hãy chạy dotnet run'; 