# 規格→程式碼移轉

## 目標
.NET Framework 4.8 完整遷移至 .NET 8.0，採用 ASP.NET Core 架構，完成相容性修正、安全性更新，移除 Windows 特定依賴，啟用跨平台與雲端部署能力，並建立前後端獨立開發與部署流程。

---
## 資料夾結構
- 確保專案結構清晰，便於維護與擴展，禁止將舊有專案與更新後程式碼及專案混和於同一資料夾中。
- 建議資料夾結構如下：
```
/ContosoUniversity
│── /ContosoUniversity.Legacy       # 舊有 .NET Framework 4.8 專案
│── /ContosoUniversity              # 更新後 .NET 8.0 專案
│── /Docs                           # 文件與規格文件
│── /Scripts                        # 部署與自動化腳本
│── README.md                       # 專案說明文件
|── .gitignore
```
---

## 核心遷移範圍
### 專案升級
- .NET Framework 4.8 遷移至 .NET 8.0
- 轉換為 SDK-style csproj
- 更新所有 NuGet 套建至 .NET 8.0 相容版本

### Framework 遷移
- 以直接的 HTML 標籤取代原有的打包與壓縮機制（bundling / minification）
- 在 Program.cs 中使用 app.MapControllerRoute() 進行路由設定
- 啟用 ASP.NET Core MVC（Program.cs + Middleware）

---

## 架構與程式碼調整
- Global.asax / App_Start → Program.cs
- RouteCollection → app.MapControllerRoute
- GlobalFilter → Middleware
- Controllers 改用 Microsoft.AspNetCore.Mvc
- TryUpdateModel → TryUpdateModelAsync
- Views 移除 @Scripts.Render / @Styles.Render，改用直接 HTML 標籤
- 設定移至 appsettings.json，移除 Web.config
- 保留使用引用本地的 jQuery validation 檔案方式，務必確保新舊專案中的引用檔案並未遺漏
- todo 功能使用到預存程序，在 DbInitializer 在啟動時自動載入預存程序，確保 SQL 腳本會被複製到輸出目錄，讓執行時能讀到

---

## 套件與安全性更新
- EF Core 3.1 → 8.x
- Microsoft.Data.SqlClient 2.1.4 → 6.1.2（修補 CVE）
- Microsoft.Identity.Client 升級至最新版
- Microsoft.Extensions.* 對齊 .NET 8

---

## 服務與相依性注入
- 全面使用 Dependency Injection
- NotificationService 暫停 MSMQ (System.Messaging 不支援 .NET 8)，預留未來整合 Azure Service Bus 介面
- 生產環境需支援 HTTPS，預留整合 Azure Key Vault 作為機敏資訊存放
- Azure 部署需求
- 可部署至 Azure App Service（Linux）
- 提供 ARM Template 建立基礎資源
- 提供 PowerShell 一鍵部署腳本
- 提供 GitHub Actions CI/CD（使用 Publish Profile）

---

## 允許本地測試
- 可透過 codespace 進行本地端測試
- 本地端測試時，採用 devlopment 模式運行，使用 container 方式執行 SQL Server 並連線進行測試
- 建立 `appsettings.Development.json` 供本地測試使用，包含 code space 中容器運行之 SQL Server 連線字串
- 終端機於 codespace 中執行以下指令啟動應用程式：
  ```bash
  export ASPNETCORE_ENVIRONMENT=Development dotnet run
  ```

---
## .NET 8 Nullable 常見問題預防
為避免 .NET 8 中 Nullable 參考型別相關警告與錯誤，請在移轉過程中遵循以下規則：

### 1) 避免將 null 指派給非 Nullable 型別
**問題描述**：出現「Converting null literal or possible null value to non-nullable type」警告。

**預防策略**：
- 若屬性或變數允許為空，請明確標註為 Nullable（例如 `string?`）。
- 若邏輯上必須非空，請在建構子、初始化或資料載入流程中確保值一定被指派。
- 避免使用 `null` 當作預設值給非 Nullable 型別。

### 2) 建構子退出時非 Nullable 屬性不可為 null
**問題描述**：出現「on-nullable property 'Name' must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring the property as nullable」警告。

**預防策略**：
- 若屬性必填，改用 `required` 修飾詞並在建立物件時提供值。
- 若屬性可空，將型別改為 Nullable（例如 `string? Name`）。
- 在建構子中為非 Nullable 屬性提供預設值或明確指派。

### 3) 遷移時的落地原則
- Entity / ViewModel / DTO 先行盤點欄位是否允許空值，統一標示 Nullable 狀態。
- 若模型為必填欄位，請使用 `required` 搭配資料驗證（如 DataAnnotations）。
- 查詢與對應資料時，需特別處理可能為空的資料來源，避免在指派時產生警告。

---

## 驗證與完成條件
- 本地測試運行正常
- 務必通過以下驗證條件：
  - Create / Read / Update / Delete 功能在 students / courses / instructors / departments / todo 頁面均正常運作(使用者進入 notification 中點選 create new student，輸入完資料可以成功建立學生資料，並可在 students 頁面看到該學生資料)
- dotnet build -c Release 成功
- Azure App Service 成功啟動並可連線資料庫
- GitHub Action CI/CD 可自動部署

---

##  文件說明 
- `README.md`: 說明目前專案的架構及技術採用以供後續維護使用
- `UPGRADE_REPORT.md`: 說明更版的細節及內容，包括技術決策、變更內容與效能比較
- `LOCAL_SETUP_GUIDE.md`: 說明如何在 codespace 中順利將該正常運行於 localhost 測試
- `DEPLOYMENT_GUIDE.md`: 說明 Azure 部署流程、PowerShell 腳本、CI/CD