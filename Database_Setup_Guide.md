# Hướng dẫn Thiết lập Database cho Hệ thống SIMS

## Tổng quan

Dự án Web_SIMS (Student Information Management System) sử dụng SQL Server với Entity Framework Core. Bộ script này cung cấp đầy đủ các công cụ để tạo, quản lý và bảo trì database.

## Cấu trúc Files

### 1. `Create_Database_Complete.sql`
**Mục đích**: Script chính để tạo database hoàn chỉnh từ đầu
**Nội dung**:
- Tạo database Web_SIMS
- Tạo tất cả bảng với đầy đủ constraints
- Tạo indexes để tối ưu hiệu suất
- Tạo foreign keys
- Insert dữ liệu mẫu đầy đủ
- Tạo views báo cáo
- Tạo stored procedures
- Tạo functions hỗ trợ

### 2. `Database_Backup_Restore.sql`
**Mục đích**: Script backup và restore database
**Nội dung**:
- Backup full database
- Backup transaction log
- Restore database
- Kiểm tra backup history
- Tạo job backup tự động
- Export data to CSV

### 3. `Reset_Database.sql`
**Mục đích**: Script reset hoàn toàn database
**Nội dung**:
- Backup database trước khi xóa (tùy chọn)
- Xóa database cũ
- Tạo database mới
- Tạo cấu trúc cơ bản
- Insert dữ liệu mẫu tối thiểu

### 4. `Database_Setup_Guide.md`
**Mục đích**: Hướng dẫn sử dụng (file này)

## Cấu trúc Database

### Bảng chính:
1. **Roles**: Vai trò người dùng (Admin, Faculty, Student)
2. **Students**: Thông tin sinh viên
3. **Faculties**: Thông tin giảng viên
4. **Users**: Tài khoản người dùng
5. **Courses**: Thông tin khóa học
6. **Enrollments**: Đăng ký khóa học
7. **AcademicRecords**: Bảng điểm

### Relationships:
- Users ↔ Roles (Many-to-One)
- Users ↔ Students (One-to-One)
- Users ↔ Faculties (One-to-One)
- Courses ↔ Faculties (Many-to-One)
- Enrollments ↔ Students (Many-to-One)
- Enrollments ↔ Courses (Many-to-One)
- AcademicRecords ↔ Students (Many-to-One)
- AcademicRecords ↔ Courses (Many-to-One)

## Hướng dẫn Sử dụng

### Lần đầu thiết lập Database

1. **Mở SQL Server Management Studio (SSMS)**
2. **Kết nối đến SQL Server instance**
3. **Chạy script tạo database hoàn chỉnh**:
   ```sql
   -- Mở file Create_Database_Complete.sql và chạy toàn bộ
   ```

### Kiểm tra kết quả:
```sql
USE Web_SIMS
GO

-- Kiểm tra số lượng bảng
SELECT COUNT(*) as TableCount FROM sys.tables WHERE type = 'U'

-- Kiểm tra dữ liệu
SELECT 'Roles' as TableName, COUNT(*) as RecordCount FROM Roles
UNION ALL
SELECT 'Students', COUNT(*) FROM Students
UNION ALL
SELECT 'Users', COUNT(*) FROM Users
```

### Reset Database (khi cần)

**⚠️ CẢNH BÁO**: Script này sẽ xóa hoàn toàn database hiện tại!

1. **Backup database trước** (nếu cần):
   ```sql
   -- Uncomment phần backup trong Reset_Database.sql
   ```

2. **Chạy script reset**:
   ```sql
   -- Mở file Reset_Database.sql và chạy toàn bộ
   ```

### Backup Database

#### Backup thủ công:
```sql
-- Mở file Database_Backup_Restore.sql
-- Chạy phần "1. BACKUP DATABASE"
```

#### Thiết lập backup tự động:
```sql
-- Uncomment và chạy phần "5. SCRIPT TẠO BACKUP JOB TỰ ĐỘNG"
-- Cần SQL Server Agent đang chạy
```

### Restore Database

1. **Tìm file backup**:
   ```
   Đường dẫn mặc định: C:\DatabaseBackups\
   ```

2. **Chỉnh sửa đường dẫn trong script**:
   ```sql
   -- Trong Database_Backup_Restore.sql, phần "3. SCRIPT RESTORE DATABASE"
   -- Thay đổi đường dẫn file backup
   ```

3. **Chạy script restore**

## Tài khoản Mặc định

Sau khi chạy script, bạn có thể đăng nhập với các tài khoản sau:

| Vai trò | Username | Password | Email |
|---------|----------|----------|-------|
| Admin | admin | admin123 | admin@sims.edu |
| Faculty | faculty1 | faculty123 | nguyen.giang@university.edu.vn |
| Student | student1 | student123 | nguyenvana@sims.edu |

## Connection String

Cập nhật connection string trong `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=Web_SIMS;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;Encrypt=false"
  }
}
```

### Cho SQL Server thật:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=Web_SIMS;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true;Encrypt=false"
  }
}
```

## Views và Stored Procedures

### Views có sẵn:
1. **vw_StudentEnrollments**: Thông tin đăng ký khóa học của sinh viên
2. **vw_StudentGrades**: Bảng điểm chi tiết
3. **vw_CourseStatistics**: Thống kê khóa học

### Stored Procedures:
1. **sp_GetStudentByCode**: Lấy thông tin sinh viên theo mã
2. **sp_EnrollStudent**: Đăng ký sinh viên vào khóa học
3. **sp_UpdateGrade**: Cập nhật điểm số

### Functions:
1. **fn_CalculateGPA**: Tính GPA của sinh viên

## Ví dụ Sử dụng

### Lấy thông tin sinh viên:
```sql
EXEC sp_GetStudentByCode 'SV001'
```

### Đăng ký khóa học:
```sql
EXEC sp_EnrollStudent @StudentId = 1, @CourseId = 1, @Notes = N'Đăng ký bình thường'
```

### Cập nhật điểm:
```sql
EXEC sp_UpdateGrade 
    @StudentId = 1, 
    @CourseId = 1, 
    @Semester = 1, 
    @AcademicYear = 2024,
    @MidtermScore = 8.5,
    @FinalScore = 9.0
```

### Xem báo cáo:
```sql
-- Xem đăng ký khóa học
SELECT * FROM vw_StudentEnrollments WHERE StudentCode = 'SV001'

-- Xem bảng điểm
SELECT * FROM vw_StudentGrades WHERE StudentCode = 'SV001'

-- Thống kê khóa học
SELECT * FROM vw_CourseStatistics
```

## Troubleshooting

### Lỗi thường gặp:

1. **"Database already exists"**:
   - Chạy script Reset_Database.sql để xóa và tạo lại

2. **"Permission denied"**:
   - Đảm bảo user có quyền tạo database
   - Chạy SSMS với quyền Administrator

3. **"Invalid object name"**:
   - Kiểm tra đã chọn đúng database: `USE Web_SIMS`

4. **Connection string error**:
   - Kiểm tra server name và authentication method
   - Đảm bảo SQL Server đang chạy

### Kiểm tra tình trạng database:
```sql
-- Kiểm tra database tồn tại
SELECT name FROM sys.databases WHERE name = 'Web_SIMS'

-- Kiểm tra bảng
USE Web_SIMS
SELECT name FROM sys.tables WHERE type = 'U'

-- Kiểm tra dữ liệu
SELECT COUNT(*) FROM Users
```

## Bảo trì Database

### Backup định kỳ:
- Thiết lập job backup tự động
- Backup trước khi update lớn
- Lưu trữ backup ở vị trí an toàn

### Monitoring:
- Kiểm tra log files
- Monitor database size
- Kiểm tra performance

### Security:
- Thay đổi mật khẩu mặc định
- Thiết lập user permissions
- Enable encryption nếu cần

## Liên hệ

Nếu có vấn đề khi thiết lập database, vui lòng:
1. Kiểm tra log errors
2. Xem lại connection string
3. Đảm bảo SQL Server đang chạy
4. Kiểm tra permissions

---

**Lưu ý**: Luôn backup database trước khi thực hiện các thay đổi lớn!