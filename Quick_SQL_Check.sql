-- Script kiểm tra nhanh SQL Server
-- Chạy script này để kiểm tra SQL Server có hoạt động không

-- 1. Kiểm tra SQL Server version
SELECT '=== SQL SERVER VERSION ===' as Info;
SELECT @@VERSION as Version;
GO

-- 2. Kiểm tra server name
SELECT '=== SERVER NAME ===' as Info;
SELECT @@SERVERNAME as ServerName;
SELECT SERVERPROPERTY('ServerName') as ServerProperty;
GO

-- 3. Kiểm tra databases
SELECT '=== ALL DATABASES ===' as Info;
SELECT 
    name as DatabaseName,
    database_id,
    state_desc as Status,
    create_date
FROM sys.databases
ORDER BY name;
GO

-- 4. Kiểm tra Web_SIMS database
SELECT '=== WEB_SIMS DATABASE ===' as Info;
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Web_SIMS')
BEGIN
    PRINT '✓ Database Web_SIMS TỒN TẠI';
    SELECT 
        name as DatabaseName,
        state_desc as Status,
        create_date,
        recovery_model_desc as RecoveryModel
    FROM sys.databases 
    WHERE name = 'Web_SIMS';
END
ELSE
BEGIN
    PRINT '✗ Database Web_SIMS KHÔNG TỒN TẠI';
END
GO

-- 5. Kiểm tra connection
SELECT '=== CONNECTION TEST ===' as Info;
SELECT 
    DB_NAME() as CurrentDatabase,
    SYSTEM_USER as CurrentUser,
    GETDATE() as CurrentTime;
GO

PRINT '=== KẾT QUẢ ===';
PRINT 'Nếu thấy thông tin trên, SQL Server đang hoạt động bình thường';
PRINT 'Nếu database Web_SIMS không tồn tại, cần tạo lại'; 