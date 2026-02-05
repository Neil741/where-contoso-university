# Contoso University 本地開發環境設定指南

## 📋 目錄

- [先決條件](#先決條件)
- [快速開始](#快速開始)
- [詳細設定步驟](#詳細設定步驟)
- [Docker 環境設定](#docker-環境設定)
- [環境變數配置](#環境變數配置)
- [Codespace 開發指南](#codespace-開發指南)
- [常見問題排除](#常見問題排除)
- [進階設定](#進階設定)

## 🔧 先決條件

### 必要軟體

#### 1. .NET 8.0 SDK
下載並安裝 [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

**驗證安裝**:
```bash
dotnet --version
# 應顯示 8.0.x 或更高版本
```

**檢查已安裝的 SDK**:
```bash
dotnet --list-sdks
```

#### 2. Docker Desktop（推薦）
下載並安裝 [Docker Desktop](https://www.docker.com/products/docker-desktop)

**支援的作業系統**:
- Windows 10/11 Pro, Enterprise, or Education (with WSL2)
- macOS 10.15 或更新版本
- Linux（Ubuntu, Debian, Fedora 等）

**驗證安裝**:
```bash
docker --version
docker-compose --version
```

#### 3. Git
下載並安裝 [Git](https://git-scm.com/downloads)

**驗證安裝**:
```bash
git --version
```

### 選擇性軟體

#### 1. IDE / 編輯器
選擇以下任一開發工具：

**Visual Studio 2022（推薦用於 Windows）**
- [下載 Community Edition](https://visualstudio.microsoft.com/downloads/)（免費）
- 安裝工作負載：「ASP.NET 和 Web 開發」

**Visual Studio Code（跨平台）**
- [下載 VS Code](https://code.visualstudio.com/)
- 安裝擴充功能：
  - C# Dev Kit
  - C# 擴充功能
  - Docker
  - GitLens（選擇性）

**JetBrains Rider（商業授權）**
- [下載 Rider](https://www.jetbrains.com/rider/)
- 內建完整的 .NET 開發支援

#### 2. SQL Server Management Studio（選擇性）
用於直接管理資料庫：
- [下載 SSMS](https://docs.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms)
- 或使用 [Azure Data Studio](https://docs.microsoft.com/sql/azure-data-studio/download)（跨平台）

#### 3. Postman 或 Thunder Client
用於測試 API（未來功能）：
- [下載 Postman](https://www.postman.com/downloads/)
- 或在 VS Code 中安裝 Thunder Client 擴充功能

### 系統需求

| 元件 | 最低需求 | 建議需求 |
|------|----------|----------|
| CPU | 2 核心 | 4 核心或更多 |
| RAM | 4 GB | 8 GB 或更多 |
| 硬碟空間 | 10 GB | 20 GB 或更多（包含 Docker） |
| 作業系統 | Windows 10, macOS 10.15, Ubuntu 20.04 | 最新版本 |

## 🚀 快速開始

### 5 分鐘快速設定

```bash
# 1. 克隆專案
git clone https://github.com/your-org/where-contoso-university.git
cd where-contoso-university

# 2. 啟動 SQL Server（Docker）
docker-compose up -d

# 3. 等待資料庫啟動（約 30 秒）
echo "等待 SQL Server 啟動..."
sleep 30

# 4. 切換到專案目錄
cd ContosoUniversity

# 5. 還原套件並執行
dotnet restore
dotnet run
```

**完成！** 開啟瀏覽器並前往 `https://localhost:5001`

## 📚 詳細設定步驟

### 步驟 1：克隆專案

```bash
# 使用 HTTPS
git clone https://github.com/your-org/where-contoso-university.git

# 或使用 SSH（如果已設定 SSH 金鑰）
git clone git@github.com:your-org/where-contoso-university.git

# 進入專案目錄
cd where-contoso-university
```

### 步驟 2：檢查專案結構

```bash
# 列出專案內容
ls -la

# 應該看到以下資料夾：
# - ContosoUniversity/         （.NET 8.0 版本）
# - ContosoUniversity.Legacy/  （原始 .NET Framework 版本）
# - Docs/                      （文件）
# - Scripts/                   （部署腳本）
# - docker-compose.yml         （Docker 設定）
```

### 步驟 3：啟動資料庫

#### 選項 A：使用 Docker（推薦）

**啟動 SQL Server 容器**:
```bash
docker-compose up -d
```

**檢查容器狀態**:
```bash
docker ps

# 應該看到：
# CONTAINER ID   IMAGE                                        STATUS
# xxxxx          mcr.microsoft.com/mssql/server:2022-latest   Up 30 seconds
```

**查看容器日誌**:
```bash
docker-compose logs -f sqlserver
```

等待看到類似以下訊息：
```
SQL Server is now ready for client connections.
```

**測試資料庫連線**:
```bash
# Windows (使用 Docker Desktop 的終端機)
docker exec -it contoso-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -Q 'SELECT @@VERSION'

# Linux/macOS
docker exec -it contoso-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -Q 'SELECT @@VERSION'
```

#### 選項 B：使用本地 SQL Server

如果您已安裝 SQL Server：

**Windows (SQL Server Express LocalDB)**:
```bash
# 檢查 LocalDB 是否安裝
sqllocaldb info

# 啟動 MSSQLLocalDB 實例
sqllocaldb start MSSQLLocalDB
```

**連線字串**（更新 appsettings.Development.json）:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ContosoUniversity;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**完整 SQL Server**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ContosoUniversity;Integrated Security=true;TrustServerCertificate=True"
  }
}
```

### 步驟 4：設定開發環境變數

#### 建立開發設定檔

在 `ContosoUniversity/` 目錄中，建立或編輯 `appsettings.Development.json`：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ContosoUniversity;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "NotificationSettings": {
    "Enabled": false,
    "ServiceBusConnectionString": "",
    "QueueName": "contoso-notifications"
  }
}
```

**說明**:
- `Logging`: 設定日誌層級
  - `Default`: 應用程式日誌
  - `Microsoft.AspNetCore`: ASP.NET Core 框架日誌
  - `Microsoft.EntityFrameworkCore.Database.Command`: SQL 查詢日誌（開發時有用）
- `ConnectionStrings`: 資料庫連線
- `NotificationSettings`: 通知服務設定（目前停用）

#### 驗證設定檔

```bash
cd ContosoUniversity
cat appsettings.Development.json
```

### 步驟 5：還原 NuGet 套件

```bash
cd ContosoUniversity
dotnet restore
```

**預期輸出**:
```
Determining projects to restore...
Restored ContosoUniversity.csproj (in 2.3 sec).
```

### 步驟 6：建置專案

```bash
dotnet build
```

**預期輸出**:
```
Microsoft (R) Build Engine version 17.x.x
...
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 步驟 7：執行應用程式

```bash
dotnet run
```

**預期輸出**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### 步驟 8：驗證安裝

開啟瀏覽器並訪問：
- **HTTPS**: `https://localhost:5001`
- **HTTP**: `http://localhost:5000`

**預期結果**:
- 看到 Contoso University 首頁
- 導覽列顯示：Home, Students, Courses, Instructors, Departments
- 資料庫已自動初始化並植入範例資料

**測試功能**:
1. 點擊 "Students" 檢視學生清單
2. 點擊 "Create New" 建立新學生
3. 測試編輯和刪除功能

## 🐳 Docker 環境設定

### Docker Compose 詳解

**docker-compose.yml** 內容：

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: contoso-sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    healthcheck:
      test: /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -Q 'SELECT 1' || exit 1
      interval: 10s
      timeout: 3s
      retries: 10
      start_period: 10s

volumes:
  sqlserver_data:
```

**設定說明**:
- `image`: 使用 SQL Server 2022 最新版本
- `ACCEPT_EULA=Y`: 接受 SQL Server 授權條款
- `SA_PASSWORD`: 管理員密碼（**生產環境請更改**）
- `MSSQL_PID=Developer`: 開發者版本（免費）
- `ports`: 將容器的 1433 埠對應到主機的 1433
- `volumes`: 持久化資料儲存
- `healthcheck`: 確保資料庫已準備好接受連線

### Docker 常用命令

#### 容器管理

```bash
# 啟動容器（背景執行）
docker-compose up -d

# 啟動容器（前景執行，查看日誌）
docker-compose up

# 停止容器
docker-compose stop

# 停止並移除容器
docker-compose down

# 停止並移除容器和資料卷（⚠️ 會刪除資料）
docker-compose down -v

# 重啟容器
docker-compose restart
```

#### 日誌和監控

```bash
# 查看日誌
docker-compose logs

# 持續追蹤日誌
docker-compose logs -f

# 查看特定服務日誌
docker-compose logs sqlserver

# 查看最後 50 行日誌
docker-compose logs --tail=50
```

#### 資料庫管理

```bash
# 進入容器的 bash shell
docker exec -it contoso-sqlserver bash

# 執行 SQL 命令
docker exec -it contoso-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd'

# 快速查詢
docker exec -it contoso-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -Q 'SELECT name FROM sys.databases'
```

#### 資料備份和還原

**備份資料庫**:
```bash
# 建立備份目錄
docker exec contoso-sqlserver mkdir -p /var/opt/mssql/backup

# 備份資料庫
docker exec contoso-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -Q "BACKUP DATABASE ContosoUniversity TO DISK='/var/opt/mssql/backup/ContosoUniversity.bak'"

# 複製備份到主機
docker cp contoso-sqlserver:/var/opt/mssql/backup/ContosoUniversity.bak ./ContosoUniversity.bak
```

**還原資料庫**:
```bash
# 複製備份到容器
docker cp ./ContosoUniversity.bak contoso-sqlserver:/var/opt/mssql/backup/

# 還原資料庫
docker exec contoso-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -Q "RESTORE DATABASE ContosoUniversity FROM DISK='/var/opt/mssql/backup/ContosoUniversity.bak' WITH REPLACE"
```

### 更改 SQL Server 密碼

**警告**: 預設密碼僅供開發使用，生產環境必須更改！

**步驟**:
1. 編輯 `docker-compose.yml`，更改 `SA_PASSWORD`
2. 更新 `appsettings.Development.json` 中的連線字串
3. 重新建立容器：
   ```bash
   docker-compose down -v
   docker-compose up -d
   ```

## ⚙️ 環境變數配置

### 設定檔階層

ASP.NET Core 使用階層式設定系統：

```
appsettings.json                      （基本設定）
  ↓ 覆寫
appsettings.Development.json          （開發環境）
  ↓ 覆寫
appsettings.Production.json           （生產環境）
  ↓ 覆寫
環境變數                              （最高優先權）
```

### 開發環境設定範例

**appsettings.Development.json**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ContosoUniversity;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "NotificationSettings": {
    "Enabled": false,
    "ServiceBusConnectionString": "",
    "QueueName": "contoso-notifications"
  },
  "DetailedErrors": true,
  "DeveloperExceptionPage": true
}
```

### 使用者機密（User Secrets）

**敏感資訊不應存在設定檔中！**

#### 初始化 User Secrets

```bash
cd ContosoUniversity
dotnet user-secrets init
```

這會在專案檔中新增：
```xml
<PropertyGroup>
  <UserSecretsId>aspnet-ContosoUniversity-xxxxx</UserSecretsId>
</PropertyGroup>
```

#### 設定機密

```bash
# 設定資料庫密碼
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ContosoUniversity;User Id=sa;Password=MySecretPassword;TrustServerCertificate=True"

# 設定 Service Bus 連線字串
dotnet user-secrets set "NotificationSettings:ServiceBusConnectionString" "Endpoint=sb://..."

# 列出所有機密
dotnet user-secrets list

# 移除機密
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"

# 清除所有機密
dotnet user-secrets clear
```

#### 機密儲存位置

**Windows**:
```
%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json
```

**Linux / macOS**:
```
~/.microsoft/usersecrets/<user_secrets_id>/secrets.json
```

### 環境變數設定

#### Windows (PowerShell)

**臨時設定（當前工作階段）**:
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;..."
```

**永久設定**:
```powershell
[Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development", "User")
```

#### Linux / macOS

**臨時設定**:
```bash
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="Server=localhost,1433;..."
```

**永久設定**（加入到 `~/.bashrc` 或 `~/.zshrc`）:
```bash
echo 'export ASPNETCORE_ENVIRONMENT=Development' >> ~/.bashrc
source ~/.bashrc
```

### 驗證設定

**檢查當前環境**:
```bash
dotnet run --environment Development
```

**程式碼中檢查設定**:
```csharp
// 在 Controller 中
public class HomeController : Controller
{
    private readonly IConfiguration _configuration;
    
    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public IActionResult TestConfig()
    {
        var connString = _configuration.GetConnectionString("DefaultConnection");
        var isEnabled = _configuration.GetValue<bool>("NotificationSettings:Enabled");
        
        return Content($"Connection: {connString}\nNotifications: {isEnabled}");
    }
}
```

## ☁️ Codespace 開發指南

GitHub Codespaces 提供完整的雲端開發環境，無需本地安裝。

### 建立 Codespace

#### 方法 1：從 GitHub 網站

1. 前往專案的 GitHub 頁面
2. 點擊綠色的 "Code" 按鈕
3. 選擇 "Codespaces" 標籤
4. 點擊 "Create codespace on main"

#### 方法 2：使用 GitHub CLI

```bash
# 安裝 GitHub CLI
# Windows: winget install GitHub.cli
# macOS: brew install gh
# Linux: 參考 https://github.com/cli/cli#installation

# 登入
gh auth login

# 建立 Codespace
gh codespace create --repo your-org/where-contoso-university

# 列出 Codespaces
gh codespace list

# 連接到 Codespace
gh codespace code
```

### Codespace 設定檔

專案包含 `.devcontainer/devcontainer.json` 設定：

```json
{
  "name": "Contoso University Dev Container",
  "image": "mcr.microsoft.com/devcontainers/dotnet:8.0",
  "features": {
    "ghcr.io/devcontainers/features/docker-in-docker:2": {},
    "ghcr.io/devcontainers/features/azure-cli:1": {}
  },
  "forwardPorts": [5000, 5001, 1433],
  "postCreateCommand": "docker-compose up -d && cd ContosoUniversity && dotnet restore",
  "customizations": {
    "vscode": {
      "extensions": [
        "ms-dotnettools.csharp",
        "ms-dotnettools.csdevkit",
        "ms-azuretools.vscode-docker",
        "humao.rest-client"
      ]
    }
  }
}
```

### Codespace 開發工作流程

#### 1. 啟動 Codespace

Codespace 建立後會自動：
- 安裝 .NET 8.0 SDK
- 啟動 Docker
- 執行 `docker-compose up -d`（啟動 SQL Server）
- 執行 `dotnet restore`

#### 2. 執行應用程式

```bash
cd ContosoUniversity
dotnet run
```

#### 3. 存取應用程式

Codespace 會自動轉發埠口 5000 和 5001：
- VS Code 會顯示通知："Your application running on port 5001 is available"
- 點擊 "Open in Browser" 或
- 前往 "PORTS" 標籤，點擊埠口 5001 的 🌐 圖示

#### 4. 資料庫管理

使用 VS Code 的 SQL Server 擴充功能：
- 伺服器: `localhost,1433`
- 使用者: `sa`
- 密碼: `YourStrong@Passw0rd`
- 資料庫: `ContosoUniversity`

#### 5. 停止 Codespace

```bash
# 在 Codespace 終端機中
gh codespace stop

# 或從 GitHub 網站管理 Codespaces
```

### Codespace 最佳實踐

#### 資料持久化
Codespace 的工作區資料會持久化，但 Docker 容器資料不會。

**建議**:
```bash
# 使用命名的資料卷
docker volume create contoso-db-data

# 在 docker-compose.yml 中使用
volumes:
  - contoso-db-data:/var/opt/mssql
```

#### 效能優化
```bash
# 使用 .dockerignore 排除不需要的檔案
echo "bin/" >> .dockerignore
echo "obj/" >> .dockerignore
echo ".vs/" >> .dockerignore
```

#### 成本控制
- 不使用時停止 Codespace
- 設定自動停止時間（設定 → 超時）
- 定期刪除不需要的 Codespaces

### Codespace 常見任務

#### 重建容器
```bash
# 如果環境有問題，重建容器
# Ctrl+Shift+P → "Codespaces: Rebuild Container"
```

#### 安裝額外工具
```bash
# 安裝 Azure CLI（如果未自動安裝）
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# 安裝 SQL Server 命令列工具
sudo apt-get install mssql-tools unixodbc-dev
```

#### 設定 Git
```bash
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

## 🔍 常見問題排除

### 問題 1：無法連接到資料庫

**症狀**:
```
SqlException: A network-related or instance-specific error occurred...
```

**解決方案**:

**檢查 1：Docker 容器是否執行**
```bash
docker ps

# 如果沒有看到 contoso-sqlserver，啟動它
docker-compose up -d
```

**檢查 2：等待資料庫準備好**
```bash
# SQL Server 需要 20-30 秒啟動
docker-compose logs -f sqlserver

# 等待看到："SQL Server is now ready for client connections"
```

**檢查 3：測試連線**
```bash
docker exec -it contoso-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -Q 'SELECT 1'
```

**檢查 4：防火牆設定**
```bash
# Windows：確保允許 Docker 通過防火牆
# Linux：檢查 iptables 規則
sudo iptables -L | grep 1433
```

**檢查 5：連線字串**
驗證 `appsettings.Development.json` 中的連線字串：
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ContosoUniversity;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### 問題 2：編譯錯誤

**症狀**:
```
error CS0246: The type or namespace name 'X' could not be found
```

**解決方案**:

```bash
# 清理建置
dotnet clean

# 刪除 bin 和 obj 資料夾
rm -rf bin obj

# 還原套件
dotnet restore

# 重新建置
dotnet build
```

### 問題 3：資料庫遷移錯誤

**症狀**:
```
The model backing the 'SchoolContext' context has changed...
```

**解決方案**:

```bash
# 刪除資料庫並重新建立
docker exec -it contoso-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -Q "DROP DATABASE ContosoUniversity"

# 重新執行應用程式（會自動建立資料庫）
dotnet run
```

### 問題 4：埠口已被使用

**症狀**:
```
Unable to bind to https://localhost:5001 on the IPv4 loopback interface: 'Address already in use'.
```

**解決方案**:

**選項 A：找出並停止占用埠口的程式**
```bash
# Windows
netstat -ano | findstr :5001
taskkill /PID <PID> /F

# Linux/macOS
lsof -i :5001
kill -9 <PID>
```

**選項 B：使用不同的埠口**
```bash
dotnet run --urls "https://localhost:5101;http://localhost:5100"
```

### 問題 5：Docker 容器無法啟動

**症狀**:
```
Error response from daemon: driver failed programming external connectivity
```

**解決方案**:

```bash
# 重啟 Docker Desktop
# Windows/macOS：從系統托盤重啟

# Linux：重啟 Docker 服務
sudo systemctl restart docker

# 清理並重新啟動
docker-compose down
docker system prune -f
docker-compose up -d
```

### 問題 6：熱重載不工作

**症狀**: 修改程式碼後需要手動重啟

**解決方案**:

**啟用檔案監視**:
```bash
dotnet watch run
```

**或在 Visual Studio 中啟用 Hot Reload**:
- 工具 → 選項 → 偵錯 → .NET/C++ 熱重載 → 啟用

### 問題 7：Nullable 警告過多

**症狀**:
```
warning CS8618: Non-nullable property 'X' must contain a non-null value...
```

**解決方案**:

**選項 A：暫時停用**（不建議）
在專案檔中：
```xml
<PropertyGroup>
  <Nullable>disable</Nullable>
</PropertyGroup>
```

**選項 B：正確處理 nullable**（建議）
```csharp
// 初始化非 nullable 屬性
public string LastName { get; set; } = string.Empty;

// 或使用 nullable 類型
public string? MiddleName { get; set; }

// 或在建構函式中初始化
public Student(string lastName)
{
    LastName = lastName;
}
```

### 問題 8：EF Core 工具未安裝

**症狀**:
```
No executable found matching command "dotnet-ef"
```

**解決方案**:

```bash
# 安裝全域工具
dotnet tool install --global dotnet-ef

# 驗證安裝
dotnet ef --version

# 如果已安裝但仍有問題，更新它
dotnet tool update --global dotnet-ef
```

### 問題 9：SSL 憑證錯誤

**症狀**:
```
Unable to configure HTTPS endpoint. The certificate file could not be loaded...
```

**解決方案**:

```bash
# 信任開發憑證
dotnet dev-certs https --trust

# 如果仍有問題，清除並重新建立
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

## 🚀 進階設定

### 多環境設定

**建立 Staging 環境**:

1. 建立 `appsettings.Staging.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=staging-server;Database=ContosoUniversity;..."
  }
}
```

2. 執行時指定環境:
```bash
dotnet run --environment Staging
```

### 效能分析

**啟用詳細的 EF Core 日誌**:
```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

**使用 MiniProfiler**:
```bash
dotnet add package MiniProfiler.AspNetCore.Mvc --version 4.3.8
```

### 自動化設定腳本

**Windows (PowerShell)**:

建立 `setup-dev.ps1`:
```powershell
#!/usr/bin/env pwsh

Write-Host "Setting up Contoso University development environment..." -ForegroundColor Green

# 檢查 .NET SDK
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -ne 0) {
    Write-Error ".NET SDK not found. Please install .NET 8.0 SDK."
    exit 1
}
Write-Host "✓ .NET SDK $dotnetVersion found" -ForegroundColor Green

# 檢查 Docker
docker --version | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker not found. Please install Docker Desktop."
    exit 1
}
Write-Host "✓ Docker found" -ForegroundColor Green

# 啟動 SQL Server
Write-Host "Starting SQL Server container..." -ForegroundColor Yellow
docker-compose up -d

Write-Host "Waiting for SQL Server to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# 還原套件
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
Set-Location ContosoUniversity
dotnet restore

# 建置專案
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build

Write-Host "`nSetup complete! Run 'dotnet run' to start the application." -ForegroundColor Green
```

**Linux/macOS (Bash)**:

建立 `setup-dev.sh`:
```bash
#!/bin/bash

echo "Setting up Contoso University development environment..."

# 檢查 .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 8.0 SDK."
    exit 1
fi
echo "✓ .NET SDK $(dotnet --version) found"

# 檢查 Docker
if ! command -v docker &> /dev/null; then
    echo "❌ Docker not found. Please install Docker."
    exit 1
fi
echo "✓ Docker found"

# 啟動 SQL Server
echo "Starting SQL Server container..."
docker-compose up -d

echo "Waiting for SQL Server to be ready..."
sleep 30

# 還原套件
echo "Restoring NuGet packages..."
cd ContosoUniversity
dotnet restore

# 建置專案
echo "Building project..."
dotnet build

echo ""
echo "✓ Setup complete! Run 'dotnet run' to start the application."
```

**使用腳本**:
```bash
# Windows
.\setup-dev.ps1

# Linux/macOS
chmod +x setup-dev.sh
./setup-dev.sh
```

## 📝 開發檢查清單

### 每日開發工作流程

- [ ] 啟動 Docker 容器
  ```bash
  docker-compose up -d
  ```

- [ ] 拉取最新程式碼
  ```bash
  git pull origin main
  ```

- [ ] 還原套件（如有變更）
  ```bash
  dotnet restore
  ```

- [ ] 執行應用程式
  ```bash
  cd ContosoUniversity
  dotnet watch run
  ```

- [ ] 進行開發和測試

- [ ] 提交變更
  ```bash
  git add .
  git commit -m "Your commit message"
  git push
  ```

- [ ] 停止 Docker（可選）
  ```bash
  docker-compose stop
  ```

## 🎓 下一步

現在您的開發環境已設定完成，您可以：

1. **探索程式碼**
   - 查看 Controllers、Models、Views
   - 理解 EF Core 資料存取模式
   - 研究依賴注入使用

2. **閱讀文件**
   - [遷移報告](UPGRADE_REPORT.md) - 了解架構變更
   - [部署指南](DEPLOYMENT_GUIDE.md) - 學習如何部署

3. **開始開發**
   - 新增新功能
   - 修改現有功能
   - 撰寫測試

4. **學習資源**
   - [ASP.NET Core 官方文件](https://docs.microsoft.com/aspnet/core)
   - [Entity Framework Core 文件](https://docs.microsoft.com/ef/core)
   - [.NET 8.0 新功能](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-8)

## 💬 取得協助

如果遇到問題：

1. 查看本指南的[常見問題排除](#常見問題排除)部分
2. 搜尋專案的 [GitHub Issues](https://github.com/your-org/where-contoso-university/issues)
3. 開啟新的 Issue 描述您的問題
4. 加入社群討論

---

**祝您開發愉快！** 🚀
