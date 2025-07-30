# 🔍 HƯỚNG DẪN KIỂM TRA SQL SERVER

## 🚨 **VẤN ĐỀ: Database bị xóa**

Database của bạn đã bị xóa. Hãy kiểm tra SQL Server theo các bước sau:

## ✅ **BƯỚC 1: KIỂM TRA SQL SERVER**

### **1. Mở SQL Server Management Studio (SSMS)**
- Tìm và mở **SQL Server Management Studio**
- Kết nối đến server: `QUANGVINH` (theo connection string)

### **2. Chạy script kiểm tra nhanh**
```sql
-- Chạy script: Quick_SQL_Check.sql
-- Hoặc chạy từng phần:

-- Kiểm tra SQL Server version
SELECT @@VERSION as Version;

-- Kiểm tra server name
SELECT @@SERVERNAME as ServerName;

-- Kiểm tra tất cả databases
SELECT name as DatabaseName, state_desc as Status 
FROM sys.databases ORDER BY name;

-- Kiểm tra Web_SIMS database
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Web_SIMS')
    PRINT '✓ Database Web_SIMS TỒN TẠI'
ELSE
    PRINT '✗ Database Web_SIMS KHÔNG TỒN TẠI'
```

## ✅ **BƯỚC 2: TẠO LẠI DATABASE**

### **Nếu database không tồn tại:**

#### **Phương pháp 1: Chạy Script SQL**
1. Mở **SQL Server Management Studio**
2. Kết nối đến server `QUANGVINH`
3. Chạy script: `Recreate_Database.sql`
4. Kiểm tra thông báo "DATABASE ĐÃ ĐƯỢC TẠO LẠI THÀNH CÔNG"

#### **Phương pháp 2: Chạy ứng dụng**
```bash
dotnet run
```
Ứng dụng sẽ tự động tạo database và seed dữ liệu.

## ✅ **BƯỚC 3: KIỂM TRA DỮ LIỆU**

### **Chạy script kiểm tra chi tiết:**
```sql
-- Chạy script: Check_SQL_Server.sql
-- Hoặc kiểm tra từng bảng:

USE Web_SIMS;

-- Kiểm tra Roles
SELECT COUNT(*) as RolesCount FROM Roles;
SELECT * FROM Roles;

-- Kiểm tra Users
SELECT COUNT(*) as UsersCount FROM Users;
SELECT Username, Password, FullName, IsActive FROM Users;

-- Kiểm tra Students
SELECT COUNT(*) as StudentsCount FROM Students;
SELECT StudentCode, FullName, Email FROM Students;

-- Kiểm tra Courses
SELECT COUNT(*) as CoursesCount FROM Courses;
SELECT CourseCode, CourseName, Instructor FROM Courses;
```

## ✅ **BƯỚC 4: TEST ĐĂNG NHẬP**

### **1. Truy cập ứng dụng:**
```
http://localhost:5267
```

### **2. Test đăng nhập:**

#### **🔐 Admin:**
- **Username:** `admin`
- **Password:** `admin123`

#### **👨‍🏫 Faculty:**
- **Username:** `faculty1`
- **Password:** `faculty123` ⚠️ **NHẬP ĐẦY ĐỦ**

#### **👨‍🎓 Student:**
- **Username:** `student1`
- **Password:** `student123`

## 🔧 **TROUBLESHOOTING**

### **Lỗi "Cannot connect to SQL Server":**
1. Kiểm tra SQL Server service đang chạy
2. Kiểm tra server name: `QUANGVINH`
3. Kiểm tra Windows Authentication

### **Lỗi "Database not found":**
1. Chạy script `Recreate_Database.sql`
2. Hoặc chạy `dotnet run`

### **Lỗi "Login failed":**
1. Kiểm tra dữ liệu trong bảng Users
2. Đảm bảo nhập đầy đủ password
3. Kiểm tra IsActive = 1

## 📋 **CHECKLIST KIỂM TRA**

### **✅ SQL Server:**
- [ ] SQL Server Management Studio mở được
- [ ] Kết nối đến server `QUANGVINH` thành công
- [ ] Chạy script `Quick_SQL_Check.sql`

### **✅ Database:**
- [ ] Database Web_SIMS tồn tại
- [ ] Có đủ các bảng: Roles, Users, Students, Courses
- [ ] Có dữ liệu trong các bảng

### **✅ Ứng dụng:**
- [ ] Chạy `dotnet run` thành công
- [ ] Truy cập http://localhost:5267
- [ ] Test đăng nhập thành công

## 📞 **LIÊN HỆ HỖ TRỢ**

Nếu gặp vấn đề:
1. **Chạy script:** `Quick_SQL_Check.sql` để kiểm tra SQL Server
2. **Chạy script:** `Recreate_Database.sql` để tạo lại database
3. **Chạy ứng dụng:** `dotnet run` để tự động tạo
4. **Kiểm tra logs:** Terminal để debug

---

**🎯 Mục tiêu:** Kiểm tra SQL Server và khôi phục database để đăng nhập hoạt động. 