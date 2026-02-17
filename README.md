# AnhEmMotor Backend API

**Copyright (C) 2026 Tran Thanh Binh, Nguyen Huynh Kim Ngan, Nguyen Trinh Anh Khoi, Trinh Minh Uyen.**

This project is licensed under the **Apache License 2.0**.
See the [LICENSE](LICENSE) file for details.

> **🚨 CẢNH BÁO QUAN TRỌNG CHO NGƯỜI MỚI:**
>
> **Dự án này sẽ cần chạy SQL Server trên máy local và MySQL trên VPS.**
> **Khi thay đổi cấu trúc database (thêm bảng, sửa cột, etc.), BẮT BUỘC phải tạo migration theo cách thức dưới đây!**
>
> ```powershell
> # Chạy lệnh này mỗi khi thay đổi Entity/DbContext:
> .\add-migration.ps1 "TenMigration"
> ```
>
> **Nếu quên tạo MySQL migration:**
>
> - ✅ Code mới được deploy lên VPS
> - ❌ Database KHÔNG được update
> - 💥 **Application sẽ CRASH khi chạy!**
>
> ➡️ Chi tiết xem [Section 3. Tạo và Quản Lý Database Migrations](#3-tạo-và-quản-lý-database-migrations)



# 1. Yêu cầu hệ thống

Máy tính lập trình nên dùng hệ điều hành Windows để có trải nghiệm lập trình tốt nhất.

Trước khi bắt đầu, đảm bảo máy tính của bạn đã cài đặt các phần mềm sau:

- **.NET 10 SDK** - [Tải tại đây](https://dotnet.microsoft.com/download/dotnet/10.0)
- **SQL Server** - [Tải tại đây](https://www.microsoft.com/sql-server/sql-server-downloads) và **SQL Server Management Studio (SSMS)** - [Tải tại đây](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms)
- **Git** - [Tải tại đây](https://git-scm.com/downloads)
- **Visual Studio 2026** - [Tải tại đây](https://visualstudio.microsoft.com/downloads/)
- **Docker** - [Tải tại đây](https://www.docker.com/products/docker-desktop)

# 2. Thiết lập dự án

## 1. Clone dự án, Restore NuGet & NodeJS Package

Clone dự án

```
git clone git@github.com:AnhEmMotor/AnhEmMotor-Backend.git
cd AnhEmMotor-Backend
```

Mở terminal tại thư mục gốc của dự án và chạy lệnh:

```powershell
dotnet restore
```

## 2. Tạo file cấu hình

Dự án sử dụng file `appsettings.json` để cấu hình. File mẫu là `appsettings.Template.json`.

1. Di chuyển vào thư mục `WebAPI`:

   ```powershell
   cd WebAPI
   ```

2. Tạo file `appsettings.json` từ file mẫu. Code bên dưới chạy trong Powershell. Bạn có thể tự copy Ctrl + C và Ctrl + V bằng tay:

   ```powershell
   Copy-Item appsettings.Template.json appsettings.json
   ```

3. Tạo file `appsettings.Development.json` (nếu cần):
   ```powershell
   Copy-Item appsettings.Template.Development.json appsettings.Development.json
   ```

## 3. Cấu hình ứng dụng

Mở file `WebAPI/appsettings.json` và điền các thông tin sau:

### 1. Chọn Database Provider (BẮT BUỘC)

**Dự án hỗ trợ cả SQL Server và MySQL.** Chọn provider phù hợp với môi trường:

- SQL Server (Dành cho Development trên Windows):

  ```json
  {
    "Provider": "SqlServer",
    "ConnectionStrings": {
      "StringConnection": "Server=localhost;Database=AnhEmMotorDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
    }
  }
  ```

- MySQL (Dành cho Production hoặc Testing):

  ```json
  {
    "Provider": "MySql",
    "ConnectionStrings": {
      "StringConnection": "Server=localhost;Database=anhemmotor;User=root;Password=your_password;"
    }
  }
  ```

> **⚠️ LƯU Ý QUAN TRỌNG:**
>
> - **Local Development:** Dùng **SQL Server**
> - **Production/VPS:** sử dụng **MySQL**
> - **Testing:** Tự động dùng MySQL qua Docker (không cần cấu hình, nhưng cần phải cài đặt Docker)


### 2. Connection String Chi Tiết

```json
"ConnectionStrings": {
  "StringConnection": "Server=localhost;Database=AnhEmMotorDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

Nếu bạn chưa có Database, vui lòng tạo 1 Database, sau đó tìm chuỗi kết nối của Database đó. Để tìm chuỗi Database, làm như sau trong Visual Studio:

- Vào Tools -> Connect Databases...
- Trong hộp thoại mới hiện ra, điền các thông tin sau:
  - Server name: Nếu máy chỉ có 1 server SQL Server, nên gõ dấu "."; nếu như có nhiều máy chủ, nên tìm tên đăng nhập của máy chủ đó.
  - Authenication: Chọn kiểu login là "Windows Authenication" (nếu login bằng user trong Windows) hoặc "SQL Server Authenication" (dùng username - password đăng nhập)
  - Nếu server cần Trust Certificate, hãy tích vào ô "Trust Server Certificate"
- Sau khi điền xong, ô "Select or enter a database name:" sáng lên, hãy chọn Database cần kết nối (Ở đây chính là Database mới tạo ở phía trên).
- Sau đó nhấn Advanced. Một chuỗi sẽ hiện ra trong cửa sổ mới, đó chính là chuỗi kết nối Database bạn cần dùng. Copy và dán vào chổ đó.

### 3. JWT Configuration (BẮT BUỘC)

Cấu hình JWT để xác thực người dùng:

```json
"Jwt": {
  "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!@#$%^&*()",
  "Issuer": "https://localhost:7001",
  "Audience": "https://localhost:3000",
  "AccessTokenExpiryInMinutes": 5,
  "RefreshTokenExpiryInDays": 7
}
```

**Lưu ý:**

- `Key`: Phải dài hơn 32 ký tự
- `Issuer`: URL của API (thường là `https://localhost:7001`)
- `Audience`: URL của client (frontend). Với dự án chạy trên máy cá nhân, dù có tận 2 dự án với 2 đầu API khác nhau, nhưng điều này cũng không quan trọng.

### 4. Allowed Hosts

Danh sách các hosts được phép truy cập:

```json
"AllowedHosts": "localhost;127.0.0.1;*.yourdomain.com"
```

Hoặc cho phép tất cả:

```json
```json
"AllowedHosts": "*"
```

### 5. CORS Configuration
Cấu hình CORS để cho phép các domain khác gọi API:

```json
"Cors": {
  "AllowedOrigins": "https://frontend.com;http://localhost:3000"
}
```
Hoặc cho phép tất cả (chỉ dùng cho dev):
```json
"Cors": {
  "AllowedOrigins": "*"
}
```

### 6. Protected Authorization Entities

Cấu hình roles và users mặc định:

```json
"ProtectedAuthorizationEntities": {
  "SuperRoles": ["Admin", "SuperAdmin"],
  "ProtectedUsers": [
    "admin@anhem.com:Admin@123456",
    "superadmin@anhem.com:SuperAdmin@123456"
  ],
  "DefaultRolesForNewUsers": ["User", "Customer"]
}
```

**Giải thích:**

- **SuperRoles**: Roles có full quyền, không thể xóa
  - Có thể có nhiều roles: `["Admin", "SuperAdmin", "Manager"]`
  - Mỗi super role sẽ tự động có tất cả permissions
- **ProtectedUsers**: Users không thể xóa, được tạo tự động khi app khởi động
  - Format: `"email:password"` hoặc chỉ `"email"` (password mặc định: `DefaultProtectedUser@123456`)
  - Ví dụ nhiều users:
    ```json
    [
    	"admin@anhem.com:Admin@123456",
    	"manager@anhem.com",
    	"support@anhem.com:Support@2024"
    ]
    ```
- **DefaultRolesForNewUsers**: Roles được gán tự động cho user mới đăng ký
  - Có thể có nhiều default roles: `["User", "Customer", "Member"]`

### 7. Seeding Options (Tùy chọn)

Cấu hình seeding dữ liệu ban đầu:

```json
"SeedingOptions": {
  "RunDataSeedingOnStartup": true,
}
````

### 8. Một số cài đặt bổ sung

- Cấu hình các danh mục sản phẩm không xoá

  ```json
  "ProtectedProductCategory": [ "Xe máy", "Phụ kiện", "Phụ tùng" ],
  ```

- Cấu hình địa chỉ OpenTelemetry URL để gửi dữ liệu giám sát dự án khi chạy

  ```json
  "OpenTelemetry": {
    "OtlpEndpoint": "http://localhost:4317"
  },
  ```

## 4. Tạo Database

1. Mở terminal tại thư mục gốc của dự án
2. Chạy lệnh tạo database:

   ```powershell
   cd WebAPI
   dotnet ef database update
   ```

3. Khi chạy dự án lần đầu tiên trong máy, các dữ liệu cần có để dự án bắt đầu chạy vẫn chưa có, cần phải tạo dữ liệu. Trong file `appsettings.json`, bật seeding:

   ```json
   "SeedingOptions": {
     "RunDataSeedingOnStartup": true,
   }
   ```

   Chạy ứng dụng lần đầu tiên, database và dữ liệu mẫu sẽ được tạo tự động

   **Quan trọng:** Sau khi chạy lần đầu, tắt `RunDataSeedingOnStartup`:

    ```json
    "RunDataSeedingOnStartup": false
    ```



# 3. Tạo và Quản Lý Database Migrations

> **🚨 QUAN TRỌNG CHO NGƯỜI MỚI:**
>
> Khi bạn thay đổi cấu trúc database (thêm bảng, sửa cột, etc.), **BẮT BUỘC phải tạo migration**!
> Nếu không, dự án sẽ bị lỗi khi deploy lên VPS!

## Tạo Migration (Recommended Way)

### Tự động tạo cả SQL Server và MySQL migrations

Sử dụng script wrapper để tự động tạo migrations cho CẢ 2 providers:

```powershell
# Từ thư mục gốc của dự án
.\add-migration.ps1 "TenMigration"
```

**Ví dụ:**

```powershell
.\add-migration.ps1 "AddProductColorColumn"
```

Script sẽ tự động:

- Tạo SQL Server migration (cho local development)
- Tạo MySQL migration (cho production deployment)
- Báo lỗi rõ ràng nếu có vấn đề

### Update Local Database

Sau khi tạo migration, update database local:

```powershell
dotnet ef database update --context ApplicationDBContext --project Infrastructure --startup-project WebAPI
```

## Tạo Migration Thủ Công (Advanced)

### Tạo SQL Server Migration

```powershell
dotnet ef migrations add TenMigration --project Infrastructure --startup-project WebAPI
```

### Tạo MySQL Migration

```powershell
dotnet ef migrations add TenMigration --context MySqlDbContext --output-dir MySqlMigrations --project Infrastructure --startup-project WebAPI
```

> **⚠️ NGUY HIỂM:**
>
> Nếu chỉ tạo SQL Server migration mà **quên tạo MySQL migration**, khi deploy lên VPS:
>
> - ✅ Code mới được deploy
> - ❌ Database KHÔNG được update
> - 💥 **Application sẽ CRASH** (mismatch giữa code và DB schema)
>
> **Khuyến nghị:** Luôn dùng `add-migration.ps1` để tránh quên!



## Các Lệnh Migration Hữu Ích

### Xem danh sách migrations

```powershell
# SQL Server migrations
dotnet ef migrations list --project Infrastructure --startup-project WebAPI

# MySQL migrations
dotnet ef migrations list --context MySqlDbContext --project Infrastructure --startup-project WebAPI
```

### Xóa migration cuối cùng (nếu chưa apply)

```powershell
# SQL Server
dotnet ef migrations remove --project Infrastructure --startup-project WebAPI

# MySQL
dotnet ef migrations remove --context MySqlDbContext --project Infrastructure --startup-project WebAPI
```

# 4. Chạy ứng dụng

## Cách 1: Sử dụng Visual Studio

1. Mở file `AnhEmMotor-Backend.sln` bằng Visual Studio 2022
2. Chọn project `WebAPI` làm Startup Project (chuột phải > Set as Startup Project).
3. Nhấn `F5` hoặc click nút **Run**. Chú ý ở chổ mũi tên xanh phải đang chọn "https"

Sau khi chạy ứng dụng, truy cập Swagger UI để xem tài liệu API:

```
https://localhost:7001/swagger
```

## Cách 2: Sử dụng Command Line

1. Mở terminal tại thư mục `WebAPI`:

   ```powershell
   cd WebAPI
   ```

2. Chạy ứng dụng:

   ```powershell
   dotnet build --project "WebAPI" --launch-profile "https"
   ```

3. Ứng dụng sẽ chạy tại:
   - **HTTPS:** `https://localhost:7001`
   - **HTTP:** `http://localhost:5000`

Sau khi chạy ứng dụng, truy cập Swagger UI để xem tài liệu API:

```
https://localhost:7001/swagger
```

# 5. Cấu hình Môi trường Test (Yêu cầu)

Dự án sử dụng **Testcontainers** để tự động tạo môi trường MySQL cô lập khi chạy Test.

**Yêu cầu duy nhất:** Máy tính phải cài đặt và đang chạy **Docker Desktop** (hoặc Docker Engine).

**Cách chạy Test:**

1.  Bật Docker.
2.  Chạy lệnh: `dotnet test`
3.  Hệ thống sẽ tự động:
    - Tải Docker Image `mysql:8.0`.
    - Khởi tạo Container.
    - Chạy Migrations.
    - Thực thi Test.
    - Tự động dọn dẹp sau khi xong.

# 6. GitHub Secrets Configuration (Cho Production Deploy)

Cần setup các secrets sau trong GitHub repository:

**Vào:** `Settings` → `Secrets and variables` → `Actions` → `New repository secret`

### Required Secrets

| Secret Name       | Mô Tả                               | Ví Dụ                                                          |
| ----------------- | ----------------------------------- | -------------------------------------------------------------- |
| `ALLOWED_HOSTS`   | Domains được phép                   | `api.yourdomain.com;yourdomain.com` hoặc `*`                   |
| `CORS_ALLOWED_ORIGINS` | CORS Allowed Origins           | `https://yourdomain.com;http://localhost:3000` hoặc `*`        |
| `DB_CONNECTION`   | MySQL connection string             | `Server=localhost;Database=anhemmotor;User=root;Password=xxx;` |
| `JWT_KEY`         | JWT secret key (>= 32 chars)        | `Your-Super-Secret-JWT-Key-32-Chars`                           |
| `JWT_ISSUER`      | API URL                             | `https://api.yourdomain.com`                                   |
| `JWT_AUDIENCE`    | Client URL                          | `https://yourdomain.com`                                       |
| `HOST`            | VPS IP hoặc domain, hoặc dấu \*     | `*`                                                            |
| `USERNAME`        | SSH username                        | `root` hoặc `youruser`                                         |
| `SSH_PRIVATE_KEY` | Private SSH key                     | Nội dung file `~/.ssh/id_rsa`                                  |
| `RUN_DATA_SEEDING` | Chạy data seeding khi deploy (true/false) | `false` (production) hoặc `true` (lần đầu setup) |

### Array Secrets (SuperRoles, ProtectedUsers, DefaultRoles)

**Quan trọng:** GitHub Secrets hỗ trợ JSON array format!

#### SUPER_ROLES

**Single value:**

```
["Admin"]
```

**Multiple values:**

```json
["Admin", "SuperAdmin", "Manager"]
```

#### PROTECTED_USERS

**Single user:**

```json
["admin@anhem.com:Admin@123456"]
```

**Multiple users:**

```json
["admin@anhem.com:Admin@123456","manager@anhem.com","support@anhem.com:Support@2024"]
```

#### DEFAULT_ROLES

**Single role:**

```json
["User"]
```

**Multiple roles:**

````json
["User","Customer"]
````

> **Lưu ý:** Không có space (khoảng cách) sau dấu phẩy trong JSON array! Ví dụ viết như 2 ví dụ sau là sai:
```json
["admin@anhem.com:Admin@123456" , "manager@anhem.com"]
["admin@anhem.com:Admin@123456", "manager@anhem.com"]
````

# 7. Troubleshooting

## Lỗi: "Docker is not running"

**Giải pháp:**
Hãy chắc chắn bạn đã bật Docker Desktop trước khi chạy Test.

## Lỗi: "Unable to connect to SQL Server"

**Giải pháp:**

1. Kiểm tra SQL Server đã được khởi động chưa. (Kiểm tra service "MSSQLSERVER" đã khởi động chưa)
2. Kiểm tra connection string trong `appsettings.json`
3. Thử connection string khác:
   - Với SQL Express: `Server=.\\SQLEXPRESS;...`
   - Với LocalDB: `Server=(localdb)\\MSSQLLocalDB;...`

## Lỗi: "The certificate chain was issued by an authority that is not trusted"

**Giải pháp:**
Thêm `TrustServerCertificate=True` vào connection string:

```json
"StringConnection": "Server=localhost;Database=AnhEmMotorDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

## Lỗi: "JWT Key must be at least 32 characters"

**Giải pháp:**
Đảm bảo `Jwt.Key` trong `appsettings.json` dài hơn 32 ký tự.

## Lỗi: "Unable to resolve service for type 'DbContext'"

**Giải pháp:**

1. Chạy `dotnet restore`
2. Rebuild solution
3. Kiểm tra connection string đúng chưa

## Port đã được sử dụng

**Giải pháp:**

Khởi động lại máy tính.

Nếu không được, thay đổi port trong file `WebAPI/Properties/launchSettings.json` (Nhưng vì file này có thể push lên github nên nhớ trở về cổng cũ):

```json
"applicationUrl": "https://localhost:7002;http://localhost:5001"
```
