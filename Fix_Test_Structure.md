# 🚨 PHÁT HIỆN VẤN ĐỀ VÀ GIẢI PHÁP

## ❌ **Vấn đề hiện tại:**

Test project đang **ảnh hưởng đến việc build dự án chính** vì:

1. **Thư mục `Web_SIMS.Tests` nằm TRONG thư mục main project**
2. **Dotnet build tự động compile** tất cả `.cs` files trong thư mục
3. **Test files thiếu dependencies** (xUnit, Moq, FluentAssertions) khi build với main project

## ✅ **Giải pháp:**

### **Cách 1: Di chuyển Test Project ra ngoài (KHUYẾN NGHỊ)**

```
📦 Cấu trúc MỚI:
├── 📁 Web_SIMS/                    (Main project)
│   ├── Controllers/
│   ├── Models/
│   ├── Views/
│   ├── Web_SIMS.csproj
│   └── Web_SIMS.sln
│
└── 📁 Web_SIMS.Tests/              (Test project - NGOÀI main folder)
    ├── Controllers/
    ├── TestHelper.cs
    ├── Web_SIMS.Tests.csproj
    └── Web_SIMS.Tests.sln
```

### **Cách 2: Sử dụng .csproj exclude patterns**

Thêm vào `Web_SIMS.csproj`:
```xml
<ItemGroup>
  <Compile Remove="Web_SIMS.Tests/**" />
</ItemGroup>
```

### **Cách 3: Tạo .gitignore cho build**

Thêm vào `.gitignore`:
```
Web_SIMS.Tests/bin/
Web_SIMS.Tests/obj/
```

## 🎯 **Khuyến nghị:**

**Sử dụng Cách 1** - Di chuyển test project ra ngoài vì:
- ✅ Tách biệt hoàn toàn
- ✅ Không ảnh hưởng build performance
- ✅ Cấu trúc rõ ràng
- ✅ CI/CD friendly

## 📋 **Các bước thực hiện:**

### **Bước 1: Tạo cấu trúc mới**
```bash
# Tạo thư mục test ngoài main project
mkdir ../Web_SIMS.Tests

# Di chuyển test files
mv Web_SIMS.Tests/* ../Web_SIMS.Tests/

# Xóa thư mục test cũ
rmdir Web_SIMS.Tests
```

### **Bước 2: Cập nhật project references**
```xml
<!-- Trong Web_SIMS.Tests.csproj -->
<ProjectReference Include="../Web_SIMS/Web_SIMS.csproj" />
```

### **Bước 3: Cập nhật solution**
```
<!-- Trong Web_SIMS.Tests.sln -->
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "Web_SIMS", "../Web_SIMS/Web_SIMS.csproj"
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "Web_SIMS.Tests", "Web_SIMS.Tests.csproj"
```

### **Bước 4: Test build**
```bash
# Test main project
cd Web_SIMS
dotnet build Web_SIMS.csproj

# Test solution
cd ../Web_SIMS.Tests  
dotnet build Web_SIMS.Tests.sln
```

## 🔍 **Kiểm tra sau khi fix:**

1. **Main project build clean**: `dotnet build Web_SIMS.csproj`
2. **Main project run OK**: `dotnet run --project Web_SIMS.csproj`
3. **Test project build OK**: `dotnet build Web_SIMS.Tests.sln`
4. **Tests run OK**: `dotnet test Web_SIMS.Tests.sln`

---

**Kết luận**: Test project hiện tại **ĐANG ẢNH HƯỞNG** đến main project và cần được tách biệt!