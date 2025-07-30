# 🔧 HƯỚNG DẪN TEST ĐĂNG NHẬP

## 🚨 **VẤN ĐỀ ĐÃ SỬA**

### **1. Database Seeding:**
- ✅ Thêm method `SeedData()` vào `AppDbContext.cs`
- ✅ Sửa `Program.cs` để gọi seeding khi khởi tạo
- ✅ Sửa lỗi model (Role không có CreatedDate, EnrollmentStatus enum)

### **2. Logging:**
- ✅ Thêm logging chi tiết vào `AccountController.cs`
- ✅ Hiển thị thông tin debug về password verification

## 🛠️ **CÁCH TEST ĐĂNG NHẬP**

### **Bước 1: Truy cập ứng dụng**
```
http://localhost:5267
```

### **Bước 2: Test đăng nhập Faculty**
1. **Username:** `faculty1`
2. **Password:** `faculty123` ⚠️ **NHẬP ĐẦY ĐỦ**
3. **Click:** "Đăng nhập"
4. **Kết quả mong đợi:** Redirect đến `/Faculty/Home`

### **Bước 3: Test đăng nhập Admin**
1. **Username:** `admin`
2. **Password:** `admin123`
3. **Click:** "Đăng nhập"
4. **Kết quả mong đợi:** Redirect đến `/Dashboard/Index`

### **Bước 4: Test đăng nhập Student**
1. **Username:** `student1`
2. **Password:** `student123`
3. **Click:** "Đăng nhập"
4. **Kết quả mong đợi:** Redirect đến `/StudentPortal/Home`

## 🔍 **DEBUGGING STEPS**

### **1. Kiểm tra Terminal Logs:**
- Mở terminal chạy `dotnet run`
- Xem logs khi đăng nhập
- Tìm thông tin:
  ```
  Đang tìm kiếm user: faculty1
  Tìm thấy user: faculty1, Role: Faculty, IsActive: True
  Kiểm tra password cho user: faculty1
  Input password: faculty123
  Stored password: faculty123
  Using plain text comparison: True
  ```

### **2. Kiểm tra Browser Console:**
- Mở Developer Tools (F12)
- Xem Console tab
- Xem Network tab khi submit form

### **3. Kiểm tra Database:**
```sql
-- Chạy script Quick_Test_Login.sql
-- Kiểm tra dữ liệu có đúng không
```

## 🚀 **TESTING CHECKLIST**

### **✅ Test 1: Faculty Login**
- [ ] Nhập username: `faculty1`
- [ ] Nhập password: `faculty123` (đầy đủ)
- [ ] Click "Đăng nhập"
- [ ] Kiểm tra redirect đến `/Faculty/Home`
- [ ] Kiểm tra logs trong terminal

### **✅ Test 2: Admin Login**
- [ ] Nhập username: `admin`
- [ ] Nhập password: `admin123`
- [ ] Click "Đăng nhập"
- [ ] Kiểm tra redirect đến `/Dashboard/Index`

### **✅ Test 3: Student Login**
- [ ] Nhập username: `student1`
- [ ] Nhập password: `student123`
- [ ] Click "Đăng nhập"
- [ ] Kiểm tra redirect đến `/StudentPortal/Home`

## 🔧 **TROUBLESHOOTING**

### **Lỗi "Password trống":**
- ✅ Đảm bảo nhập đầy đủ password
- ✅ Kiểm tra không có khoảng trắng thừa

### **Lỗi "User không tồn tại":**
- ✅ Kiểm tra database có dữ liệu không
- ✅ Chạy script `Quick_Test_Login.sql`

### **Lỗi "Password không đúng":**
- ✅ Kiểm tra password trong database
- ✅ Đảm bảo nhập đúng password

### **Lỗi "Database connection":**
- ✅ Kiểm tra connection string
- ✅ Restart ứng dụng

## 📞 **LIÊN HỆ HỖ TRỢ**

Nếu vẫn gặp vấn đề:
1. **Kiểm tra logs:** Terminal và Browser Console
2. **Test với password đầy đủ:** `faculty123`
3. **Chạy script:** `Quick_Test_Login.sql`
4. **Restart ứng dụng:** `dotnet run`

---

**🎯 Mục tiêu:** Đảm bảo đăng nhập thành công với password đầy đủ và đúng. 