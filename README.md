# AnhEmMotor Backend API

[English](#english) | [Tiếng Việt](#tiếng-việt) | [Rules (Quy chuẩn)](./RULES.md) | [Setup VPS](./SETUP_VPS.md)

---

<a id="english"></a>

# English

[Project Rules](./RULES.md)

**Copyright (C) 2026 Tran Thanh Binh, Nguyen Huynh Kim Ngan, Nguyen Trinh Anh Khoi, Trinh Minh Uyen.**

This project is licensed under the **Apache License 2.0**.
See the [LICENSE](LICENSE) file for details.

> **🚨 IMPORTANT WARNING FOR BEGINNERS:**
>
> **This project requires running SQL Server locally and PostgreSQL on a VPS.**
> **When changing the database structure (adding tables, modifying columns, etc.), you MUST create a migration using the method below!**
>
> ```powershell
> # Run this command whenever you modify Entity/DbContext:
> .\add-migration.ps1 "MigrationName"
> ```
>
> **If you forget to create a PostgreSQL migration:**
>
> - ✅ New code is deployed to VPS
> - ❌ Database is NOT updated
> - 💥 **The application will CRASH when running!**
>
> ➡️ For details, see [Section 3. Create and Manage Database Migrations](#3-create-and-manage-database-migrations)
>
> **🚨 IMPORTANT REGARDING DEPLOYMENT:**
>
> **When deploying to a host, remember to configure `COOKIE_DOMAIN` in GitHub Secrets. If left blank when using a domain name, the session will not persist when the page reloads!**
>
> Additionally, to run project tests quickly in Windows operation, you should also add this project folder to the exclusion list in Windows Defender (If you feel it's dangerous, you can omit this.). Follow these steps to exclude a specific folder from Windows Defender scans:
>
> 1.  **Open Windows Security**: Press the `Windows` key, type **Windows Security**, and press **Enter**.
> 2.  **Navigate to Protection**: Click on **Virus & threat protection** in the left menu.
> 3.  **Manage Settings**: Under the **Virus & threat protection settings** section, click **Manage settings**.
> 4.  **Exclusions**: Scroll down to the bottom and click **Add or remove exclusions**.
> 5.  **Add Folder**: Click the **+ Add an exclusion** button and select **Folder** from the dropdown menu.
> 6.  **Select Path**: Browse to the folder you want to exclude (folder contain project), select it, and click **Select Folder**.
> 7.  **Confirm**: If a User Account Control (UAC) prompt appears, click **Yes**.

## Table of Contents

- [1. System Requirements](#1-system-requirements)
- [2. Project Setup](#2-project-setup)
  - [1. Clone the project, Restore NuGet & NodeJS Packages](#1-clone-the-project-restore-nuget--nodejs-packages)
  - [2. Create configuration files](#2-create-configuration-files)
  - [3. Application Configuration](#3-application-configuration)
  - [4. Database Creation](#4-database-creation)
- [3. Create and Manage Database Migrations](#3-create-and-manage-database-migrations)
- [4. Running the application](#4-running-the-application)
- [5. Social Login Configuration Guide](#5-social-login-configuration-guide)
- [6. Test Environment Configuration (Required)](#6-test-environment-configuration-required)
- [7. GitHub Secrets Configuration (For Production Deploy)](#7-github-secrets-configuration-for-production-deploy)
- [8. Online Payment Configuration (English)](#8-online-payment-configuration-english)
- [9. GHN (Giao Hàng Nhanh) Configuration (English)](#9-GHN-giao-hàng-tiết-kiệm-configuration-english)
- [10. AI Sidecar Configuration (Gemini & LangSmith)](#10-ai-sidecar-configuration-gemini--langsmith)
- [11. Troubleshooting](#11-troubleshooting)

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

**The project supports both SQL Server, MySQL and PostgreSQL.** Choose the appropriate provider for your environment:

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

- PostgreSQL (For Production or Testing):

  ```json
  {
  	"Provider": "PostgreSQL",
  	"ConnectionStrings": {
  		"StringConnection": "Server=localhost;Database=anhemmotor;User=root;Password=your_password;"
  	}
  }
  ```

> **⚠️ IMPORTANT NOTE:**
>
> - **Local Development:** Use **SQL Server**
> - **Production/VPS:** Use **PostgreSQL**
> - **Testing:** Automatically uses PostgreSQL via Docker (no configuration needed, but Docker must be installed)

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

### 7. External Authentication (Google & Facebook)

Configure Google and Facebook Client IDs and Secrets for Social Login:

```json
"Authentication": {
  "Google": {
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret"
  },
  "Facebook": {
    "AppId": "your-facebook-app-id",
    "AppSecret": "your-facebook-app-secret"
  }
}
```

### 8. Seeding Options (Optional)

Configure initial data seeding:

```json
"SeedingOptions": {
  "RunDataSeedingOnStartup": true
}
```

### 9. Some additional settings

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

### Automatically create both SQL Server and PostgreSQL migrations

Use a script wrapper to automatically create migrations for BOTH providers:

```powershell
# From the project root directory
.\add-migration.ps1 "MigrationName"
```

> **⚠️ RULE: ONE BRANCH = ONE MIGRATION**
>
> To keep the migration history clean and avoid deployment conflicts, **each branch/Pull Request is only allowed to have EXACTLY ONE migration**.
>
> - If you need to make more changes to the database, **remove the previous migration** and create a new one, or combine your changes.
> - The `add-migration.ps1` script and GitHub Actions will block you if you try to include more than one migration.

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

### Create PostgreSQL Migration

```powershell
dotnet ef migrations add MigrationName --context PostgreSqlDbContext --output-dir PostgreSqlMigrations --project Infrastructure --startup-project WebAPI
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
dotnet ef migrations list --context ApplicationDBContext --project Infrastructure --startup-project WebAPI

# MySQL migrations
dotnet ef migrations list --context MySqlDbContext --project Infrastructure --startup-project WebAPI

# PostgreSQL migrations
dotnet ef migrations list --context PostgreSqlDbContext --project Infrastructure --startup-project WebAPI
```

### Remove the last migration (if not yet applied)

```powershell
# SQL Server
dotnet ef migrations remove --context ApplicationDBContext--project Infrastructure --startup-project WebAPI

# MySQL
dotnet ef migrations remove --context MySqlDbContext --project Infrastructure --startup-project WebAPI

# PostgreSQL
dotnet ef migrations remove --context PostgreSqlDbContext --project Infrastructure --startup-project WebAPI
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

# 5. Social Login Configuration Guide

If you don't have Social Login credentials yet, follow these steps to create your own Client ID and Client Secret:

### 1. Google Social Login (Google Cloud Console)

1.  Go to [Google Cloud Console](https://console.cloud.google.com/).
2.  **Create a Project**: Click the project dropdown top-left -> **"New Project"** -> Enter name -> **"Create"**.
3.  **Configure OAuth Consent Screen**:
    - Search for **"APIs & Services"** > **"OAuth consent screen"**.
    - User Type: Select **External** -> **"Create"**.
    - **App information**: Fill in App name, User support email, and Developer contact information. Click **"Save and Continue"**.
    - **Scopes**: Click **"Add or Remove Scopes"** -> Search and select `.../auth/userinfo.email` and `.../auth/userinfo.profile` -> **"Update"** -> **"Save and Continue"**.
    - **Test users**: Add your email to test before publishing.
4.  **Create Credentials**:
    - Go to **"Credentials"** > **"Create Credentials"** > **"OAuth client ID"**.
    - Application type: Select **Web application**.
    - Name: Enter your app name (e.g., "AnhEmMotor Web").
    - **Authorized JavaScript origins**:
      - `https://localhost:5173` (Management UI)
      - `http://localhost:3000` (Store UI)
    - **Authorized redirect URIs**:
      - `https://localhost:7001/signin-google` (Backend API)
5.  **Get Credentials**: A dialog will appear with your **Client ID** and **Client Secret**. Copy them to your `appsettings.json`.

### 2. Facebook Social Login (Meta for Developers)

1.  Go to [Meta for Developers](https://developers.facebook.com/).
2.  **Create an App**:
    - Click **"My Apps"** -> **"Create App"**.
    - Select **"Authenticate and request data from users with Facebook Login"** -> **"Next"**.
    - Fill in App Name and Contact Email -> **"Create app"**.
3.  **Configure Facebook Login**:
    - In the App Dashboard, find **"Facebook Login"** product -> click **"Set up"**.
    - Select **"Web"**. You can skip the Quickstart steps.
4.  **Get Credentials**:
    - Go to **"App settings"** > **"Basic"**.
    - Copy your **App ID** and **App Secret**.
    - Fill in **Privacy Policy URL** (e.g., `https://yourdomain.com/privacy`).
5.  **Configure Redirect URIs**:
    - Go to **"Facebook Login"** > **"Settings"**.
    - Under **"Client OAuth settings"**, add your redirect URIs to **"Valid OAuth Redirect URIs"**:
      - `https://localhost:7001/signin-facebook`
    - Ensure **"Embedded Browser OAuth Login"** is turned on.

# 6. Test Environment Configuration (Required)

The project uses **Testcontainers** to automatically create an isolated MySQL environment when running tests.

**Only requirement:** Your computer must have **Docker Desktop** (or Docker Engine) installed and running.

**How to run Tests:**

1.  Start Docker.
2.  Run the command: `dotnet test`
3.  The system will automatically:
    - Download the `postgres:17` Docker Image.
    - Initialize the Container.
    - Run Migrations.
    - Execute Tests.
    - Automatically clean up afterwards.

# 7. GitHub Secrets Configuration (For Production Deploy)

The following secrets need to be set up in the GitHub repository:

**Go to:** `Settings` → `Secrets and variables` → `Actions` → `New repository secret`

### Required Secrets

| Secret Name                        | Description                              | Example                                                                                                  |
| ---------------------------------- | ---------------------------------------- | -------------------------------------------------------------------------------------------------------- |
| `ALLOWED_HOSTS`                    | Allowed domains                          | `api.yourdomain.com;yourdomain.com` or `*`                                                               |
| `CORS_ALLOWED_ORIGINS`             | CORS Allowed Origins                     | `https://anhemmotor.online;https://admin.anhemmotor.online;http://localhost:5002`                        |
| `DB_CONNECTION_STRING`             | PostgreSQL connection string             | `Host=XXXXX;Port=5432;Database=AnhEmMotorDB;Username=postgres;Password=XXXXX;Include Error Detail=true;` |
| `JWT_SECRET_KEY`                   | JWT secret key (>= 32 chars)             | `Your-Super-Secret-JWT-Key-32-Chars`                                                                     |
| `JWT_ISSUER`                       | API URL                                  | `https://api.yourdomain.com`                                                                             |
| `JWT_AUDIENCE`                     | Client URL                               | `https://yourdomain.com`                                                                                 |
| `PRODUCTION_SERVER_IP`             | VPS IP or domain                         | `*`                                                                                                      |
| `PRODUCTION_SERVER_USERNAME`       | SSH username                             | `root` or `youruser`                                                                                     |
| `SERVER_REMOTE_ACCESS_PRIVATE_KEY` | Private SSH key                          | Content of `~/.ssh/id_rsa`                                                                               |
| `ENABLE_DATABASE_SEEDING`          | Run data seeding on deploy (true/false)  | `false` (production) or `true` (first-time setup)                                                        |
| `COOKIE_DOMAIN`                    | Cookie Domain (for refresh tokens)       | `.yourdomain.com` or empty for IP address                                                                |
| `SUPER_ROLES_LIST`                 | Admin/SuperAdmin roles (JSON Array)      | `["Admin", "SuperAdmin"]`                                                                                |
| `PROTECTED_USERS_LIST`             | Un-deletable users (JSON Array)          | `["admin@anhem.com:Admin@123456"]`                                                                       |
| `DEFAULT_ROLES_FOR_NEW_USER_LIST`  | Default roles for new users (JSON Array) | `["User"]`                                                                                               |
| `OTLP_ENDPOINT`                    | OpenTelemetry OTLP Endpoint              | `http://your-otel-collector:4317`                                                                        |
| `GOOGLE_CLIENT_ID`                 | Google OAuth Client ID                   | `your-google-client-id.apps.googleusercontent.com`                                                       |
| `GOOGLE_CLIENT_SECRET`             | Google OAuth Client Secret               | `GOCSPX-your-google-secret`                                                                              |
| `FACEBOOK_APP_ID`                  | Facebook App ID                          | `your-facebook-app-id`                                                                                   |
| `FACEBOOK_APP_SECRET`              | Facebook App Secret                      | `your-facebook-app-secret`                                                                               |
| `VNPAY__TMN_CODE`                  | VNPay TmnCode                            | `your-vnpay-tmn-code`                                                                                    |
| `VNPAY__HASH_SECRET`               | VNPay HashSecret                         | `your-vnpay-hash-secret`                                                                                 |
| `VNPAY__BASE_URL`                  | VNPay BaseUrl                            | `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html`                                                     |
| `VNPAY__CALLBACK_URL`              | VNPay CallbackUrl                        | `https://api.yourdomain.com/api/payment/vnpay-callback`                                                  |
| `PAYOS__CLIENT_ID`                 | PayOS Client ID                          | `your-payos-client-id`                                                                                   |
| `PAYOS__API_KEY`                   | PayOS API Key                            | `your-payos-api-key`                                                                                     |
| `PAYOS__CHECKSUM_KEY`              | PayOS Checksum Key                       | `your-payos-checksum-key`                                                                                |
| `PAYOS__BASE_URL`                  | PayOS Base URL                           | `https://api-merchant.payos.vn`                                                                          |
| `PAYOS__RETURN_URL`                | PayOS Return URL                         | `https://anhemmotor.online/payment-processing`                                                           |
| `PAYOS__CANCEL_URL`                | PayOS Cancel URL                         | `https://anhemmotor.online/payment-processing`                                                           |
| `UPLOAD_PATH`                      | Persistent image storage path            | `/var/www/anhemmotor/uploads`                                                                            |
| `GHN_TOKEN`                        | GHN API Token                            | `a1b2c3d4e5f6g7h8...`                                                                                    |
| `GHN_SHOP_ID`                      | GHN Shop ID                              | `012345`                                                                                                 |
| `GHN_BASE_URL`                     | GHN API Base URL                         | `https://dev-online-gateway.ghn.vn`                                                                      |
| `GEMINI_API_KEY`                   | Google Gemini API Key                    | `AIzaSyB...`                                                                                             |
| `GEMINI_MODEL`                     | Google Gemini Model Name                 | `gemini-3.5-flash`                                                                                       |
| `LANGSMITH_TRACING`                | Enable LangSmith tracing (true/false)    | `true`                                                                                                   |
| `LANGSMITH_API_KEY`                | LangSmith API Key                        | `lsv2_pt_...`                                                                                            |

### Array Secrets (SuperRoles, ProtectedUsers, DefaultRoles)

**Important:** GitHub Secrets support JSON array format!

#### SUPER_ROLES_LIST

**Single value:**

```json
["Admin"]
```

**Multiple values:**

```json
["Admin", "SuperAdmin", "Manager"]
```

#### PROTECTED_USERS_LIST

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

#### DEFAULT_ROLES_FOR_NEW_USER_LIST

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

# 8. Online Payment Configuration (English)

To enable payment features, you need to obtain credentials from VNPay and PayOS.

### 1. VNPay Configuration (Sandbox Environment)

1.  **Register Test Account**: Visit [VNPay Sandbox](https://sandbox.vnpayment.vn/devreg/) or contact VNPay to get a test account.
2.  **Obtain Credentials**: After registration, you will receive an email containing:
    - **vnp_TmnCode**: Terminal ID.
    - **vnp_HashSecret**: Secret key used for signing hashes.
3.  **Callback Configuration**: Ensure the callback URL matches your environment:
    - `https://api.yourdomain.com/api/payment/vnpay-callback` (Production)
    - `https://localhost:7001/api/payment/vnpay-callback` (Local)

### 2. PayOS Configuration (Production or Test)

1.  **Create Account**: Sign up at [PayOS.vn](https://payos.vn/).
2.  **Create Payment Channel**: Go to **"Payment Channels"** -> **"Create Payment Channel"** -> Choose the appropriate type.
3.  **Get API Key**: After creating a channel or by clicking on an existing one, you will find:
    - **Client ID**
    - **API Key**
    - **Checksum Key**

# 9. GHN (Giao Hàng Nhanh) Configuration (English)

To enable the automatic feature of pushing orders to GHN, you need to configure the settings in GitHub Secrets and in `appsettings.*.json`. The following guide will help you get these 3 pieces of information in the Staging/Development environment (Not production, to avoid losing money):

1. **Log in**: Access the [GHN Customer Website](https://5sao.ghn.dev/).
2. **Get Shop ID**: Click on "Chủ cửa hàng" (Shop owner) in the sidebar. In the "Thông tin cá nhân" (Personal information) tab, there is an ID field. This ID is the value for the `GHN_SHOP_ID` secret (and in the `appsettings.*.json` file, it is GHNSettings -> ShopId).
3. **Get API Token**: Also in the "Chủ cửa hàng" section, select the "Bảo mật" (Security) tab. On the "Token API & IP Tin cậy" line, click "Quản lý" (Manage). Another tab will open. In the "Token API" section, click the eye icon, verify your phone number, and a string will appear. This is the value for the `GHN_TOKEN` secret (and in the `appsettings.*.json` file, it is GHNSettings -> Token).
4. **GHN URL**: Use `https://dev-online-gateway.ghn.vn` if you are in the Development environment. This is your `GHN_BASE_URL` (and in the `appsettings.*.json` file, it is GHNSettings -> BaseUrl).
5. **Change shop location**: Go to "Quản lý cửa hàng" (Store management) in the Sidebar, you need to change/add the default store location right there.
6. **Register Webhook**: If you want to use Webhooks to receive notifications when an order is completed, you need to send an email to api@ghn.vn with the following information: Company Name, Client ID, Requested Environment, Webhook URL (which is the Endpoint URL running this project). Then wait for their response.

# 10. AI Sidecar Configuration (Gemini & LangSmith)

To enable the AI capabilities (e.g. smart search), you need a Gemini API Key and optionally LangSmith for tracing.

### 1. Get Gemini API Key

1. Go to [Google AI Studio](https://aistudio.google.com/app/api-keys).
2. Sign in with your Google account.
3. Click "Create API key" in a new or existing project.
4. Copy the generated API key and put it into `AISetup -> GeminiApiKey` in your `appsettings.json`.

### 2. Get LangSmith API Key (Optional for Tracing)

1. Go to [LangSmith](https://smith.langchain.com/).
2. Sign in or sign up.
3. In the sidebar, go to Settings (gear icon) -> API Keys.
4. Click "+ API Key", give it a name, and copy the key.
5. In your `appsettings.json`, set `AISetup -> LangSmithTracing` to `true` and paste the key into `LangSmithApiKey`.

# 11. Troubleshooting

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
> **Dự án này sẽ cần chạy SQL Server trên máy local và PostgreSQL trên VPS.**
> **Khi thay đổi cấu trúc database (thêm bảng, sửa cột, etc.), BẮT BUỘC phải tạo migration theo cách thức dưới đây!**
>
> ```powershell
> # Chạy lệnh này mỗi khi thay đổi Entity/DbContext:
> .\add-migration.ps1 "TenMigration"
> ```
>
> **Nếu quên tạo PostgreSQL migration:**
>
> - ✅ Code mới được deploy lên VPS
> - ❌ Database KHÔNG được update
> - 💥 **Application sẽ CRASH khi chạy!**
>
> ➡️ Chi tiết xem [Section 3. Tạo và Quản Lý Database Migrations](#3-tạo-và-quản-lý-database-migrations)
>
> **🚨 QUAN TRỌNG VỀ DEPLOY:**
> **Khi deploy lên host, hãy nhớ cấu hình `COOKIE_DOMAIN` trong GitHub Secrets. Nếu để trống khi sử dụng tên miền, session sẽ không thể duy trì khi reload trang!**
>
> Ngoài ra, để dự án Test chạy nhanh hơn trên máy Windows, hãy làm theo các bước sau để thêm folder dự án này vào danh sách không quét của Windows Defender (nếu bạn lo ngại về mã độc, bạn có thể không làm):
>
> 1.  **Mở Windows Security**: Nhấn phím `Windows`, gõ **Windows Security** và nhấn **Enter**.
> 2.  **Đi đến mục Bảo vệ**: Nhấn vào **Virus & threat protection** ở danh mục bên trái.
> 3.  **Quản lý cài đặt**: Tại phần **Virus & threat protection settings**, nhấn vào dòng **Manage settings**.
> 4.  **Loại trừ (Exclusions)**: Cuộn xuống dưới cùng và nhấn vào **Add or remove exclusions**.
> 5.  **Thêm thư mục**: Nhấn nút **+ Add an exclusion** và chọn **Folder** từ trình đơn thả xuống.
> 6.  **Chọn đường dẫn**: Tìm đến thư mục chứa dự án này, chọn nó và nhấn **Select Folder**.
> 7.  **Xác nhận**: Nếu có bảng thông báo User Account Control (UAC) hiện lên, hãy chọn **Yes**.

## Mục lục

- [1. Yêu cầu hệ thống](#1-yêu-cầu-hệ-thống)
- [2. Thiết lập dự án](#2-thiết-lập-dự-án)
  - [1. Clone dự án, Restore NuGet & NodeJS Package](#1-clone-dự-án-restore-nuget--nodejs-package)
  - [2. Tạo file cấu hình](#2-tạo-file-cấu-hình)
  - [3. Cấu hình ứng dụng](#3-cấu-hình-ứng-dụng)
  - [4. Tạo Database](#4-tạo-database)
- [3. Tạo và Quản Lý Database Migrations](#3-tạo-và-quản-lý-database-migrations)
- [4. Chạy ứng dụng](#4-chạy-ứng-dụng)
- [5. Cấu hình Môi trường Test (Yêu cầu)](#5-cấu-hình-môi-trường-test-yêu-cầu)
- [6. Hướng dẫn Cấu hình Đăng nhập Mạng xã hội](#6-hướng-dẫn-cấu-hình-đăng-nhập-mạng-xã-hội)
- [7. GitHub Secrets Configuration (Cho Production Deploy)](#7-github-secrets-configuration-cho-production-deploy)
- [8. Hướng dẫn Cấu hình Thanh toán Online](#8-hướng-dẫn-cấu-hình-thanh-toán-online)
- [9. Hướng dẫn Cấu hình Giao Hàng Nhanh (GHN)](#9-hướng-dẫn-cấu-hình-giao-hàng-tiết-kiệm-GHN)
- [10. Hướng dẫn Cấu hình AI Sidecar (Gemini & LangSmith)](#10-hướng-dẫn-cấu-hình-ai-sidecar-gemini--langsmith)
- [11. Troubleshooting](#11-troubleshooting-1)

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

**Dự án hỗ trợ cả SQL Server, MySQL và PostgreSQL.** Chọn provider phù hợp với môi trường:

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

- PostgreSQL (Dành cho Production hoặc Testing):

  ```json
  {
  	"Provider": "PostgreSql",
  	"ConnectionStrings": {
  		"StringConnection": "Server=localhost;Database=anhemmotor;User=root;Password=your_password;"
  	}
  }
  ```

> **⚠️ LƯU Ý QUAN TRỌNG:**
>
> - **Local Development:** Dùng **SQL Server**
> - **Production/VPS:** sử dụng **PostgreSql**
> - **Testing:** Tự động dùng PostgreSql qua Docker (không cần cấu hình, nhưng cần phải cài đặt Docker)

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
- **DefaultRolesForNewUsers**: Các Role mặc định được gán cho người dùng mới đăng ký
  - Có thể gán nhiều role: `["User", "Customer", "Member"]`

### 7. Cấu hình Đăng nhập mạng xã hội (Google & Facebook)

Cấu hình Client ID và Secret cho Google và Facebook:

```json
"Authentication": {
  "Google": {
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret"
  },
  "Facebook": {
    "AppId": "your-facebook-app-id",
    "AppSecret": "your-facebook-app-secret"
  }
}
```

### 8. Seeding Options (Tùy chọn)

Cấu hình seeding dữ liệu ban đầu:

```json
"SeedingOptions": {
  "RunDataSeedingOnStartup": true
}
```

### 9. Một số cài đặt bổ sung

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

### Tự động tạo cả SQL Server và PostgreSql migrations

Sử dụng script wrapper để tự động tạo migrations cho CẢ 2 providers:

```powershell
# Từ thư mục gốc của dự án
.\add-migration.ps1 "TenMigration"
```

> **⚠️ QUY ĐỊNH: 1 NHÁNH = 1 MIGRATION**
>
> Để giữ lịch sử migration sạch sẽ và tránh xung đột khi deploy, **mỗi nhánh (branch) hoặc Pull Request chỉ được phép có DUY NHẤT 1 migration**.
>
> - Nếu bạn cần thay đổi thêm database, hãy **xóa migration cũ** và tạo lại cái mới, hoặc gộp các thay đổi lại.
> - Script `add-migration.ps1` và GitHub Actions sẽ chặn nếu bạn cố tình tạo nhiều hơn 1 migration trong cùng 1 PR.

**Ví dụ:**

```powershell
.\add-migration.ps1 "AddProductColorColumn"
```

Script sẽ tự động:

- Tạo SQL Server migration (cho local development)
- Tạo PostgreSql migration (cho production deployment)
- Báo lỗi rõ ràng nếu có vấn đề

### Update Local Database

Sau khi tạo migration, update database local:

```powershell
dotnet ef database update --context ApplicationDBContext --project Infrastructure --startup-project WebAPI
```

## Tạo Migration Thủ Công (Advanced)

### Tạo SQL Server Migration

```powershell
dotnet ef migrations add TenMigration --context ApplicationDBContext --project Infrastructure --startup-project WebAPI
```

### Tạo MySQL Migration

```powershell
dotnet ef migrations add TenMigration --context MySqlDbContext --output-dir MySqlMigrations --project Infrastructure --startup-project WebAPI
```

### Tạo PostgreSql Migration

```powershell
dotnet ef migrations add TenMigration --context PostgreSqlDbContext --output-dir PostgreSqlMigrations --project Infrastructure --startup-project WebAPI
```

> **⚠️ NGUY HIỂM:**
>
> Nếu chỉ tạo SQL Server migration mà **quên tạo PostgreSql migration**, khi deploy lên VPS:
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
dotnet ef migrations list --context ApplicationDBContext --project Infrastructure --startup-project WebAPI

# MySQL migrations
dotnet ef migrations list --context MySqlDbContext --project Infrastructure --startup-project WebAPI

# PostgreSql migrations
dotnet ef migrations list --context PostgreSqlDbContext --project Infrastructure --startup-project WebAPI
```

### Xóa migration cuối cùng (nếu chưa apply)

```powershell
# SQL Server
dotnet ef migrations remove --context ApplicationDBContext --project Infrastructure --startup-project WebAPI

# MySQL
dotnet ef migrations remove --context MySqlDbContext --project Infrastructure --startup-project WebAPI

# PostgreSql
dotnet ef migrations remove --context PostgreSqlDbContext --project Infrastructure --startup-project WebAPI
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

Dự án sử dụng **Testcontainers** để tự động tạo môi trường PostgreSQL cô lập khi chạy Test.

**Yêu cầu duy nhất:** Máy tính phải cài đặt và đang chạy **Docker Desktop** (hoặc Docker Engine).

**Cách chạy Test:**

1.  Bật Docker.
2.  Chạy lệnh: `dotnet test`
3.  Hệ thống sẽ tự động:
    - Tải Docker Image `postgres:17.9`.
    - Khởi tạo Container.
    - Chạy Migrations.
    - Thực thi Test.
    - Tự động dọn dẹp sau khi xong.

# 6. Hướng dẫn Cấu hình Đăng nhập Mạng xã hội

Nếu bạn chưa có thông tin xác thực Social Login, hãy làm theo các bước sau để tự tạo Client ID và Client Secret:

### 1. Đăng nhập Google (Google Cloud Console)

1.  Truy cập [Google Cloud Console](https://console.cloud.google.com/).
2.  **Tạo Project**: Click vào Project dropdown ở góc trên bên trái -> **"New Project"** -> Nhập tên dự án -> **"Create"**.
3.  **Cấu hình Màn hình ID OAuth**:
    - Tìm kiếm **"APIs & Services"** > **"OAuth consent screen"**.
    - User Type: Chọn **External** -> **"Create"**.
    - **Thông tin ứng dụng**: Điền tên App, User support email, và Developer contact information. Nhấn **"Save and Continue"**.
    - **Phạm vi (Scopes)**: Nhấn **"Add or Remove Scopes"** -> Tìm và chọn `.../auth/userinfo.email` và `.../auth/userinfo.profile` -> **"Update"** -> **"Save and Continue"**.
    - **Test users**: Thêm email của bạn để test trước khi publish.
4.  **Tạo Credentials**:
    - Vào mục **"Credentials"** > **"Create Credentials"** > **"OAuth client ID"**.
    - Application type: Chọn **Web application**.
    - Name: Nhập tên ứng dụng của bạn (ví dụ: "AnhEmMotor Web").
    - **Authorized JavaScript origins**:
      - `https://localhost:5173` (Giao diện Quản lý)
      - `http://localhost:3000` (Giao diện Cửa hàng)
    - **Authorized redirect URIs**:
      - `https://localhost:7001/signin-google` (Đầu API Backend)
5.  **Lấy thông tin**: Một hộp thoại sẽ hiện ra chứa **Client ID** và **Client Secret**. Hãy copy chúng vào file `appsettings.json` của bạn.

### 2. Đăng nhập Facebook (Meta for Developers)

1.  Truy cập [Meta for Developers](https://developers.facebook.com/).
2.  **Tạo Ứng dụng**:
    - Click **"My Apps"** -> **"Create App"**.
    - Chọn **"Authenticate and request data from users with Facebook Login"** -> **"Next"**.
    - Điền App Name và Contact Email -> **"Create app"**.
3.  **Cấu hình Facebook Login**:
    - Trong App Dashboard, tìm sản phẩm **"Facebook Login"** -> nhấn **"Set up"**.
    - Chọn **"Web"**. Bạn có thể bỏ qua các bước Quickstart.
4.  **Lấy thông tin**:
    - Vào mục **"App settings"** > **"Basic"**.
    - Sao chép **App ID** và **App Secret**.
    - Điền **Privacy Policy URL** (ví dụ: `https://yourdomain.com/privacy`).
5.  **Cấu hình Redirect URIs**:
    - Vào mục **"Facebook Login"** > **"Settings"**.
    - Tại phần **"Client OAuth settings"**, thêm redirect URIs vào mục **"Valid OAuth Redirect URIs"**:
      - `https://localhost:7001/signin-facebook`
    - Đảm bảo **"Embedded Browser OAuth Login"** đang được bật (ON).

# 7. GitHub Secrets Configuration (Cho Production Deploy)

Cần setup các secrets sau trong GitHub repository:

**Vào:** `Settings` → `Secrets and variables` → `Actions` → `New repository secret`

### Required Secrets

| Secret Name                        | Mô Tả                                     | Ví Dụ                                                                                                    |
| ---------------------------------- | ----------------------------------------- | -------------------------------------------------------------------------------------------------------- |
| `ALLOWED_HOSTS`                    | Domains được phép                         | `api.yourdomain.com;yourdomain.com` hoặc `*`                                                             |
| `CORS_ALLOWED_ORIGINS`             | CORS Allowed Origins                      | `https://anhemmotor.online;https://admin.anhemmotor.online;http://localhost:5002`                        |
| `DB_CONNECTION_STRING`             | PostgreSQL connection string              | `Host=XXXXX;Port=5432;Database=AnhEmMotorDB;Username=postgres;Password=XXXXX;Include Error Detail=true;` |
| `JWT_SECRET_KEY`                   | JWT secret key (>= 32 chars)              | `Your-Super-Secret-JWT-Key-32-Chars`                                                                     |
| `JWT_ISSUER`                       | API URL                                   | `https://api.yourdomain.com`                                                                             |
| `JWT_AUDIENCE`                     | Client URL                                | `https://yourdomain.com`                                                                                 |
| `PRODUCTION_SERVER_IP`             | VPS IP hoặc domain                        | `*`                                                                                                      |
| `PRODUCTION_SERVER_USERNAME`       | SSH username                              | `root` hoặc `youruser`                                                                                   |
| `SERVER_REMOTE_ACCESS_PRIVATE_KEY` | Private SSH key                           | Nội dung file `~/.ssh/id_rsa`                                                                            |
| `ENABLE_DATABASE_SEEDING`          | Chạy data seeding khi deploy (true/false) | `false` (production) hoặc `true` (lần đầu setup)                                                         |
| `COOKIE_DOMAIN`                    | Cookie Domain (for refresh tokens)        | `.yourdomain.com` hoặc để trống nếu đang chạy trên Localhost (Your IP)                                   |
| `SUPER_ROLES_LIST`                 | Danh sách Roles Admin (JSON Array)        | `["Admin", "SuperAdmin"]`                                                                                |
| `PROTECTED_USERS_LIST`             | Người dùng không thể xóa (JSON Array)     | `["admin@anhem.com:Admin@123456"]`                                                                       |
| `DEFAULT_ROLES_FOR_NEW_USER_LIST`  | Roles mặc định cho user mới (JSON Array)  | `["User"]`                                                                                               |
| `OTLP_ENDPOINT`                    | Địa chỉ OpenTelemetry OTLP                | `http://your-otel-collector:4317`                                                                        |
| `GOOGLE_CLIENT_ID`                 | Google OAuth Client ID                    | `your-google-client-id.apps.googleusercontent.com`                                                       |
| `GOOGLE_CLIENT_SECRET`             | Google OAuth Client Secret                | `GOCSPX-your-google-secret`                                                                              |
| `FACEBOOK_APP_ID`                  | Facebook App ID                           | `your-facebook-app-id`                                                                                   |
| `FACEBOOK_APP_SECRET`              | Facebook App Secret                       | `your-facebook-app-secret`                                                                               |
| `VNPAY__TMN_CODE`                  | VNPay Terminal Code (TmnCode)             | `XXT7WT32`                                                                                               |
| `VNPAY__HASH_SECRET`               | VNPay Hash Secret                         | `1ff1b8243e86dd74cd6f4c284b06bb2ad5...`                                                                  |
| `VNPAY__BASE_URL`                  | VNPay Base URL (Sandbox/Production)       | `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html`                                                     |
| `VNPAY__CALLBACK_URL`              | VNPay Return/Callback URL                 | `https://api.yourdomain.com/api/payment/vnpay-callback`                                                  |
| `PAYOS__CLIENT_ID`                 | PayOS Client ID                           | `your-payos-client-id`                                                                                   |
| `PAYOS__API_KEY`                   | PayOS API Key                             | `your-payos-api-key`                                                                                     |
| `PAYOS__CHECKSUM_KEY`              | PayOS Checksum Key                        | `your-payos-checksum-key`                                                                                |
| `PAYOS__BASE_URL`                  | PayOS Base URL                            | `https://api-merchant.payos.vn`                                                                          |
| `PAYOS__RETURN_URL`                | PayOS Return URL                          | `https://yourdomain.online/payment-processing`                                                           |
| `PAYOS__CANCEL_URL`                | PayOS Cancel URL                          | `https://yourdomain.online/payment-processing`                                                           |
| `UPLOAD_PATH`                      | Đường dẫn lưu trữ ảnh vĩnh viễn           | `/var/www/anhemmotor/uploads`                                                                            |
| `GHN_TOKEN`                        | Mã API Token từ GHN                       | `a1b2c3d4e5f6g7h8...`                                                                                    |
| `GHN_SHOP_ID`                      | Mã cửa hàng (Shop ID) trên GHN            | `012345`                                                                                                 |
| `GHN_BASE_URL`                     | Địa chỉ API của GHN                       | `https://dev-online-gateway.ghn.vn`                                                                      |
| `GEMINI_API_KEY`                   | Google Gemini API Key                     | `AIzaSyB...`                                                                                             |
| `GEMINI_MODEL`                     | Tên mô hình Gemini                        | `gemini-3.5-flash`                                                                                       |
| `LANGSMITH_TRACING`                | Bật LangSmith tracing (true/false)        | `true`                                                                                                   |
| `LANGSMITH_API_KEY`                | LangSmith API Key                         | `lsv2_pt_...`                                                                                            |

### Array Secrets (SuperRoles, ProtectedUsers, DefaultRoles)

**Quan trọng:** GitHub Secrets hỗ trợ JSON array format!

#### SUPER_ROLES_LIST

**Single value:**

```json
["Admin"]
```

**Multiple values:**

```json
["Admin", "SuperAdmin", "Manager"]
```

#### PROTECTED_USERS_LIST

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

#### DEFAULT_ROLES_FOR_NEW_USER_LIST

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

# 8. Hướng dẫn Cấu hình Thanh toán Online

Để tính năng thanh toán hoạt động, bạn cần lấy các mã bí mật từ cổng VNPay và PayOS.

### 1. Cấu hình VNPay (Môi trường Sandbox)

1.  **Đăng ký tài khoản Test**: Truy cập [VNPay Sandbox](https://sandbox.vnpayment.vn/devreg/) hoặc liên hệ VNPay để lấy tài khoản test.
2.  **Lấy thông tin**: Sau khi đăng ký, bạn sẽ nhận được email chứa:
    - **vnp_TmnCode**: Mã định danh cửa hàng (Terminal ID).
    - **vnp_HashSecret**: Chuỗi bí mật dùng để tạo chữ ký (Secret Key).
3.  **Cấu hình Callback**: Đảm bảo URL gọi lại khớp với cấu hình của bạn:
    - `https://api.yourdomain.com/api/payment/vnpay-callback` (Production)
    - `https://localhost:7001/api/payment/vnpay-callback` (Local)

### 2. Cấu hình PayOS (Chính thức hoặc Test)

1.  **Tạo tài khoản**: Đăng ký tại [PayOS.vn](https://payos.vn/).
2.  **Tạo kênh thanh toán**: Vào mục **"Kênh thanh toán"** -> **"Tạo kênh thanh toán"** -> Chọn loại hình phù hợp.
3.  **Lấy API Key**: Ở lần đầu tiên sau khi tạo kênh thanh toán hoặc khi bấm vào kênh thanh toán trong phần "Kênh thanh toán", bạn sẽ thấy:
    - **Client ID**
    - **API Key**
    - **Checksum Key**

# 9. Hướng dẫn Cấu hình Giao Hàng Nhanh (GHN)

Để kích hoạt tính năng tự động đẩy đơn hàng sang GHN, bạn cần thiết lập cấu hình trên GitHub Secrets và trong appsetting.\*.json. Hướng dẫn sau sẽ giúp bạn lấy được 3 thông tin này dưới tầng Staging/Development (Không phải production, tránh bị mất tiền):

1. **Đăng nhập**: Truy cập vào [Website Khách hàng GHN](https://5sao.ghn.dev/).
2. **Lấy Shop ID (Mã Cửa Hàng)**: Bạn sẽ nhấn vào chữ "Chủ cửa hàng" ở tại sidebar, Ở trong tab "Thông tin cá nhân", có 1 ô ID. ID này chính là giá trị cho bí mật `GHN_SHOP_ID` (và trong file appsettings.\*.json là GHNSettings -> ShopId).
3. **Lấy API Token**: Cũng ở trong mục "Chủ cửa hàng", chọn tab "Bảo mật", trên dòng "Token API & IP Tin cậy", nhấn "Quản lý", 1 tab khác hiện ra, trong phần "Token API", bạn nhấn vào hình con mắt, xác thực số điện thoại và 1 chuỗi sẽ hiện ra. Đây chính là giá trị cho bí mật `GHN_TOKEN` (và trong file appsettings.\*.json là GHNSettings -> Token).
4. **URL GHN**: Sử dụng https://dev-online-gateway.ghn.vn nếu bạn ở môi trường Development. Đây là GHN_BASE_URL của bạn (và trong file appsettings.\*.json là GHNSettings -> BaseUrl).
5. **Thay đổi vị trí của hàng**: Vào phần "Quản lý cửa hàng" ở phần Sidebar, bạn cần thay đổi/thêm địa điểm của cửa hàng mặc định ngay trong đó.
6. **Đăng kí Webhook**: Nếu như bạn muốn sử dụng Webhook để nhận tin khi có đơn hàng đã hoàn tất, bạn sẽ cần gửi mail đến địa chỉ api@ghn.vn với các thông tin làTên công ty, Mã khách hàng (ClientID), Môi trường yêu cầu, URL Webhook (chính là URL Endpoint chạy dự án này). Rồi chờ họ phản hồi.

# 10. Hướng dẫn Cấu hình AI Sidecar (Gemini & LangSmith)

Để sử dụng được các tính năng AI (ví dụ: tìm kiếm thông minh), bạn cần lấy Gemini API Key và tùy chọn lấy thêm LangSmith API Key để theo dõi (tracing).

### 1. Cách lấy Gemini API Key

1. Truy cập [Google AI Studio](https://aistudio.google.com/app/api-keys).
2. Đăng nhập bằng tài khoản Google của bạn.
3. Nhấn "Create API key" trong dự án hiện có hoặc tạo mới.
4. Copy API key vừa tạo và dán vào `AISetup -> GeminiApiKey` trong file `appsettings.json`.

### 2. Cách lấy LangSmith API Key (Tùy chọn Tracing)

1. Truy cập [LangSmith](https://smith.langchain.com/).
2. Đăng nhập hoặc tạo tài khoản.
3. Ở menu bên trái, chọn Settings (biểu tượng bánh răng) -> API Keys.
4. Nhấn "+ API Key", đặt tên và copy key.
5. Vào file `appsettings.json`, đặt `AISetup -> LangSmithTracing` thành `true` và dán key vào `LangSmithApiKey`.

# 11. Troubleshooting

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
