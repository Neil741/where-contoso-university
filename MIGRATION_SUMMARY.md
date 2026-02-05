# .NET Framework 4.8 到 .NET 8.0 遷移摘要

## 遷移日期
2024年1月

## 專案概述
將 Contoso University 專案從 .NET Framework 4.8 (ASP.NET MVC 5) 遷移到 .NET 8.0 (ASP.NET Core MVC)

## 主要變更

### 1. 專案結構變更
- ✅ 移除 `App_Start` 資料夾 (BundleConfig, FilterConfig, RouteConfig)
- ✅ 移除 `Global.asax` 和 `Global.asax.cs`
- ✅ 移除 `Web.config` 和 `packages.config`
- ✅ 新增 `Program.cs` 作為應用程式進入點
- ✅ 新增 `appsettings.json` 和 `appsettings.Development.json` 用於配置
- ✅ 新增 `wwwroot` 資料夾存放靜態資源

### 2. Models 更新
- ✅ 所有 string 類型屬性添加 `required` 或 `?` nullable 標記
- ✅ 所有 navigation properties 使用 `= null!` 或標記為 nullable
- ✅ 所有集合屬性初始化為空集合 `= new List<T>()`
- ✅ ErrorViewModel 更新為 ASP.NET Core 標準格式

### 3. Data 層更新
- ✅ SchoolContext 已經是 EF Core 格式，無需修改
- ✅ DbInitializer 已經是 EF Core 格式，無需修改
- ✅ 移除 SchoolContextFactory (使用 DI 替代)
- ✅ ToDoStoredProcedures.sql 設置為自動複製到輸出目錄

### 4. Controllers 更新
- ✅ 所有 `using System.Web.Mvc` 改為 `using Microsoft.AspNetCore.Mvc`
- ✅ 所有 `HttpStatusCodeResult` 改為 `BadRequest()` 或 `NotFound()`
- ✅ 所有 `HttpNotFound()` 改為 `NotFound()`
- ✅ `TryUpdateModel` 改為 `TryUpdateModelAsync` (async/await 模式)
- ✅ BaseController 使用建構子注入替代手動實例化
- ✅ 所有 Controllers 使用建構子注入 `SchoolContext` 和 `NotificationService`
- ✅ 移除所有 `Dispose` 方法 (由 DI 容器管理)
- ✅ CoursesController 文件上傳從 `HttpPostedFileBase` 改為 `IFormFile`
- ✅ CoursesController 使用 `IWebHostEnvironment` 處理文件路徑
- ✅ 文件路徑從 `~/Uploads/` 改為 `/uploads/` (相對於 wwwroot)

### 5. Services 更新
- ✅ NotificationService 移除 System.Messaging (MSMQ) 依賴
- ✅ 使用 in-memory Queue<T> 實作通知隊列
- ✅ 添加 TODO 註解說明未來可整合 Azure Service Bus
- ✅ 移除 Dispose 方法

### 6. Views 更新
- ✅ 創建 `_ViewImports.cshtml` 添加全局 using 語句
- ✅ 移除所有 `@Scripts.Render()` 
- ✅ 移除所有 `@Styles.Render()`
- ✅ 改用直接的 `<script src="~/js/...">` 標籤
- ✅ 改用直接的 `<link rel="stylesheet" href="~/css/...">` 標籤
- ✅ Error.cshtml 從 `HandleErrorInfo` 改為 `ErrorViewModel`
- ✅ 移除 Views/Web.config (不再需要)

### 7. 配置檔案
- ✅ Program.cs 配置 DI、中間件、路由
- ✅ appsettings.json 添加 ConnectionStrings 和 NotificationQueuePath
- ✅ appsettings.Development.json 設置 SQL Server 連接字串 (for Codespace)

### 8. 靜態資源
- ✅ CSS 文件從 `Content/` 移至 `wwwroot/css/`
- ✅ JavaScript 文件從 `Scripts/` 移至 `wwwroot/js/`
- ✅ Uploads 資料夾移至 `wwwroot/uploads/`

### 9. 依賴套件
保持以下 NuGet 套件：
- Microsoft.EntityFrameworkCore 8.0.11
- Microsoft.EntityFrameworkCore.SqlServer 8.0.11
- Microsoft.EntityFrameworkCore.Tools 8.0.11
- Microsoft.Data.SqlClient 6.1.2
- Microsoft.Identity.Client 4.73.1
- Newtonsoft.Json 13.0.3

## 未來改進建議

1. **通知系統**: 將 in-memory queue 替換為 Azure Service Bus 或其他訊息佇列服務
2. **認證授權**: 實作 ASP.NET Core Identity 或 Azure AD 認證
3. **日誌記錄**: 整合結構化日誌框架如 Serilog
4. **API**: 考慮添加 Web API endpoints
5. **前端**: 考慮升級前端框架到 Bootstrap 5 或使用現代前端框架
6. **測試**: 添加單元測試和整合測試
7. **CI/CD**: 設置持續整合和部署流程

## Build 狀態
✅ 專案成功編譯，0 個錯誤，33 個警告（主要是 nullable 警告）

## 測試建議
1. 測試資料庫連接和初始化
2. 測試所有 CRUD 操作
3. 測試文件上傳功能
4. 測試通知系統
5. 測試儲存過程 (ToDo feature)
6. 測試並發更新 (Department optimistic concurrency)

## 參考資料
- [Migrate from ASP.NET MVC to ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/migration/mvc)
- [ASP.NET Core Dependency Injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
