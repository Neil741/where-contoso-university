# Contoso University 本地開發環境設置指南

## 📋 目錄

1. [環境需求](#環境需求)
2. [GitHub Codespaces 快速設置](#github-codespaces-快速設置)
3. [本地開發環境設置](#本地開發環境設置)
4. [SQL Server 容器設置](#sql-server-容器設置)
5. [應用程式配置](#應用程式配置)
6. [運行應用程式](#運行應用程式)
7. [資料庫初始化](#資料庫初始化)
8. [測試 CRUD 操作](#測試-crud-操作)
9. [常見問題排解](#常見問題排解)

---

## 環境需求

### 最低要求

| 元件 | 版本 | 說明 |
|------|------|------|
| .NET SDK | 8.0 或更高 | [下載連結](https://dotnet.microsoft.com/download/dotnet/8.0) |
| SQL Server | 2019+ 或 Docker | 本地或容器化執行 |
| IDE | VS Code / Visual Studio | 推薦 VS Code + C# Dev Kit |
| Docker | 20.10+ | 用於運行 SQL Server 容器（可選） |
| Git | 2.30+ | 版本控制 |

### 推薦配置

- **記憶體**：至少 4GB RAM（推薦 8GB）
- **硬碟空間**：至少 2GB 可用空間
- **作業系統**：Windows 10/11、macOS 10.15+、或 Ubuntu 20.04+

---

## GitHub Codespaces 快速設置

GitHub Codespaces 提供了最快速的開發環境設置方式，無需本地安裝任何軟體。

### 步驟 1：啟動 Codespace

1. 前往 GitHub 專案頁面
2. 點擊綠色的 **Code** 按鈕
3. 選擇 **Codespaces** 分頁
4. 點擊 **Create codespace on main**

```bash
# Codespace 會自動設置以下內容：
# - .NET 8.0 SDK
# - SQL Server 開發工具
# - 必要的 VS Code 擴充功能
```

### 步驟 2：啟動 SQL Server 容器

在 Codespace 終端機中執行：

```bash
# 啟動 SQL Server 容器
docker run -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 \
  --name sqlserver \
  --hostname sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest

# 驗證容器運行狀態
docker ps | grep sqlserver
```

### 步驟 3：還原套件並運行

```bash
# 切換到專案目錄
cd ContosoUniversity

# 設定 ASPNETCORE 環境為測試
export ASPNETCORE_ENVIRONMENT=Development

# 還原 NuGet 套件
dotnet restore

# 建置專案
dotnet build

# 運行應用程式
dotnet run
```

應用程式將在 `http://localhost:5000` 啟動。Codespaces 會自動轉發埠口並提供可存取的 URL。

---

## 本地開發環境設置

### Windows 設置

#### 1. 安裝 .NET 8.0 SDK

```powershell
# 使用 Winget 安裝
winget install Microsoft.DotNet.SDK.8

# 驗證安裝
dotnet --version
```

#### 2. 安裝 SQL Server

**選項 A：使用 SQL Server Express（推薦用於 Windows）**

1. 下載 [SQL Server 2022 Express](https://www.microsoft.com/sql-server/sql-server-downloads)
2. 選擇 **Basic** 安裝類型
3. 接受授權條款並完成安裝
4. 記下伺服器名稱（通常是 `localhost\SQLEXPRESS` 或 `(localdb)\MSSQLLocalDB`）

**選項 B：使用 Docker Desktop**

```powershell
# 啟動 SQL Server 容器
docker run -e "ACCEPT_EULA=Y" `
  -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" `
  -p 1433:1433 `
  --name sqlserver `
  -d mcr.microsoft.com/mssql/server:2022-latest
```

### macOS 設置

#### 1. 安裝 .NET 8.0 SDK

```bash
# 使用 Homebrew 安裝
brew install --cask dotnet-sdk

# 驗證安裝
dotnet --version
```

#### 2. 安裝 Docker Desktop

```bash
# 使用 Homebrew 安裝
brew install --cask docker

# 或從官網下載
# https://www.docker.com/products/docker-desktop
```

### Linux (Ubuntu/Debian) 設置

#### 1. 安裝 .NET 8.0 SDK

```bash
# 新增 Microsoft 套件來源
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# 安裝 .NET SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# 驗證安裝
dotnet --version
```

#### 2. 安裝 Docker

```bash
# 安裝 Docker
sudo apt-get update
sudo apt-get install -y docker.io

# 啟動 Docker 服務
sudo systemctl start docker
sudo systemctl enable docker

# 將當前使用者加入 docker 群組
sudo usermod -aG docker $USER

# 重新登入以套用群組變更
```

---

## SQL Server 容器設置

使用 Docker 容器是跨平台開發的推薦方式。

### 啟動 SQL Server 容器

```bash
# 基本啟動命令
docker run -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 \
  --name sqlserver \
  --hostname sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest

# 帶資料持久化的啟動命令（推薦）
docker run -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 \
  --name sqlserver \
  --hostname sqlserver \
  -v sqlvolume:/var/opt/mssql \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

### 參數說明

| 參數 | 說明 |
|------|------|
| `ACCEPT_EULA=Y` | 接受 SQL Server 授權條款 |
| `MSSQL_SA_PASSWORD` | SA 帳戶密碼（必須符合強度要求） |
| `-p 1433:1433` | 映射埠口（主機:容器） |
| `--name sqlserver` | 容器名稱 |
| `-v sqlvolume:/var/opt/mssql` | 資料持久化儲存卷 |
| `-d` | 在背景執行 |

### 容器管理命令

```bash
# 檢查容器狀態
docker ps -a | grep sqlserver

# 停止容器
docker stop sqlserver

# 啟動容器
docker start sqlserver

# 重新啟動容器
docker restart sqlserver

# 檢視容器日誌
docker logs sqlserver

# 檢視最新 50 行日誌
docker logs --tail 50 sqlserver

# 即時監控日誌
docker logs -f sqlserver

# 刪除容器（會刪除資料）
docker rm -f sqlserver
```

### 連線到 SQL Server 容器

#### 使用 sqlcmd（在容器內）

```bash
# 進入容器
docker exec -it sqlserver /bin/bash

# 連線到 SQL Server
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd"

# 執行測試查詢
SELECT @@VERSION
GO

# 列出所有資料庫
SELECT name FROM sys.databases
GO

# 退出
exit
```

#### 使用 Azure Data Studio（推薦）

1. 下載 [Azure Data Studio](https://docs.microsoft.com/sql/azure-data-studio/download)
2. 建立新連線：
   - **伺服器**：`localhost,1433`
   - **使用者名稱**：`sa`
   - **密碼**：`YourStrong@Passw0rd`
   - **信任伺服器憑證**：勾選
3. 點擊 **連線**

---

## 應用程式配置

### 設定連線字串

#### 開發環境配置

編輯 `ContosoUniversity/appsettings.Development.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ContosoUniversityCore;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

#### 不同環境的連線字串範例

**使用 SQL Server Express（Windows）**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=ContosoUniversityCore;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

**使用 LocalDB（Windows）**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ContosoUniversityCore;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**使用 Docker 容器（跨平台）**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ContosoUniversityCore;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true"
  }
}
```

### 連線字串參數說明

| 參數 | 說明 | 必要 |
|------|------|------|
| `Server` | SQL Server 位址與埠口 | ✅ |
| `Database` | 資料庫名稱 | ✅ |
| `User Id` | 使用者名稱 | SQL 驗證必要 |
| `Password` | 密碼 | SQL 驗證必要 |
| `Trusted_Connection` | 使用 Windows 驗證 | Windows 整合驗證 |
| `TrustServerCertificate` | 信任自簽憑證 | 開發環境推薦 |
| `MultipleActiveResultSets` | 啟用 MARS | EF Core 推薦 |
| `Encrypt` | 加密連線 | Azure SQL 必要 |

### 環境變數配置（可選）

```bash
# Linux/macOS
export ConnectionStrings__DefaultConnection="Server=localhost;Database=ContosoUniversityCore;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true"

# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Server=localhost;Database=ContosoUniversityCore;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true"
```

---

## 運行應用程式

### 使用 .NET CLI

```bash
# 切換到專案目錄
cd ContosoUniversity

# 還原 NuGet 套件
dotnet restore

# 建置專案（可選）
dotnet build

# 運行應用程式
dotnet run

# 指定環境變數
dotnet run --environment Development

# 監看模式（自動重新載入）
dotnet watch run
```

### 使用 Visual Studio Code

1. 開啟專案資料夾
2. 按 `F5` 或點擊 **Run and Debug**
3. 選擇 **.NET Core Launch (web)**
4. 應用程式將啟動並自動開啟瀏覽器

### 使用 Visual Studio 2022

1. 開啟 `ContosoUniversity.sln`
2. 按 `F5` 或點擊 **開始偵錯**
3. 選擇 **ContosoUniversity** 專案作為啟動專案

### 驗證應用程式運行

```bash
# 應用程式啟動後，您應該看到類似輸出：
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

開啟瀏覽器並前往：
- **HTTP**：`http://localhost:5000`
- **HTTPS**：`https://localhost:5001`（如已配置）

---

## 資料庫初始化

應用程式使用 Code First 方法，並在啟動時自動初始化資料庫。

### 自動初始化（預設行為）

應用程式啟動時會自動執行：

1. **建立資料庫**（如果不存在）
2. **建立資料表**（基於 Entity Framework 模型）
3. **種子測試資料**（由 `DbInitializer.cs` 提供）

```csharp
// Program.cs 中的初始化邏輯
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SchoolContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
```

### 手動資料庫遷移（使用 EF Core Migrations）

如果需要更精細的控制，可以使用 EF Core Migrations：

```bash
# 安裝 EF Core 工具（如果尚未安裝）
dotnet tool install --global dotnet-ef

# 建立初始遷移
cd ContosoUniversity
dotnet ef migrations add InitialCreate

# 更新資料庫
dotnet ef database update

# 檢視遷移狀態
dotnet ef migrations list

# 產生 SQL 腳本（不執行）
dotnet ef migrations script

# 回滾遷移
dotnet ef database update PreviousMigrationName

# 移除最後一次遷移
dotnet ef migrations remove
```

### 重置資料庫

```bash
# 方法 1：刪除資料庫（透過 EF Core）
dotnet ef database drop --force
dotnet ef database update

# 方法 2：使用 SQL 命令
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "DROP DATABASE IF EXISTS ContosoUniversityCore"

# 重新啟動應用程式以重新建立資料庫
dotnet run
```

### 驗證資料庫建立

使用 Azure Data Studio 或 sqlcmd 檢查：

```sql
-- 檢查資料庫是否存在
SELECT name FROM sys.databases WHERE name = 'ContosoUniversityCore';
GO

-- 檢查資料表
USE ContosoUniversityCore;
GO
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;
GO

-- 檢查學生資料
SELECT COUNT(*) FROM Student;
GO

-- 查看前 10 筆學生記錄
SELECT TOP 10 * FROM Student;
GO
```

---

## 測試 CRUD 操作

### 測試學生管理功能

#### 1. 建立（Create）新學生

1. 啟動應用程式
2. 前往 `http://localhost:5000/Students`
3. 點擊 **Create New** 連結
4. 填寫表單：
   - **Last Name**：`測試`
   - **First Name**：`學生`
   - **Enrollment Date**：選擇今天日期
5. 點擊 **Create** 按鈕
6. **預期結果**：重導向至學生列表，新學生出現在清單中

#### 2. 讀取（Read）學生資料

```bash
# 測試列表頁面
curl http://localhost:5000/Students

# 測試詳細資訊頁面（假設 ID 為 1）
curl http://localhost:5000/Students/Details/1
```

**在瀏覽器中**：
1. 前往學生列表頁面
2. 點擊任一學生的 **Details** 連結
3. **預期結果**：顯示學生詳細資訊，包括選課記錄

#### 3. 更新（Update）學生資料

1. 在學生列表中點擊 **Edit** 連結
2. 修改學生資料：
   - 變更 **Last Name** 為新值
3. 點擊 **Save** 按鈕
4. **預期結果**：重導向至學生列表，資料已更新

#### 4. 刪除（Delete）學生

1. 在學生列表中點擊 **Delete** 連結
2. 確認刪除頁面顯示正確資料
3. 點擊 **Delete** 按鈕確認
4. **預期結果**：重導向至學生列表，學生已被移除

### 測試課程管理功能

```bash
# 課程列表
http://localhost:5000/Courses

# 建立新課程
http://localhost:5000/Courses/Create

# 編輯課程
http://localhost:5000/Courses/Edit/1

# 刪除課程
http://localhost:5000/Courses/Delete/1
```

### 測試講師管理功能

```bash
# 講師列表
http://localhost:5000/Instructors

# 查看講師詳細資訊（包含課程與學生）
http://localhost:5000/Instructors/Details/1
```

### 測試系所管理功能

```bash
# 系所列表
http://localhost:5000/Departments

# 建立新系所
http://localhost:5000/Departments/Create
```

### 測試統計報表

```bash
# 學生統計頁面
http://localhost:5000/Home/About
```

### API 測試（使用 curl）

```bash
# 取得所有學生（假設有 API 端點）
curl -X GET http://localhost:5000/api/students \
  -H "Content-Type: application/json"

# 建立新學生
curl -X POST http://localhost:5000/api/students \
  -H "Content-Type: application/json" \
  -d '{
    "lastName": "測試",
    "firstMidName": "學生",
    "enrollmentDate": "2024-01-19"
  }'
```

---

## 常見問題排解

### 1. 無法連線到 SQL Server

**症狀**：
```
Microsoft.Data.SqlClient.SqlException: A network-related or instance-specific error occurred
```

**解決方案**：

```bash
# 檢查 SQL Server 容器狀態
docker ps -a | grep sqlserver

# 如果容器未運行，啟動它
docker start sqlserver

# 檢查容器日誌
docker logs sqlserver

# 測試連線
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT @@VERSION"

# 檢查埠口是否被佔用
# Linux/macOS
lsof -i :1433
# Windows
netstat -ano | findstr :1433
```

### 2. 密碼不符合強度要求

**症狀**：
```
ERROR: Unable to set system administrator password: Password validation failed.
```

**解決方案**：

SQL Server 密碼必須符合以下要求：
- 至少 8 個字元
- 包含大寫字母
- 包含小寫字母
- 包含數字
- 包含特殊字元

```bash
# 使用符合要求的密碼
docker run -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd123" \
  -p 1433:1433 \
  --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

### 3. 埠口 1433 已被佔用

**症狀**：
```
docker: Error response from daemon: driver failed programming external connectivity on endpoint sqlserver: Bind for 0.0.0.0:1433 failed: port is already allocated.
```

**解決方案**：

```bash
# 方法 1：停止佔用埠口的程序
# Linux/macOS
sudo lsof -ti:1433 | xargs kill -9
# Windows
netstat -ano | findstr :1433
taskkill /PID <PID> /F

# 方法 2：使用不同的埠口
docker run -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1434:1433 \
  --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest

# 更新 appsettings.Development.json
# "Server=localhost,1434;Database=..."
```

### 4. 資料庫初始化失敗

**症狀**：
```
An error occurred while seeding the database.
```

**解決方案**：

```bash
# 檢查應用程式日誌
dotnet run

# 檢查 EF Core 日誌（在 appsettings.Development.json 中）
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}

# 手動初始化資料庫
dotnet ef database drop --force
dotnet ef database update

# 或直接刪除資料庫並重新啟動應用程式
```

### 5. NuGet 套件還原失敗

**症狀**：
```
error NU1101: Unable to find package
```

**解決方案**：

```bash
# 清除 NuGet 快取
dotnet nuget locals all --clear

# 重新還原套件
dotnet restore

# 如果使用私有來源，設定 NuGet 配置
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
```

### 6. HTTPS 憑證問題

**症狀**：
```
Your connection is not private / NET::ERR_CERT_AUTHORITY_INVALID
```

**解決方案**：

```bash
# 信任開發憑證
dotnet dev-certs https --trust

# 清除並重新建立憑證
dotnet dev-certs https --clean
dotnet dev-certs https --trust

# 或使用 HTTP（僅開發環境）
dotnet run --urls "http://localhost:5000"
```

### 7. 記憶體不足（容器）

**症狀**：
```
SQL Server terminated unexpectedly
```

**解決方案**：

```bash
# 為容器分配更多記憶體
docker run -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 \
  --name sqlserver \
  --memory 2g \
  -d mcr.microsoft.com/mssql/server:2022-latest

# 檢查 Docker 資源限制
docker stats sqlserver
```

### 8. Entity Framework 遷移衝突

**症狀**：
```
The migration has already been applied to the database
```

**解決方案**：

```bash
# 檢視遷移狀態
dotnet ef migrations list

# 移除未套用的遷移
dotnet ef migrations remove

# 強制更新到特定遷移
dotnet ef database update <MigrationName> --force
```

### 9. 監看模式無法啟動

**症狀**：
```
dotnet watch : Unable to find a project to build
```

**解決方案**：

```bash
# 確保在專案目錄中
cd ContosoUniversity

# 安裝 dotnet watch 工具
dotnet tool install --global dotnet-watch

# 清除建置快取
dotnet clean
dotnet build
dotnet watch run
```

### 10. Codespaces 埠口轉發問題

**症狀**：無法從瀏覽器存取應用程式

**解決方案**：

1. 檢查 Codespaces 埠口分頁
2. 確認埠口 5000 的可見性設為 **Public**
3. 點擊埠口旁的地球圖示以開啟轉發 URL
4. 或使用 Codespaces 提供的完整 URL：
   ```
   https://<codespace-name>-5000.githubpreview.dev
   ```

---

## 📚 其他資源

### 官方文件

- [.NET 8.0 文件](https://docs.microsoft.com/dotnet/core/)
- [ASP.NET Core 文件](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core 文件](https://docs.microsoft.com/ef/core/)
- [SQL Server on Linux](https://docs.microsoft.com/sql/linux/)

### 相關指南

- [UPGRADE_REPORT.md](./UPGRADE_REPORT.md) - 升級報告
- [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md) - 部署指南
- [README.md](../README.md) - 專案概覽

### 社群資源

- [.NET GitHub](https://github.com/dotnet)
- [Stack Overflow - .NET Core](https://stackoverflow.com/questions/tagged/.net-core)
- [Reddit - r/dotnet](https://www.reddit.com/r/dotnet/)

---

## 🆘 取得協助

如果遇到本指南未涵蓋的問題：

1. 檢查應用程式日誌（Console 輸出）
2. 檢查 SQL Server 容器日誌（`docker logs sqlserver`）
3. 查看 [GitHub Issues](https://github.com/your-repo/issues)
4. 聯絡開發團隊
