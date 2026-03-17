# AnhEmMotor Backend API

[English](#english) | [Tiếng Việt](#tiếng-việt) | [Rules (Quy chuẩn)](./RULES.md)

---

<a id="english"></a>

# English

[Project Rules](./RULES.md)

**Copyright (C) 2026 Tran Thanh Binh, Nguyen Huynh Kim Ngan, Nguyen Trinh Anh Khoi, Trinh Minh Uyen.**

This project is licensed under the **Apache License 2.0**.
See the [LICENSE](LICENSE) file for details.

> **🚨 IMPORTANT WARNING FOR BEGINNERS:**
>
> **This project requires running SQL Server locally and MySQL on a VPS.**
> **When changing the database structure (adding tables, modifying columns, etc.), you MUST create a migration using the method below!**
>
> ```powershell
> # Run this command whenever you modify Entity/DbContext:
> .\add-migration.ps1 "MigrationName"
> ```
>
> **If you forget to create a MySQL migration:**
>
> - ✅ New code is deployed to VPS
> - ❌ Database is NOT updated
> - 💥 **The application will CRASH when running!**
>
> ➡️ For details, see [Section 3. Create and Manage Database Migrations](#3-create-and-manage-database-migrations)

## Table of Contents

- [1. System Requirements](#1-system-requirements)
- [2. Project Setup](#2-project-setup)
  - [1. Clone the project, Restore NuGet & NodeJS Packages](#1-clone-the-project-restore-nuget--nodejs-packages)
  - [2. Create configuration files](#2-create-configuration-files)
  - [3. Application Configuration](#3-application-configuration)
  - [4. Database Creation](#4-database-creation)
- [3. Create and Manage Database Migrations](#3-create-and-manage-database-migrations)
  - [Create Migration (Recommended Way)](#create-migration-recommended-way)
  - [Create Migration Manually (Advanced)](#create-migration-manually-advanced)
  - [Useful Migration Commands](#useful-migration-commands)
- [4. Running the application](#4-running-the-application)
- [5. Test Environment Configuration (Required)](#5-test-environment-configuration-required)
- [6. GitHub Secrets Configuration (For Production Deploy)](#6-github-secrets-configuration-for-production-deploy)
- [7. Troubleshooting](#7-troubleshooting)

# 1. System Requirements

It is highly recommended to use Windows to have the best programming experience.

Before getting started, make sure your computer has the following software installed:

- **.NET 10 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/10.0)
- **SQL Server** - [Download here](https://www.microsoft.com/sql-server/sql-server-downloads) and **SQL Server Management Studio (SSMS)** - [Download here](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms)
- **Git** - [Download here](https://git-scm.com/downloads)
- **Visual Studio 2026** - [Download here](https://visualstudio.microsoft.com/downloads/)
- **Docker** - [Download here](https://www.docker.com/products/docker-desktop)

# 2. Project Setup

## 1. Clone the project, Restore NuGet & NodeJS Packages

Clone the project

```bash
git clone git@github.com:AnhEmMotor/AnhEmMotor-Backend.git
cd AnhEmMotor-Backend
```

Open the terminal at the project's root directory and run the following command:

```powershell
dotnet restore
```

## 2. Create configuration files

The project uses the `appsettings.json` file for configuration. The template file is `appsettings.Template.json`.

1. Go to the `WebAPI` directory:

   ```powershell
   cd WebAPI
   ```

2. Create the `appsettings.json` file from the template. The code below runs in PowerShell. You can manually copy (Ctrl + C) and paste (Ctrl + V):

   ```powershell
   Copy-Item appsettings.Template.json appsettings.json
   ```

3. Create the `appsettings.Development.json` file (if necessary):
   ```powershell
   Copy-Item appsettings.Template.Development.json appsettings.Development.json
   ```

## 3. Application Configuration

Open the `WebAPI/appsettings.json` file and fill in the following information:

### 1. Choose Database Provider (MANDATORY)

**The project supports both SQL Server and MySQL.** Choose the appropriate provider for your environment:

- SQL Server (For Development on Windows):

  ```json
  {
  	"Provider": "SqlServer",
  	"ConnectionStrings": {
  		"StringConnection": "Server=localhost;Database=AnhEmMotorDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  	}
  }
  ```

- MySQL (For Production or Testing):

  ```json
  {
  	"Provider": "MySql",
  	"ConnectionStrings": {
  		"StringConnection": "Server=localhost;Database=anhemmotor;User=root;Password=your_password;"
  	}
  }
  ```

> **⚠️ IMPORTANT NOTE:**
>
> - **Local Development:** Use **SQL Server**
> - **Production/VPS:** Use **MySQL**
> - **Testing:** Automatically uses MySQL via Docker (no configuration needed, but Docker must be installed)

### 2. Detailed Connection String

```json
"ConnectionStrings": {
  "StringConnection": "Server=localhost;Database=AnhEmMotorDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

If you do not have a Database yet, please create one, then find its connection string. To find the Database connection string in Visual Studio:

- Go to Tools -> Connect to Database...
- In the new dialog box, fill in the following information:
  - Server name: If your machine has only one SQL Server instance, type "."; if you have multiple servers, find the login name of that server.
  - Authentication: Choose "Windows Authentication" (if logging in with a Windows user) or "SQL Server Authentication" (using a username and password).
  - If the server requires Trust Certificate, check the "Trust Server Certificate" box.
- After filling in the details, the "Select or enter a database name:" dropdown will be enabled, select the Database you want to connect to (This is the Database you just created above).
- Then click Advanced. A string will appear in a new window, which is the Database connection string you need. Copy and paste it.

### 3. JWT Configuration (MANDATORY)

Configure JWT for user authentication:

```json
"Jwt": {
  "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!@#$%^&*()",
  "Issuer": "https://localhost:7001",
  "Audience": "https://localhost:3000",
  "AccessTokenExpiryInMinutes": 5,
  "RefreshTokenExpiryInDays": 7
}
```

**Note:**

- `Key`: Must be at least 32 characters long
- `Issuer`: API URL (usually `https://localhost:7001`)
- `Audience`: Client (frontend) URL. For local development, even with two projects and two different API endpoints, this doesn't matter much.

### 4. Allowed Hosts

List of allowed hosts to access the API:

```json
"AllowedHosts": "localhost;127.0.0.1;*.yourdomain.com"
```

Or allow all:

```json
"AllowedHosts": "*"
```

### 5. CORS Configuration

Configure CORS to allow other domains to call the API:

```json
"Cors": {
  "AllowedOrigins": "https://frontend.com;http://localhost:3000"
}
```

Or allow all (only for development):

```json
"Cors": {
  "AllowedOrigins": "*"
}
```

### 6. Protected Authorization Entities

Configure default roles and users:

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

**Explanation:**

- **SuperRoles**: Roles with full permissions that cannot be deleted
  - Can have multiple roles: `["Admin", "SuperAdmin", "Manager"]`
  - Each super role will automatically have all permissions
- **ProtectedUsers**: Users that cannot be deleted, automatically created when the app starts
  - Format: `"email:password"` or just `"email"` (default password: `DefaultProtectedUser@123456`)
  - Example for multiple users:
    ```json
    [
    	"admin@anhem.com:Admin@123456",
    	"manager@anhem.com",
    	"support@anhem.com:Support@2024"
    ]
    ```
- **DefaultRolesForNewUsers**: Roles automatically assigned to newly registered users
  - Can have multiple default roles: `["User", "Customer", "Member"]`

### 7. Seeding Options (Optional)

Configure initial data seeding:

```json
"SeedingOptions": {
  "RunDataSeedingOnStartup": true
}
```

### 8. Some additional settings

- Configure undeletable product categories

  ```json
  "ProtectedProductCategory": [ "Xe máy", "Phụ kiện", "Phụ tùng" ],
  ```

- Configure OpenTelemetry URL address to send project monitoring data during runtime

  ```json
  "OpenTelemetry": {
    "OtlpEndpoint": "http://localhost:4317"
  },
  ```

## 4. Database Creation

1. Open the terminal at the project's root directory
2. Run the command to create the database:

   ```powershell
   cd WebAPI
   dotnet ef database update
   ```

3. When running the project locally for the first time, necessary data for the project to start is not available, so data must be created. In the `appsettings.json` file, enable seeding:

   ```json
   "SeedingOptions": {
     "RunDataSeedingOnStartup": true
   }
   ```

   Run the application for the first time, the database and sample data will be created automatically.

   **Important:** After running for the first time, disable `RunDataSeedingOnStartup`:

   ```json
   "RunDataSeedingOnStartup": false
   ```

# 3. Create and Manage Database Migrations

> **🚨 IMPORTANT FOR BEGINNERS:**
>
> When you change the database structure (add tables, edit columns, etc.), **you MUST create a migration**!
> Otherwise, the project will encounter errors when deployed to the VPS!

## Create Migration (Recommended Way)

### Automatically create both SQL Server and MySQL migrations

Use a script wrapper to automatically create migrations for BOTH providers:

```powershell
# From the project root directory
.\add-migration.ps1 "MigrationName"
```

**Example:**

```powershell
.\add-migration.ps1 "AddProductColorColumn"
```

The script will automatically:

- Create SQL Server migration (for local development)
- Create MySQL migration (for production deployment)
- Report obvious errors if there are issues

### Update Local Database

After creating the migration, update your local database:

```powershell
dotnet ef database update --context ApplicationDBContext --project Infrastructure --startup-project WebAPI
```

## Create Migration Manually (Advanced)

### Create SQL Server Migration

```powershell
dotnet ef migrations add MigrationName --project Infrastructure --startup-project WebAPI
```

### Create MySQL Migration

```powershell
dotnet ef migrations add MigrationName --context MySqlDbContext --output-dir MySqlMigrations --project Infrastructure --startup-project WebAPI
```

> **⚠️ DANGER:**
>
> If you only create the SQL Server migration and **forget to create the MySQL migration**, when deploying to VPS:
>
> - ✅ New code is deployed
> - ❌ Database is NOT updated
> - 💥 **Application will CRASH** (mismatch between code and DB schema)
>
> **Recommendation:** Always use `add-migration.ps1` to prevent forgetting!

## Useful Migration Commands

### View migration list

```powershell
# SQL Server migrations
dotnet ef migrations list --project Infrastructure --startup-project WebAPI

# MySQL migrations
dotnet ef migrations list --context MySqlDbContext --project Infrastructure --startup-project WebAPI
```

### Remove the last migration (if not yet applied)

```powershell
# SQL Server
dotnet ef migrations remove --project Infrastructure --startup-project WebAPI

# MySQL
dotnet ef migrations remove --context MySqlDbContext --project Infrastructure --startup-project WebAPI
```

# 4. Running the application

## Method 1: Using Visual Studio

1. Open the `AnhEmMotor-Backend.sln` file with Visual Studio 2022
2. Set the `WebAPI` project as the Startup Project (Right-click > Set as Startup Project).
3. Press `F5` or click the **Run** button. Note that "https" should be selected next to the green arrow.

After running the application, access the Swagger UI to view the API documentation:

```
https://localhost:7001/swagger
```

## Method 2: Using Command Line

1. Open a terminal at the `WebAPI` directory:

   ```powershell
   cd WebAPI
   ```

2. Run the application:

   ```powershell
   dotnet watch --project "WebAPI/WebAPI.csproj" --launch-profile "https"
   ```

3. The application will run at:
   - **HTTPS:** `https://localhost:7001`
   - **HTTP:** `http://localhost:5000`

After running the application, access the Swagger UI to view the API documentation:

```
https://localhost:7001/swagger
```

# 5. Test Environment Configuration (Required)

The project uses **Testcontainers** to automatically create an isolated MySQL environment when running tests.

**Only requirement:** Your computer must have **Docker Desktop** (or Docker Engine) installed and running.

**How to run Tests:**

1.  Start Docker.
2.  Run the command: `dotnet test`
3.  The system will automatically:
    - Download the `mysql:8.0` Docker Image.
    - Initialize the Container.
    - Run Migrations.
    - Execute Tests.
    - Automatically clean up afterwards.

# 6. GitHub Secrets Configuration (For Production Deploy)

The following secrets need to be set up in the GitHub repository:

**Go to:** `Settings` → `Secrets and variables` → `Actions` → `New repository secret`

### Required Secrets

| Secret Name            | Description                             | Example                                                        |
| ---------------------- | --------------------------------------- | -------------------------------------------------------------- |
| `ALLOWED_HOSTS`        | Allowed domains                         | `api.yourdomain.com;yourdomain.com` or `*`                     |
| `CORS_ALLOWED_ORIGINS` | CORS Allowed Origins                    | `https://yourdomain.com;http://localhost:3000` or `*`          |
| `DB_CONNECTION`        | MySQL connection string                 | `Server=localhost;Database=anhemmotor;User=root;Password=xxx;` |
| `JWT_KEY`              | JWT secret key (>= 32 chars)            | `Your-Super-Secret-JWT-Key-32-Chars`                           |
| `JWT_ISSUER`           | API URL                                 | `https://api.yourdomain.com`                                   |
| `JWT_AUDIENCE`         | Client URL                              | `https://yourdomain.com`                                       |
| `HOST`                 | VPS IP or domain, or \*                 | `*`                                                            |
| `USERNAME`             | SSH username                            | `root` or `youruser`                                           |
| `SSH_PRIVATE_KEY`      | Private SSH key                         | Content of `~/.ssh/id_rsa`                                     |
| `RUN_DATA_SEEDING`     | Run data seeding on deploy (true/false) | `false` (production) or `true` (first-time setup)              |
| `COOKIE_DOMAIN`        | Cookie Domain (for refresh tokens)      | `.yourdomain.com` or empty for IP address                      |

### Array Secrets (SuperRoles, ProtectedUsers, DefaultRoles)

**Important:** GitHub Secrets support JSON array format!

#### SUPER_ROLES

**Single value:**

```json
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
[
	"admin@anhem.com:Admin@123456",
	"manager@anhem.com",
	"support@anhem.com:Support@2024"
]
```

#### DEFAULT_ROLES

**Single role:**

```json
["User"]
```

**Multiple roles:**

```json
["User", "Customer"]
```

> **Note:** Do not include spaces after commas in the JSON array! For example, the following two examples are incorrect:

```json
["admin@anhem.com:Admin@123456" , "manager@anhem.com"]
["admin@anhem.com:Admin@123456", "manager@anhem.com"]
```

# 7. Troubleshooting

## Error: "Docker is not running"

**Solution:**
Make sure you have started Docker Desktop before running tests.

## Error: "Unable to connect to SQL Server"

**Solution:**

1. Check if SQL Server has been started. (Check if the "MSSQLSERVER" service has started)
2. Check the connection string in `appsettings.json`
3. Try a different connection string:
   - For SQL Express: `Server=.\\SQLEXPRESS;...`
   - For LocalDB: `Server=(localdb)\\MSSQLLocalDB;...`

## Error: "The certificate chain was issued by an authority that is not trusted"

**Solution:**
Add `TrustServerCertificate=True` to the connection string:

```json
"StringConnection": "Server=localhost;Database=AnhEmMotorDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

## Error: "JWT Key must be at least 32 characters"

**Solution:**
Ensure `Jwt.Key` in `appsettings.json` is longer than 32 characters.

## Error: "Unable to resolve service for type 'DbContext'"

**Solution:**

1. Run `dotnet restore`
2. Rebuild solution
3. Check if the connection string is correct

## Error: Port is already in use

**Solution:**

Try run 2 command in Command Prompt Administrator:

```
sc stop winnat
sc start winnat
```

If it hasn't been successful yet, Restart your computer.

If it does not work, change the port in the `WebAPI/Properties/launchSettings.json` file (But since this file might be pushed to GitHub, remember to revert it to the old port):

```json
"applicationUrl": "https://localhost:7002;http://localhost:5001"
```

---

<a id="tiếng-việt"></a>

# Tiếng Việt

[Quy chuẩn dự án](./RULES.md)

**Copyright (C) 2026 Tran Thanh Binh, Nguyen Huynh Kim Ngan, Nguyen Trinh Anh Khoi, Trinh Minh Uyen.**

Dự án này được cấp phép theo **Giấy phép Apache 2.0**.
Xem tệp [LICENSE](LICENSE) để biết chi tiết.

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
>
> **🚨 QUAN TRỌNG VỀ DEPLOY:**
> **Khi deploy lên host, hãy nhớ cấu hình `COOKIE_DOMAIN` trong GitHub Secrets. Nếu để trống khi sử dụng tên miền, session sẽ không thể duy trì khi reload trang!**

## Mục lục

- [1. Yêu cầu hệ thống](#1-yêu-cầu-hệ-thống)
- [2. Thiết lập dự án](#2-thiết-lập-dự-án)
  - [1. Clone dự án, Restore NuGet & NodeJS Package](#1-clone-dự-án-restore-nuget--nodejs-package)
  - [2. Tạo file cấu hình](#2-tạo-file-cấu-hình)
  - [3. Cấu hình ứng dụng](#3-cấu-hình-ứng-dụng)
  - [4. Tạo Database](#4-tạo-database)
- [3. Tạo và Quản Lý Database Migrations](#3-tạo-và-quản-lý-database-migrations)
  - [Tạo Migration (Recommended Way)](#tạo-migration-recommended-way)
  - [Tạo Migration Thủ Công (Advanced)](#tạo-migration-thủ-công-advanced)
  - [Các Lệnh Migration Hữu Ích](#các-lệnh-migration-hữu-ích)
- [4. Chạy ứng dụng](#4-chạy-ứng-dụng)
- [5. Cấu hình Môi trường Test (Yêu cầu)](#5-cấu-hình-môi-trường-test-yêu-cầu)
- [6. GitHub Secrets Configuration (Cho Production Deploy)](#6-github-secrets-configuration-cho-production-deploy)
- [7. Troubleshooting](#7-troubleshooting-1)

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
  "RunDataSeedingOnStartup": true
}
```

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
     "RunDataSeedingOnStartup": true
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
   dotnet watch --project "WebAPI/WebAPI.csproj" --launch-profile "https"
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

| Secret Name            | Mô Tả                                     | Ví Dụ                                                                  |
| ---------------------- | ----------------------------------------- | ---------------------------------------------------------------------- |
| `ALLOWED_HOSTS`        | Domains được phép                         | `api.yourdomain.com;yourdomain.com` hoặc `*`                           |
| `CORS_ALLOWED_ORIGINS` | CORS Allowed Origins                      | `https://yourdomain.com;http://localhost:3000` hoặc `*`                |
| `DB_CONNECTION`        | MySQL connection string                   | `Server=localhost;Database=anhemmotor;User=root;Password=xxx;`         |
| `JWT_KEY`              | JWT secret key (>= 32 chars)              | `Your-Super-Secret-JWT-Key-32-Chars`                                   |
| `JWT_ISSUER`           | API URL                                   | `https://api.yourdomain.com`                                           |
| `JWT_AUDIENCE`         | Client URL                                | `https://yourdomain.com`                                               |
| `HOST`                 | VPS IP hoặc domain, hoặc dấu \*           | `*`                                                                    |
| `USERNAME`             | SSH username                              | `root` hoặc `youruser`                                                 |
| `SSH_PRIVATE_KEY`      | Private SSH key                           | Nội dung file `~/.ssh/id_rsa`                                          |
| `RUN_DATA_SEEDING`     | Chạy data seeding khi deploy (true/false) | `false` (production) hoặc `true` (lần đầu setup)                       |
| `COOKIE_DOMAIN`        | Cookie Domain (for refresh tokens)        | `.yourdomain.com` hoặc để trống nếu đang chạy trên Localhost (Your IP) |

### Array Secrets (SuperRoles, ProtectedUsers, DefaultRoles)

**Quan trọng:** GitHub Secrets hỗ trợ JSON array format!

#### SUPER_ROLES

**Single value:**

```json
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
[
	"admin@anhem.com:Admin@123456",
	"manager@anhem.com",
	"support@anhem.com:Support@2024"
]
```

#### DEFAULT_ROLES

**Single role:**

```json
["User"]
```

**Multiple roles:**

```json
["User", "Customer"]
```

> **Lưu ý:** Không có space (khoảng cách) sau dấu phẩy trong JSON array! Ví dụ viết như 2 ví dụ sau là sai:

```json
["admin@anhem.com:Admin@123456" , "manager@anhem.com"]
["admin@anhem.com:Admin@123456", "manager@anhem.com"]
```

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

## Lỗi: Port đã được sử dụng

**Giải pháp:**

Chạy 2 lệnh sau trong Command Prompt với quyền quản trị viên:

```
sc stop winnat
sc start winnat
```

Nếu vẫn không thành công, hãy khởi động lại máy tính.

Nếu không được, thay đổi port trong file `WebAPI/Properties/launchSettings.json` (Nhưng vì file này có thể push lên github nên nhớ trở về cổng cũ):

```json
"applicationUrl": "https://localhost:7002;http://localhost:5001"
```
