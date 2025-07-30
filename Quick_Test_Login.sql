-- Script kiểm tra nhanh database Web SIMS
USE Web_SIMS;
GO

-- 1. Kiểm tra database có tồn tại không
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Web_SIMS')
BEGIN
    PRINT 'Database Web_SIMS không tồn tại!';
    RETURN;
END

PRINT 'Database Web_SIMS tồn tại!';
GO

-- 2. Kiểm tra các bảng
SELECT '=== KIỂM TRA BẢNG ===' as Info;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
    PRINT '✓ Bảng Roles tồn tại'
ELSE
    PRINT '✗ Bảng Roles KHÔNG tồn tại';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
    PRINT '✓ Bảng Users tồn tại'
ELSE
    PRINT '✗ Bảng Users KHÔNG tồn tại';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
    PRINT '✓ Bảng Students tồn tại'
ELSE
    PRINT '✗ Bảng Students KHÔNG tồn tại';

GO

-- 3. Kiểm tra dữ liệu
SELECT '=== KIỂM TRA DỮ LIỆU ===' as Info;

-- Kiểm tra Roles
SELECT 'Roles' as TableName, COUNT(*) as RecordCount FROM Roles;

-- Kiểm tra Users
SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM Users;

-- Kiểm tra Students
SELECT 'Students' as TableName, COUNT(*) as RecordCount FROM Students;

GO

-- 4. Hiển thị thông tin đăng nhập
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

GO

-- 5. Test query đăng nhập
SELECT '=== TEST QUERY ĐĂNG NHẬP ===' as Info;

-- Test tìm user faculty1
SELECT 
    u.Username,
    u.Password,
    u.IsActive,
    r.RoleName
FROM Users u
JOIN Roles r ON u.RoleId = r.RoleId
WHERE u.Username = 'faculty1' AND u.IsActive = 1;

GO 