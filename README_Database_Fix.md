# 🔧 HƯỚNG DẪN SỬA LỖI DATABASE VÀ ĐĂNG NHẬP

## 🚨 **VẤN ĐỀ ĐÃ PHÁT HIỆN**

### **1. Lỗi Database:**
- Database có thể chưa được tạo đúng cách
- Dữ liệu mẫu có thể bị thiếu
- Connection string có thể không đúng

### **2. Lỗi Đăng nhập:**
- Logic redirect sau đăng nhập không đúng
- Không kiểm tra role để redirect đúng trang

## ✅ **GIẢI PHÁP ĐÃ ÁP DỤNG**

### **1. Sửa Logic Đăng nhập:**
- ✅ **AccountController.cs:** Sửa logic redirect theo role
- ✅ **HomeController.cs:** Thêm redirect logic cho từng role
- ✅ **Navigation:** Cập nhật menu theo role

### **2. Sửa Database:**
- ✅ **Program.cs:** Đảm bảo database được tạo tự động
- ✅ **AppDbContext.cs:** Có dữ liệu mẫu đầy đủ
- ✅ **Database_Check.sql:** Script kiểm tra và sửa database

## 🛠️ **CÁCH SỬA LỖI**

### **Bước 1: Chạy Script Database**
```sql
-- Mở SQL Server Management Studio hoặc Azure Data Studio
-- Chạy file Database_Check.sql
```

### **Bước 2: Kiểm tra Connection String**
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Web_SIMS;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### **Bước 3: Chạy lại ứng dụng**
```bash
dotnet build
dotnet run
```

## 👥 **THÔNG TIN ĐĂNG NHẬP MẪU**

### **🔐 Tài khoản Admin:**
- **Username:** `admin`
- **Password:** `admin123`
- **Role:** Admin
- **Redirect:** Dashboard

### **👨‍🏫 Tài khoản Faculty:**
- **Username:** `faculty1`
- **Password:** `faculty123`
- **Role:** Faculty
- **Redirect:** Faculty/Home

### **👨‍🎓 Tài khoản Student:**
- **Username:** `student1`
- **Password:** `student123`
- **Role:** Student
- **Redirect:** StudentPortal/Home

## 🔍 **KIỂM TRA LỖI**

### **1. Kiểm tra Database:**
```sql
-- Kiểm tra bảng có tồn tại không
SELECT * FROM sys.tables WHERE name IN ('Users', 'Roles', 'Students', 'Courses');

-- Kiểm tra dữ liệu
SELECT COUNT(*) FROM Users;
SELECT COUNT(*) FROM Roles;
SELECT COUNT(*) FROM Students;
```

### **2. Kiểm tra Logs:**
- Mở Developer Tools (F12)
- Xem Console tab để kiểm tra lỗi JavaScript
- Xem Network tab để kiểm tra API calls

### **3. Kiểm tra Authentication:**
```csharp
// Trong AccountController.cs
_logger.LogInformation($"Đăng nhập thử nghiệm: {model.Username}");
```

## 🚀 **TESTING CHECKLIST**

### **✅ Admin Test:**
- [ ] Đăng nhập với admin/admin123
- [ ] Truy cập Dashboard
- [ ] Quản lý Students, Courses, Enrollments
- [ ] Xem tất cả dữ liệu

### **✅ Faculty Test:**
- [ ] Đăng nhập với faculty1/faculty123
- [ ] Truy cập Faculty/Home
- [ ] Xem MyClasses
- [ ] Nhập điểm cho sinh viên

### **✅ Student Test:**
- [ ] Đăng nhập với student1/student123
- [ ] Truy cập StudentPortal/Home
- [ ] Xem MyCourses
- [ ] Xem Profile

## 🔧 **TROUBLESHOOTING**

### **Lỗi "Database not found":**
```bash
dotnet ef database drop --force
dotnet ef database update
```

### **Lỗi "Login failed":**
1. Kiểm tra dữ liệu trong bảng Users
2. Kiểm tra password có đúng không
3. Kiểm tra IsActive = 1

### **Lỗi "Access Denied":**
1. Kiểm tra role trong bảng Users
2. Kiểm tra [Authorize] attributes
3. Kiểm tra navigation menu

### **Lỗi "Page not found":**
1. Kiểm tra tên Controller và Action
2. Kiểm tra routing trong Program.cs
3. Kiểm tra Views có tồn tại không

## 📞 **LIÊN HỆ HỖ TRỢ**

Nếu vẫn gặp vấn đề, hãy:
1. Kiểm tra logs trong Console
2. Chạy script Database_Check.sql
3. Restart ứng dụng
4. Clear browser cache

---

**🎯 Mục tiêu:** Đảm bảo tất cả 3 role (Admin, Faculty, Student) có thể đăng nhập và truy cập đúng chức năng của mình. 