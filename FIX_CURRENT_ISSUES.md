# 🔧 GIẢI QUYẾT CÁC LỖI HIỆN TẠI

## 🚨 **CÁC LỖI ĐÃ PHÁT HIỆN**

### **1. Lỗi SSL Certificate:**
```
The certificate chain was issued by an authority that is not trusted.
```
**✅ ĐÃ SỬA:** Thêm `TrustServerCertificate=true;Encrypt=false` vào connection string

### **2. Lỗi Database không tồn tại:**
Database bị xóa khi chạy `dotnet ef database drop --force`

### **3. Lỗi Entity Framework Relationships:**
```
The foreign key property 'User.StudentId1' was created in shadow state
```

## ✅ **GIẢI PHÁP TỪNG BƯỚC**

### **Bước 1: Sửa Connection String**
Connection string đã được cập nhật trong `appsettings.json`:
```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=Web_SIMS;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;Encrypt=false"
```

### **Bước 2: Tạo lại Database**

#### **Phương pháp 1: Chạy Script SQL (Khuyến nghị)**
1. Mở **SQL Server Management Studio**
2. Kết nối đến: `(localdb)\MSSQLLocalDB`
3. Chạy script: `Recreate_LocalDB.sql`
4. Kiểm tra thông báo "DATABASE ĐÃ ĐƯỢC TẠO LẠI THÀNH CÔNG"

#### **Phương pháp 2: Chạy ứng dụng tự động**
```bash
dotnet run
```
Ứng dụng sẽ tự động tạo database và seed dữ liệu.

### **Bước 3: Kiểm tra Database**
Chạy script: `Check_LocalDB.sql` để kiểm tra:
- SQL Server LocalDB có hoạt động không
- Database Web_SIMS có tồn tại không
- Các bảng có đủ dữ liệu không

## 🔍 **KIỂM TRA CHI TIẾT**

### **1. Kiểm tra SQL Server LocalDB:**
```sql
-- Chạy script: Check_LocalDB.sql
-- Hoặc chạy từng phần:

-- Kiểm tra version
SELECT @@VERSION as Version;

-- Kiểm tra databases
SELECT name as DatabaseName, state_desc as Status 
FROM sys.databases ORDER BY name;

-- Kiểm tra Web_SIMS
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Web_SIMS')
    PRINT '✓ Database Web_SIMS TỒN TẠI'
ELSE
    PRINT '✗ Database Web_SIMS KHÔNG TỒN TẠI'
```

### **2. Kiểm tra dữ liệu:**
```sql
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

## 🚀 **TEST ĐĂNG NHẬP**

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

### **Lỗi "Cannot connect to LocalDB":**
1. Kiểm tra SQL Server LocalDB đã cài đặt
2. Chạy: `sqllocaldb start "MSSQLLocalDB"`
3. Kiểm tra connection string

### **Lỗi "Database not found":**
1. Chạy script `Recreate_LocalDB.sql`
2. Hoặc chạy `dotnet run`

### **Lỗi "Login failed":**
1. Kiểm tra dữ liệu trong bảng Users
2. Đảm bảo nhập đầy đủ password
3. Kiểm tra IsActive = 1

### **Lỗi "SSL Certificate":**
1. Đã thêm `TrustServerCertificate=true;Encrypt=false`
2. Restart ứng dụng

## 📋 **CHECKLIST GIẢI QUYẾT**

### **✅ Connection String:**
- [ ] Đã thêm `TrustServerCertificate=true;Encrypt=false`
- [ ] Server name: `(localdb)\MSSQLLocalDB`
- [ ] Database name: `Web_SIMS`

### **✅ Database:**
- [ ] Chạy script `Recreate_LocalDB.sql`
- [ ] Kiểm tra database tồn tại
- [ ] Kiểm tra dữ liệu trong các bảng

### **✅ Ứng dụng:**
- [ ] Chạy `dotnet run` thành công
- [ ] Không còn lỗi SSL
- [ ] Truy cập http://localhost:5267
- [ ] Test đăng nhập thành công

## 📞 **LIÊN HỆ HỖ TRỢ**

Nếu vẫn gặp vấn đề:
1. **Chạy script:** `Check_LocalDB.sql` để kiểm tra
2. **Chạy script:** `Recreate_LocalDB.sql` để tạo lại database
3. **Chạy ứng dụng:** `dotnet run` để tự động tạo
4. **Kiểm tra logs:** Terminal để debug

---

**🎯 Mục tiêu:** Giải quyết lỗi SSL và khôi phục database để đăng nhập hoạt động. 