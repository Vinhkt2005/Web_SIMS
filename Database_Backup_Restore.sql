-- =============================================
-- Script Backup và Restore Database cho hệ thống SIMS
-- Phiên bản: 1.0
-- Ngày tạo: 2025-08-04
-- Mô tả: Script để backup và restore database Web_SIMS
-- =============================================

USE master
GO

-- =============================================
-- 1. BACKUP DATABASE
-- =============================================

-- Tạo thư mục backup nếu chưa tồn tại (cần quyền admin)
DECLARE @BackupPath NVARCHAR(500) = 'C:\DatabaseBackups\'
DECLARE @SQL NVARCHAR(1000)

-- Kiểm tra và tạo thư mục backup
EXEC xp_create_subdir @BackupPath

-- Backup Full Database
DECLARE @BackupFileName NVARCHAR(500)
DECLARE @DateTime NVARCHAR(20) = REPLACE(REPLACE(REPLACE(CONVERT(VARCHAR, GETDATE(), 120), '-', ''), ':', ''), ' ', '_')

SET @BackupFileName = @BackupPath + 'Web_SIMS_Full_' + @DateTime + '.bak'

PRINT 'Bắt đầu backup database Web_SIMS...'
PRINT 'File backup: ' + @BackupFileName

BACKUP DATABASE [Web_SIMS] 
TO DISK = @BackupFileName
WITH 
    FORMAT,
    INIT,
    NAME = 'Web_SIMS Full Database Backup',
    DESCRIPTION = 'Full backup of Web_SIMS database',
    COMPRESSION,
    CHECKSUM,
    STATS = 10

PRINT 'Backup hoàn thành!'
GO

-- =============================================
-- 2. BACKUP LOG (nếu database ở Recovery Model FULL)
-- =============================================

-- Kiểm tra Recovery Model
IF (SELECT recovery_model_desc FROM sys.databases WHERE name = 'Web_SIMS') = 'FULL'
BEGIN
    DECLARE @LogBackupPath NVARCHAR(500) = 'C:\DatabaseBackups\'
    DECLARE @LogBackupFileName NVARCHAR(500)
    DECLARE @LogDateTime NVARCHAR(20) = REPLACE(REPLACE(REPLACE(CONVERT(VARCHAR, GETDATE(), 120), '-', ''), ':', ''), ' ', '_')
    
    SET @LogBackupFileName = @LogBackupPath + 'Web_SIMS_Log_' + @LogDateTime + '.trn'
    
    PRINT 'Bắt đầu backup transaction log...'
    PRINT 'File log backup: ' + @LogBackupFileName
    
    BACKUP LOG [Web_SIMS] 
    TO DISK = @LogBackupFileName
    WITH 
        FORMAT,
        INIT,
        NAME = 'Web_SIMS Transaction Log Backup',
        DESCRIPTION = 'Transaction log backup of Web_SIMS database',
        COMPRESSION,
        CHECKSUM,
        STATS = 10
    
    PRINT 'Log backup hoàn thành!'
END
ELSE
BEGIN
    PRINT 'Database đang ở Simple Recovery Model - không cần backup log'
END
GO

-- =============================================
-- 3. SCRIPT RESTORE DATABASE
-- =============================================

/*
-- CÁCH SỬ DỤNG RESTORE:
-- 1. Thay đổi đường dẫn file backup phù hợp
-- 2. Uncomment và chạy script dưới đây

USE master
GO

-- Đóng tất cả kết nối đến database
ALTER DATABASE [Web_SIMS] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
GO

-- Restore database từ file backup
RESTORE DATABASE [Web_SIMS] 
FROM DISK = 'C:\DatabaseBackups\Web_SIMS_Full_20250804_143000.bak'
WITH 
    REPLACE,
    STATS = 10,
    CHECKSUM

-- Đưa database về chế độ multi-user
ALTER DATABASE [Web_SIMS] SET MULTI_USER
GO

PRINT 'Restore database hoàn thành!'
*/

-- =============================================
-- 4. SCRIPT KIỂM TRA BACKUP
-- =============================================

-- Kiểm tra thông tin backup gần nhất
SELECT 
    database_name,
    backup_start_date,
    backup_finish_date,
    type,
    CASE type
        WHEN 'D' THEN 'Full Database'
        WHEN 'I' THEN 'Differential'
        WHEN 'L' THEN 'Transaction Log'
        ELSE 'Other'
    END AS backup_type,
    backup_size/1024/1024 AS backup_size_mb,
    compressed_backup_size/1024/1024 AS compressed_size_mb,
    physical_device_name
FROM msdb.dbo.backupset bs
INNER JOIN msdb.dbo.backupmediafamily bmf ON bs.media_set_id = bmf.media_set_id
WHERE database_name = 'Web_SIMS'
ORDER BY backup_start_date DESC

-- =============================================
-- 5. SCRIPT TẠO BACKUP JOB TỰ ĐỘNG (SQL Server Agent)
-- =============================================

/*
-- Tạo job backup tự động hàng ngày
-- Cần SQL Server Agent đang chạy

USE msdb
GO

-- Tạo job
EXEC dbo.sp_add_job
    @job_name = N'Web_SIMS Daily Backup'

-- Tạo job step
EXEC sp_add_jobstep
    @job_name = N'Web_SIMS Daily Backup',
    @step_name = N'Backup Database',
    @subsystem = N'TSQL',
    @command = N'
DECLARE @BackupPath NVARCHAR(500) = ''C:\DatabaseBackups\''
DECLARE @BackupFileName NVARCHAR(500)
DECLARE @DateTime NVARCHAR(20) = REPLACE(REPLACE(REPLACE(CONVERT(VARCHAR, GETDATE(), 120), ''-'', ''''), '':'', ''''), '' '', ''_'')

SET @BackupFileName = @BackupPath + ''Web_SIMS_Daily_'' + @DateTime + ''.bak''

BACKUP DATABASE [Web_SIMS] 
TO DISK = @BackupFileName
WITH 
    FORMAT,
    INIT,
    NAME = ''Web_SIMS Daily Backup'',
    COMPRESSION,
    CHECKSUM,
    STATS = 10
',
    @retry_attempts = 3,
    @retry_interval = 5

-- Tạo schedule (chạy hàng ngày lúc 2:00 AM)
EXEC dbo.sp_add_schedule
    @schedule_name = N'Daily at 2 AM',
    @freq_type = 4,
    @freq_interval = 1,
    @active_start_time = 020000

-- Gán schedule cho job
EXEC sp_attach_schedule
   @job_name = N'Web_SIMS Daily Backup',
   @schedule_name = N'Daily at 2 AM'

-- Gán job cho server
EXEC dbo.sp_add_jobserver
    @job_name = N'Web_SIMS Daily Backup'

PRINT 'Job backup tự động đã được tạo!'
*/

-- =============================================
-- 6. SCRIPT MAINTENANCE - SHRINK DATABASE
-- =============================================

/*
-- Chỉ sử dụng khi cần thiết, không nên dùng thường xuyên
USE [Web_SIMS]
GO

-- Shrink database files
DBCC SHRINKDATABASE([Web_SIMS], 10)

-- Shrink log file
DBCC SHRINKFILE([Web_SIMS_Log], 1)

PRINT 'Database maintenance hoàn thành!'
*/

-- =============================================
-- 7. SCRIPT KIỂM TRA TÌNH TRẠNG DATABASE
-- =============================================

USE [Web_SIMS]
GO

-- Kiểm tra tình trạng database
SELECT 
    name AS DatabaseName,
    state_desc AS State,
    recovery_model_desc AS RecoveryModel,
    collation_name AS Collation,
    create_date AS CreateDate,
    (SELECT COUNT(*) FROM sys.tables WHERE type = 'U') AS TableCount,
    (SELECT COUNT(*) FROM sys.views WHERE type = 'V') AS ViewCount,
    (SELECT COUNT(*) FROM sys.procedures WHERE type = 'P') AS ProcedureCount
FROM sys.databases 
WHERE name = 'Web_SIMS'

-- Kiểm tra kích thước database
SELECT 
    DB_NAME() AS DatabaseName,
    name AS LogicalName,
    physical_name AS PhysicalName,
    type_desc AS FileType,
    size * 8 / 1024 AS SizeMB,
    max_size * 8 / 1024 AS MaxSizeMB,
    growth AS Growth,
    is_percent_growth AS IsPercentGrowth
FROM sys.database_files

-- Kiểm tra số lượng records trong các bảng
SELECT 
    t.name AS TableName,
    p.rows AS RecordCount
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0, 1)
ORDER BY p.rows DESC

PRINT 'Kiểm tra database hoàn thành!'
GO

-- =============================================
-- 8. SCRIPT EXPORT DATA TO CSV
-- =============================================

/*
-- Export dữ liệu ra file CSV (cần xp_cmdshell enabled)

-- Enable xp_cmdshell (cần quyền sysadmin)
EXEC sp_configure 'show advanced options', 1
RECONFIGURE
EXEC sp_configure 'xp_cmdshell', 1
RECONFIGURE

-- Export Students table
EXEC xp_cmdshell 'bcp "SELECT * FROM Web_SIMS.dbo.Students" queryout "C:\DatabaseBackups\Students_Export.csv" -c -t"," -T -S .\SQLEXPRESS'

-- Export Courses table
EXEC xp_cmdshell 'bcp "SELECT * FROM Web_SIMS.dbo.Courses" queryout "C:\DatabaseBackups\Courses_Export.csv" -c -t"," -T -S .\SQLEXPRESS'

-- Export Enrollments table
EXEC xp_cmdshell 'bcp "SELECT * FROM Web_SIMS.dbo.Enrollments" queryout "C:\DatabaseBackups\Enrollments_Export.csv" -c -t"," -T -S .\SQLEXPRESS'

-- Disable xp_cmdshell for security
EXEC sp_configure 'xp_cmdshell', 0
RECONFIGURE
EXEC sp_configure 'show advanced options', 0
RECONFIGURE

PRINT 'Export dữ liệu hoàn thành!'
*/

PRINT '============================================='
PRINT 'SCRIPT BACKUP VÀ RESTORE ĐÃ SẴN SÀNG!'
PRINT 'Hãy kiểm tra và điều chỉnh đường dẫn backup phù hợp'
PRINT 'Đường dẫn mặc định: C:\DatabaseBackups\'
PRINT '============================================='