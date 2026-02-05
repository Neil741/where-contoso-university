# 技術架構規格

## 概述

本文件詳細描述 Contoso University 系統的技術架構，包括技術棧、基礎設施依賴、系統需求和配置管理。

---

## 1. 技術棧

### 1.1 核心框架

#### ASP.NET MVC 5

**版本**: 5.2.9

**用途**: Web 應用程式框架

**關鍵特性**:
- Model-View-Controller 模式
- Razor 視圖引擎
- 路由系統
- 模型綁定和驗證
- 過濾器和屬性路由

**主要組件**:
- `System.Web.Mvc` - MVC 核心功能
- `System.Web.Optimization` - 資源捆綁和壓縮
- `System.Web.Razor` - Razor 視圖引擎

#### .NET Framework

**版本**: 4.8.2

**目標框架**: `net482`

**運行時需求**: 
- Windows 平台
- .NET Framework 4.8.2 運行時

**關鍵命名空間**:
- `System.Web` - Web 應用程式基礎
- `System.Data` - 資料訪問
- `System.Configuration` - 配置管理
- `System.Messaging` - MSMQ 支援

### 1.2 資料訪問層

#### Entity Framework Core

**版本**: 3.1.32

**提供者**: Microsoft.EntityFrameworkCore.SqlServer

**用途**: 
- ORM (物件關係映射)
- Code-First 開發
- LINQ 查詢
- 變更追蹤
- 遷移管理

**關鍵組件**:
```xml
<package id="Microsoft.EntityFrameworkCore" version="3.1.32" />
<package id="Microsoft.EntityFrameworkCore.SqlServer" version="3.1.32" />
<package id="Microsoft.EntityFrameworkCore.Design" version="3.1.32" />
<package id="Microsoft.EntityFrameworkCore.Tools" version="3.1.32" />
```

**DbContext**: `SchoolContext`

**遷移策略**: Code-First with Migrations

#### ADO.NET

**版本**: .NET Framework 內建

**用途**: 
- 直接資料庫訪問（ToDo 功能）
- 存儲程序調用
- 高性能資料訪問

**關鍵類別**:
- `SqlConnection` - 資料庫連接
- `SqlCommand` - 命令執行
- `SqlDataReader` - 資料讀取
- `SqlParameter` - 參數化查詢

#### Microsoft.Data.SqlClient

**版本**: 2.1.4

**用途**: SQL Server 資料提供者

### 1.3 前端技術

#### Bootstrap

**版本**: 5.3.3

**用途**: 響應式 UI 框架

**功能**:
- 網格系統
- 表單元件
- 按鈕樣式
- 導航列
- 模態對話框
- 提示訊息

**CDN 引用**:
```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
```

#### jQuery

**版本**: 3.7.1

**用途**: 
- DOM 操作
- AJAX 請求
- 事件處理
- 通知輪詢

**相關套件**:
```xml
<package id="jQuery" version="3.7.1" />
<package id="jQuery.Validation" version="1.21.0" />
<package id="Microsoft.jQuery.Unobtrusive.Validation" version="4.0.0" />
```

#### Razor 引擎

**版本**: MVC 5 內建

**用途**: 伺服器端模板引擎

**特性**:
- C# 與 HTML 混合語法
- 強型別視圖模型
- 布局頁面
- 部分視圖
- HTML Helper

### 1.4 依賴注入與配置

#### Microsoft.Extensions.*

**版本**: 3.1.32

**套件**:
```xml
<package id="Microsoft.Extensions.Configuration" version="3.1.32" />
<package id="Microsoft.Extensions.Configuration.Json" version="3.1.32" />
<package id="Microsoft.Extensions.DependencyInjection" version="3.1.32" />
<package id="Microsoft.Extensions.Logging" version="3.1.32" />
<package id="Microsoft.Extensions.Logging.Console" version="3.1.32" />
```

**用途**:
- 配置管理
- 依賴注入容器
- 日誌記錄
- 選項模式

### 1.5 序列化與資料處理

#### Newtonsoft.Json

**版本**: 13.0.3

**用途**: 
- JSON 序列化/反序列化
- AJAX 回應格式化
- MSMQ 訊息序列化

**主要功能**:
- `JsonConvert.SerializeObject()`
- `JsonConvert.DeserializeObject<T>()`
- 自訂序列化設定

---

## 2. 基礎設施依賴

### 2.1 資料庫系統

#### SQL Server LocalDB

**版本**: 與 Visual Studio 捆綁

**用途**: 開發環境資料庫

**特性**:
- 輕量級 SQL Server
- 檔案基礎
- 無需安裝服務
- 適用於開發和測試

**連接字串**:
```
Server=(localdb)\\mssqllocaldb;Database=ContosoUniversityNoAuthEFCore;Trusted_Connection=True;MultipleActiveResultSets=true
```

**生產環境**: 
- SQL Server Express
- SQL Server Standard/Enterprise
- Azure SQL Database

#### 資料庫初始化

**策略**: Code-First with DbInitializer

**初始化類別**: `DbInitializer`

**初始化時機**: 應用程式啟動時（Global.asax）

### 2.2 訊息佇列

#### Microsoft Message Queuing (MSMQ)

**版本**: Windows 內建功能

**用途**: 
- 非同步通知系統
- 訊息解耦
- 可靠訊息傳遞

**佇列配置**:
```
路徑: .\private$\ContosoUniversityNotifications
類型: 私有佇列 (Private Queue)
格式: JSON
```

**系統需求**:
- Windows OS
- MSMQ Windows 功能已啟用

**啟用方法**:
1. 控制台 → 程式和功能 → 開啟或關閉 Windows 功能
2. 勾選 "Microsoft Message Queue (MSMQ) Server"
3. 勾選 "MSMQ Server Core"
4. 重新啟動電腦

**生產環境替代方案**:
- Azure Service Bus
- RabbitMQ
- Apache Kafka

### 2.3 檔案系統

#### 上傳目錄

**路徑**: `/Uploads/TeachingMaterials/`

**用途**: 儲存課程教材圖片

**權限需求**:
- IIS 應用程式池身分需要讀寫權限
- 建議設定 IIS_IUSRS 群組權限

**備份策略**:
- 定期備份上傳目錄
- 考慮使用雲端儲存（Azure Blob Storage）

---

## 3. 系統需求

### 3.1 開發環境需求

#### 作業系統

- **Windows 10** (1809 或更高版本)
- **Windows 11**
- **Windows Server 2019/2022**

#### 開發工具

**Visual Studio**:
- **版本**: Visual Studio 2019 或更高版本
- **工作負載**: 
  - ASP.NET 和 Web 開發
  - .NET 桌面開發

**替代方案**:
- Visual Studio Code (需額外配置)
- JetBrains Rider

#### .NET SDK

- **.NET Framework 4.8.2 Developer Pack**
- **.NET Core SDK 3.1** (用於 EF Core 工具)

#### 資料庫工具

- **SQL Server Management Studio (SSMS)** - 選用，用於資料庫管理
- **Azure Data Studio** - 選用

#### 版本控制

- **Git** 2.x 或更高版本

### 3.2 運行時需求

#### 伺服器環境

**Windows Server**:
- Windows Server 2016 或更高版本
- .NET Framework 4.8.2 運行時
- IIS 10.0 或更高版本
- MSMQ 功能已啟用

**桌面環境** (測試用):
- Windows 10/11
- .NET Framework 4.8.2 運行時
- IIS Express (Visual Studio 內建)
- MSMQ 功能已啟用

#### IIS 配置

**應用程式池**:
- **.NET CLR 版本**: .NET CLR Version v4.0.30319
- **管道模式**: 整合 (Integrated)
- **身分識別**: ApplicationPoolIdentity

**網站設定**:
- **繫結**: HTTP (預設 80 埠)
- **實體路徑**: 應用程式根目錄
- **應用程式**: 配置為應用程式

**權限**:
- 應用程式池身分對網站目錄有讀取權限
- 應用程式池身分對 `/Uploads/` 目錄有讀寫權限

#### 資料庫需求

**SQL Server**:
- SQL Server 2016 或更高版本
- SQL Server Express (最小需求)
- LocalDB (開發環境)

**連接需求**:
- TCP/IP 協定已啟用
- SQL Server 瀏覽器服務運行中（如果使用具名實例）
- 防火牆允許 SQL Server 連接

### 3.3 硬體需求

#### 最低需求

- **CPU**: 雙核心 1.8 GHz
- **記憶體**: 4 GB RAM
- **磁碟空間**: 10 GB 可用空間
- **網路**: 100 Mbps

#### 建議需求

- **CPU**: 四核心 2.5 GHz 或更高
- **記憶體**: 8 GB RAM 或更高
- **磁碟空間**: 50 GB SSD
- **網路**: 1 Gbps

---

## 4. 配置管理

### 4.1 Web.config 配置

#### 連接字串

```xml
<connectionStrings>
    <add name="ContosoUniversityNoAuthEFCore" 
         connectionString="Server=(localdb)\mssqllocaldb;Database=ContosoUniversityNoAuthEFCore;Trusted_Connection=True;MultipleActiveResultSets=true" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

**配置說明**:
- `Server`: 資料庫伺服器位址
- `Database`: 資料庫名稱
- `Trusted_Connection`: 使用 Windows 驗證
- `MultipleActiveResultSets`: 允許多個活動結果集

**環境差異**:
- 開發: LocalDB
- 測試: SQL Server Express
- 生產: SQL Server / Azure SQL Database

#### 應用程式設定

```xml
<appSettings>
    <add key="NotificationQueuePath" value=".\private$\ContosoUniversityNotifications" />
    <add key="webpages:Version" value="3.0.0.0" />
    <add key="webpages:Enabled" value="false" />
    <add key="ClientValidationEnabled" value="true" />
    <add key="UnobtrusiveJavaScriptEnabled" value="true" />
</appSettings>
```

**關鍵設定**:
- `NotificationQueuePath`: MSMQ 佇列路徑
- `ClientValidationEnabled`: 啟用客戶端驗證
- `UnobtrusiveJavaScriptEnabled`: 啟用非侵入式 JavaScript

#### 系統設定

##### HTTP 運行時

```xml
<system.web>
    <httpRuntime maxRequestLength="10240" 
                 executionTimeout="300" 
                 targetFramework="4.8.2" />
</system.web>
```

**設定說明**:
- `maxRequestLength`: 最大請求大小 (KB) - 10MB
- `executionTimeout`: 請求執行超時 (秒)
- `targetFramework`: 目標框架版本

##### 請求過濾

```xml
<system.webServer>
    <security>
        <requestFiltering>
            <requestLimits maxAllowedContentLength="10485760" /> <!-- 10MB -->
        </requestFiltering>
    </security>
</system.webServer>
```

**設定說明**:
- `maxAllowedContentLength`: 最大內容長度 (bytes) - 10MB

##### 編譯設定

```xml
<system.web>
    <compilation debug="true" targetFramework="4.8.2" />
</system.web>
```

**注意**:
- 開發環境: `debug="true"`
- 生產環境: `debug="false"` (效能優化)

##### 錯誤處理

```xml
<system.web>
    <customErrors mode="RemoteOnly" defaultRedirect="~/Error">
        <error statusCode="404" redirect="~/Error/NotFound" />
        <error statusCode="500" redirect="~/Error/ServerError" />
    </customErrors>
</system.web>
```

**模式**:
- `Off`: 顯示詳細錯誤（僅開發）
- `RemoteOnly`: 遠端使用者看到自訂錯誤頁面
- `On`: 所有使用者看到自訂錯誤頁面

### 4.2 路由配置

**位置**: `App_Start/RouteConfig.cs`

```csharp
public class RouteConfig
{
    public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

        routes.MapRoute(
            name: "Default",
            url: "{controller}/{action}/{id}",
            defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
        );
    }
}
```

**預設路由**:
- 控制器: Home
- 動作: Index
- ID: 選用參數

### 4.3 捆綁配置

**位置**: `App_Start/BundleConfig.cs`

```csharp
public class BundleConfig
{
    public static void RegisterBundles(BundleCollection bundles)
    {
        // jQuery
        bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                    "~/Scripts/jquery-{version}.js"));

        // jQuery Validation
        bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                    "~/Scripts/jquery.validate*"));

        // Bootstrap
        bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                    "~/Scripts/bootstrap.js"));

        bundles.Add(new StyleBundle("~/Content/css").Include(
                    "~/Content/bootstrap.css",
                    "~/Content/site.css"));
    }
}
```

**優化**:
- 生產環境: 自動壓縮和合併
- 開發環境: 使用未壓縮版本

### 4.4 過濾器配置

**位置**: `App_Start/FilterConfig.cs`

```csharp
public class FilterConfig
{
    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
        filters.Add(new HandleErrorAttribute());
    }
}
```

**全域過濾器**:
- HandleErrorAttribute: 全域錯誤處理

---

## 5. 部署配置

### 5.1 發佈設定

**發佈方法**:
1. **檔案系統發佈** - 發佈到本地資料夾
2. **Web Deploy** - 直接部署到 IIS
3. **FTP** - 透過 FTP 上傳

**發佈配置檔**:
- `Properties/PublishProfiles/*.pubxml`

**轉換設定**:
- `Web.Debug.config` - 調試轉換
- `Web.Release.config` - 發佈轉換

### 5.2 Web.Release.config 範例

```xml
<configuration xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">
  <system.web>
    <compilation xdt:Transform="RemoveAttributes(debug)" />
  </system.web>
  
  <connectionStrings>
    <add name="ContosoUniversityNoAuthEFCore" 
         connectionString="Server=production-server;Database=ContosoUniversity;User Id=app_user;Password=***;MultipleActiveResultSets=true" 
         xdt:Transform="SetAttributes" xdt:Locator="Match(name)"/>
  </connectionStrings>
  
  <appSettings>
    <add key="NotificationQueuePath" value="FormatName:DIRECT=OS:production-server\private$\ContosoNotifications" 
         xdt:Transform="SetAttributes" xdt:Locator="Match(key)"/>
  </appSettings>
</configuration>
```

### 5.3 環境變數

**支援的環境變數** (選用):
- `ASPNETCORE_ENVIRONMENT` - 環境名稱
- `ConnectionStrings:DefaultConnection` - 連接字串覆蓋

---

## 6. 監控與日誌

### 6.1 日誌服務

**位置**: `Services/LoggingService.cs`

**功能**:
- 錯誤日誌記錄
- 資訊日誌記錄
- 警告日誌記錄

**實作**:
```csharp
public static class LoggingService
{
    public static void LogError(string message)
    {
        // 實作日誌記錄邏輯
        System.Diagnostics.Trace.TraceError(message);
    }
    
    public static void LogInfo(string message)
    {
        System.Diagnostics.Trace.TraceInformation(message);
    }
}
```

**擴展選項**:
- Log4Net
- NLog
- Serilog
- Application Insights (Azure)

### 6.2 效能監控

**IIS 效能計數器**:
- ASP.NET 應用程式請求數
- 平均請求執行時間
- 錯誤率

**SQL Server 監控**:
- 查詢執行時間
- 連接池使用情況
- 鎖定和死鎖

**自訂監控**:
- MSMQ 佇列深度
- 檔案上傳成功率

---

## 7. 安全配置

### 7.1 驗證與授權

**驗證模式**: Windows Authentication / Forms Authentication

**角色**:
- Admin - 完整權限
- Teacher - 教材上傳權限

**授權屬性**:
```csharp
[Authorize(Roles = "Admin")]
public ActionResult AdminOnly() { }

[Authorize(Roles = "Admin,Teacher")]
public ActionResult TeacherAccess() { }
```

### 7.2 資料保護

**SQL 注入防護**:
- 使用 Entity Framework 參數化查詢
- 存儲程序使用 SqlParameter

**XSS 防護**:
- Razor 自動編碼輸出
- 使用 `@Html.Raw()` 時需謹慎

**CSRF 防護**:
- `@Html.AntiForgeryToken()` 在表單中
- `[ValidateAntiForgeryToken]` 在 POST 動作

### 7.3 連接字串保護

**開發環境**: Web.config

**生產環境**: 
- Windows 配置檔加密
- Azure Key Vault
- 環境變數

**加密命令**:
```bash
aspnet_regiis -pef "connectionStrings" "C:\path\to\web\root" -prov "DataProtectionConfigurationProvider"
```

---

## 8. 可擴展性考量

### 8.1 水平擴展

**挑戰**:
- MSMQ 單機限制
- 本地檔案系統儲存

**解決方案**:
- 遷移到 Azure Service Bus
- 使用 Azure Blob Storage

### 8.2 垂直擴展

**優化點**:
- 增加 IIS 應用程式池工作執行緒
- 增加 SQL Server 記憶體
- 使用 Redis 進行快取

### 8.3 效能優化

**資料庫**:
- 索引優化
- 查詢優化
- 連接池配置

**應用程式**:
- 輸出快取
- 靜態資源 CDN
- 非同步控制器動作

---

## 9. 雲端遷移路徑

### 9.1 Azure 架構建議

**計算**:
- Azure App Service / Azure Container Apps

**資料庫**:
- Azure SQL Database

**訊息**:
- Azure Service Bus

**儲存**:
- Azure Blob Storage

**監控**:
- Azure Application Insights

### 9.2 容器化

**Dockerfile 範例** (需要遷移到 .NET Core):
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ContosoUniversity.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContosoUniversity.dll"]
```

---

## 10. 已知限制

### 10.1 平台限制

- 僅支援 Windows (MSMQ 依賴)
- .NET Framework (非跨平台)

### 10.2 擴展限制

- MSMQ 單機部署
- 本地檔案系統儲存
- 單體應用架構

### 10.3 功能限制

- 無即時通知（使用輪詢）
- 檔案上傳僅支援圖片
- 無多租戶支援

---

## 參考文件

- [系統架構概覽](./01-system-architecture-overview.md)
- [資料庫規格](./06-database-specification.md)
- [特殊功能規格](./04-special-features-specification.md)

## 外部參考

- [ASP.NET MVC 5 文件](https://docs.microsoft.com/aspnet/mvc/)
- [Entity Framework Core 文件](https://docs.microsoft.com/ef/core/)
- [MSMQ 文件](https://docs.microsoft.com/windows/win32/msmq/microsoft-message-queuing)
- [IIS 配置](https://docs.microsoft.com/iis/)
