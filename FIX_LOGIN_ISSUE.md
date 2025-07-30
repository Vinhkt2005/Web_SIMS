# 🔧 HƯỚNG DẪN SỬA LỖI ĐĂNG NHẬP

## 🚨 **VẤN ĐỀ ĐÃ PHÁT HIỆN**

Từ hình ảnh bạn cung cấp, tôi thấy:
- **Username:** `faculty1` (đã nhập)
- **Password:** Trống (chưa nhập)
- **Lỗi:** "Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại sau."

## ✅ **NGUYÊN NHÂN VÀ GIẢI PHÁP**

### **1. Vấn đề Password trống:**
- **Nguyên nhân:** Bạn chưa nhập password
- **Giải pháp:** Nhập password `faculty123` vào ô mật khẩu

### **2. Vấn đề Database:**
- **Nguyên nhân:** Database có thể chưa có dữ liệu đúng
- **Giải pháp:** Chạy script `Fix_Login_Data.sql`

### **3. Vấn đề Logging:**
- **Nguyên nhân:** Không có thông tin debug
- **Giải pháp:** Đã thêm logging chi tiết vào `AccountController.cs`

## 🛠️ **CÁCH SỬA LỖI**

### **Bước 1: Chạy Script Database**
```sql
-- Mở SQL Server Management Studio hoặc Azure Data Studio
-- Chạy file Fix_Login_Data.sql
```

### **Bước 2: Kiểm tra dữ liệu**
```sql
-- Kiểm tra Users table
SELECT Username, Password, FullName, RoleId, IsActive 
FROM Users 
WHERE IsActive = 1;

-- Kiểm tra Roles table
SELECT * FROM Roles;
```

### **Bước 3: Test đăng nhập**
1. **Truy cập:** http://localhost:5267
2. **Đăng nhập với:**
   - **Username:** `faculty1`
   - **Password:** `faculty123` (⚠️ **QUAN TRỌNG:** Nhập đầy đủ password)

### **Bước 4: Kiểm tra Logs**
- Mở Developer Tools (F12)
- Xem Console tab
- Kiểm tra Network tab khi đăng nhập

## 👥 **THÔNG TIN ĐĂNG NHẬP CHÍNH XÁC**

### **🔐 Admin:**
- **Username:** `admin`
- **Password:** `admin123`
- **Role:** Admin

### **👨‍🏫 Faculty:**
- **Username:** `faculty1`
- **Password:** `faculty123` ⚠️ **NHẬP ĐẦY ĐỦ**
- **Role:** Faculty

### **👨‍🎓 Student:**
- **Username:** `student1`
- **Password:** `student123`
- **Role:** Student

## 🔍 **DEBUGGING STEPS**

### **1. Kiểm tra Database:**
```sql
-- Kiểm tra xem có dữ liệu không
SELECT COUNT(*) FROM Users;
SELECT COUNT(*) FROM Roles;

-- Kiểm tra user cụ thể
SELECT u.Username, u.Password, u.IsActive, r.RoleName
FROM Users u
JOIN Roles r ON u.RoleId = r.RoleId
WHERE u.Username = 'faculty1';
```

### **2. Kiểm tra Logs:**
- Mở terminal chạy `dotnet run`
- Xem logs khi đăng nhập
- Tìm thông tin debug về password verification

### **3. Kiểm tra Browser:**
- Mở Developer Tools (F12)
- Xem Console tab
- Xem Network tab khi submit form

## 🚀 **TESTING CHECKLIST**

### **✅ Test Faculty Login:**
- [ ] Nhập username: `faculty1`
- [ ] Nhập password: `faculty123` (đầy đủ)
- [ ] Click "Đăng nhập"
- [ ] Kiểm tra redirect đến `/Faculty/Home`

### **✅ Test Admin Login:**
- [ ] Nhập username: `admin`
- [ ] Nhập password: `admin123`
- [ ] Click "Đăng nhập"
- [ ] Kiểm tra redirect đến `/Dashboard/Index`

### **✅ Test Student Login:**
- [ ] Nhập username: `student1`
- [ ] Nhập password: `student123`
- [ ] Click "Đăng nhập"
- [ ] Kiểm tra redirect đến `/StudentPortal/Home`

## 🔧 **TROUBLESHOOTING**

### **Lỗi "Password trống":**
- ✅ Đảm bảo nhập đầy đủ password
- ✅ Kiểm tra không có khoảng trắng thừa

### **Lỗi "User không tồn tại":**
- ✅ Chạy script `Fix_Login_Data.sql`
- ✅ Kiểm tra database có dữ liệu không

### **Lỗi "Password không đúng":**
- ✅ Kiểm tra password trong database
- ✅ Đảm bảo nhập đúng password

### **Lỗi "Database connection":**
- ✅ Kiểm tra connection string
- ✅ Restart ứng dụng

## 📞 **LIÊN HỆ HỖ TRỢ**

Nếu vẫn gặp vấn đề:
1. **Chạy script:** `Fix_Login_Data.sql`
2. **Kiểm tra logs:** Terminal và Browser Console
3. **Test với password đầy đủ:** `faculty123`
4. **Restart ứng dụng:** `dotnet run`

---

**🎯 Mục tiêu:** Đảm bảo đăng nhập thành công với password đầy đủ và đúng. 