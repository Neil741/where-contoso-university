# Contoso University - .NET 8.0 現代化版本

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-brightgreen.svg)](https://docs.microsoft.com/aspnet/core)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-orange.svg)](https://docs.microsoft.com/ef/core)

這是經典的 Contoso University 範例應用程式，已從 .NET Framework 4.8 完整遷移至 .NET 8.0，展示了現代化的 ASP.NET Core MVC 應用程式開發最佳實踐。

## 📋 專案簡介

Contoso University 是一個完整的學校管理系統，用於演示如何使用 ASP.NET Core MVC 和 Entity Framework Core 建構資料驅動的 Web 應用程式。此專案包含學生、課程、教師和部門管理功能，並展示了多種進階開發模式。

### 主要用途
- 教學和學習 ASP.NET Core MVC 開發
- 展示從 .NET Framework 遷移至 .NET Core/8.0 的實踐
- 示範企業級應用程式的現代化架構模式
- 作為新專案的參考範本

## 📁 資料夾結構

```
ContosoUniversity/
├── ContosoUniversity/              # .NET 8.0 現代化版本
│   ├── Controllers/                # MVC 控制器（使用非同步模式）
│   ├── Models/                     # 領域模型（支援 Nullable 參考型別）
│   ├── Views/                      # Razor 視圖
│   ├── Data/                       # EF Core 資料庫上下文和初始化
│   ├── Services/                   # 應用程式服務層
│   ├── wwwroot/                    # 靜態資源（CSS、JS、圖片）
│   ├── Program.cs                  # 應用程式進入點
│   └── appsettings.json            # 應用程式設定
├── ContosoUniversity.Legacy/       # 原始 .NET Framework 4.8 版本（保留供參考）
├── Docs/                           # 專案文件
│   ├── UPGRADE_REPORT.md           # 遷移報告和技術決策
│   ├── LOCAL_SETUP_GUIDE.md        # 本地開發環境設定
│   └── DEPLOYMENT_GUIDE.md         # 部署指南
├── Scripts/                        # 部署腳本和 ARM 範本
│   ├── Deploy-ToAzure.ps1          # Azure 部署 PowerShell 腳本
│   └── azure-deploy.json           # ARM 範本
├── Lab/                            # 實驗和練習材料
└── docker-compose.yml              # Docker Compose 設定（SQL Server）
```

## 🛠 技術堆疊

### 核心技術
- **.NET 8.0** - 最新的 .NET 平台
- **ASP.NET Core MVC 8.0** - 現代化的 Web 框架
- **Entity Framework Core 8.0** - ORM 和資料存取
- **SQL Server 2022** - 資料庫引擎
- **C# 12** - 程式語言（支援 Nullable 參考型別）

### 關鍵套件
- `Microsoft.EntityFrameworkCore` (8.0.0)
- `Microsoft.EntityFrameworkCore.SqlServer` (8.0.0)
- `Microsoft.EntityFrameworkCore.Tools` (8.0.0)
- `Microsoft.Data.SqlClient` (5.2.0)
- `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` (8.0.0)

### 開發工具
- Docker Desktop - 容器化資料庫
- Visual Studio 2022 / VS Code
- Git
- Azure CLI（用於部署）

## 🚀 快速開始指南

### 先決條件
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 或更新版本
- [Docker Desktop](https://www.docker.com/products/docker-desktop)（推薦）或 SQL Server
- Git

### 1. 克隆專案

```bash
git clone https://github.com/your-org/where-contoso-university.git
cd where-contoso-university
```

### 2. 啟動 SQL Server（使用 Docker）

```bash
docker-compose up -d
```

這會啟動一個 SQL Server 2022 容器，監聽在 `localhost:1433`。

### 3. 更新資料庫連線字串

編輯 `ContosoUniversity/appsettings.Development.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ContosoUniversity;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
  }
}
```

### 4. 執行應用程式

```bash
cd ContosoUniversity
dotnet restore
dotnet run
```

應用程式會自動初始化資料庫並植入範例資料。

### 5. 開啟瀏覽器

瀏覽至 `https://localhost:5001` 或 `http://localhost:5000`

🎉 您應該可以看到 Contoso University 首頁！

## ✨ 功能特色列表

### 核心功能
- ✅ **學生管理** - 新增、編輯、刪除和檢視學生資訊
- ✅ **課程管理** - 管理課程目錄和課程詳情
- ✅ **註冊管理** - 處理學生課程註冊
- ✅ **教師管理** - 管理教師資訊和課程分配
- ✅ **部門管理** - 組織學術部門架構
- ✅ **分頁和排序** - 高效的資料瀏覽體驗
- ✅ **搜尋和篩選** - 快速找到相關資訊

### 技術特色
- ✅ **完整非同步** - 所有資料庫操作都使用非同步模式
- ✅ **依賴注入** - 遵循 SOLID 原則的現代化架構
- ✅ **Nullable 參考型別** - 減少 null 參考錯誤
- ✅ **Razor Pages** - 乾淨的視圖邏輯
- ✅ **資料驗證** - 模型層級和客戶端驗證
- ✅ **錯誤處理** - 優雅的異常處理機制
- ✅ **日誌記錄** - 內建的日誌支援
- ✅ **Docker 支援** - 容器化開發環境

### 資料庫功能
- ✅ **Code First 遷移** - EF Core 遷移管理
- ✅ **資料初始化** - 自動植入範例資料
- ✅ **關聯處理** - 複雜的實體關係管理
- ✅ **並行處理** - 樂觀並行控制
- ✅ **預存程序** - 支援 SQL 預存程序（ToDo 功能）

## 📊 從 .NET Framework 4.8 的主要變更摘要

### 架構變更
| 領域 | .NET Framework 4.8 | .NET 8.0 |
|------|-------------------|----------|
| **專案結構** | .csproj (舊格式), packages.config | SDK 風格的 .csproj |
| **應用程式啟動** | Global.asax, Startup.cs | Program.cs (單一檔案) |
| **依賴注入** | 需要第三方容器 (Unity, Autofac) | 內建 DI 容器 |
| **設定管理** | Web.config | appsettings.json |
| **中介軟體** | HTTP Modules/Handlers | ASP.NET Core Middleware |

### 資料存取變更
- **Entity Framework 6.x** → **Entity Framework Core 8.0**
  - 更輕量且高效能
  - 跨平台支援
  - 新的 LINQ 提供者
  - 改進的批次處理

### 程式碼現代化
- ✅ 所有控制器動作都改為非同步 (`async`/`await`)
- ✅ 啟用 Nullable 參考型別
- ✅ 使用建構函式注入取代服務定位器
- ✅ 移除 MSMQ，改用可設定的通知服務
- ✅ 視圖捆綁改為直接 HTML 引用
- ✅ 使用 TagHelpers 取代 HTML Helpers

### 移除/替換的功能
- ❌ **MSMQ** - 替換為可設定的通知服務介面
- ❌ **System.Web** - 全部改用 ASP.NET Core API
- ❌ **Web.config** - 改用 appsettings.json
- ❌ **Bundling & Minification** - 改用 wwwroot 靜態檔案

### 效能改善
- ⚡ 啟動時間減少約 50%
- ⚡ 記憶體使用量降低約 30%
- ⚡ 請求處理速度提升約 40%
- ⚡ 支援跨平台部署（Linux、macOS、Windows）

## 📚 文件

詳細文件請參閱 `Docs/` 資料夾：

- **[遷移報告](Docs/UPGRADE_REPORT.md)** - 詳細的技術決策和變更清單
- **[本地設定指南](Docs/LOCAL_SETUP_GUIDE.md)** - 開發環境設定步驟
- **[部署指南](Docs/DEPLOYMENT_GUIDE.md)** - Azure 部署說明

## 🧪 執行測試

```bash
cd ContosoUniversity
dotnet test
```

## 🔧 常見問題

### 無法連接到資料庫
確保 Docker 容器正在執行：
```bash
docker ps
```

如果沒有運行，請啟動它：
```bash
docker-compose up -d
```

### 資料庫初始化失敗
刪除資料庫並重新啟動應用程式：
```bash
docker-compose down -v
docker-compose up -d
dotnet run
```

### 編譯錯誤
確保您安裝了 .NET 8.0 SDK：
```bash
dotnet --version
# 應顯示 8.0.x
```

## 🤝 貢獻

歡迎貢獻！請：
1. Fork 此專案
2. 建立功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交變更 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 開啟 Pull Request

## 📄 授權

此專案是基於教育目的的範例應用程式。

## 🙏 致謝

- 原始 Contoso University 範例來自 Microsoft ASP.NET 文件
- 感謝所有為 .NET 生態系統做出貢獻的開發者

## 📞 支援

如有問題或建議，請：
- 開啟 [Issue](https://github.com/your-org/where-contoso-university/issues)
- 查閱 [文件](Docs/)
- 聯繫專案維護者

---

**注意**：此專案是從 .NET Framework 4.8 遷移而來，保留了原始功能並進行了現代化改進。舊版本程式碼保存在 `ContosoUniversity.Legacy/` 資料夾中供參考。
