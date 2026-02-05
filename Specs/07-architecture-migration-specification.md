# 架構拆分與升級規格

## 概述

本文件定義從 ASP.NET MVC 5 (.NET Framework 4.8) 單體架構遷移到前後端分離的現代化架構的完整規格，包含相容性評估、架構拆分策略、以及遷移風險與替代方案。

---

## 1. 架構遷移目標

### 1.1 目標架構

**前端架構**:
- **框架**: React 18+ with TypeScript
- **建置工具**: Vite 5+
- **UI 框架**: 
  - Material-UI (MUI) 或 Ant Design（建議 MUI，與 Azure 生態系統整合較佳）
  - 保留 Bootstrap 樣式邏輯以降低視覺遷移成本
- **狀態管理**: 
  - React Query / TanStack Query（伺服器狀態）
  - Zustand 或 Context API（客戶端狀態）
- **路由**: React Router v6+
- **HTTP Client**: Axios 或 Fetch API
- **表單處理**: React Hook Form + Zod（型別安全驗證）

**後端架構**:
- **框架**: ASP.NET Core 8.0 Web API
- **資料訪問**: Entity Framework Core 8.0
- **API 規範**: RESTful API with OpenAPI/Swagger
- **認證授權**: 
  - ASP.NET Core Identity（未來擴充）
  - JWT Bearer Token 認證
- **依賴注入**: ASP.NET Core 內建 DI 容器

**資料庫**:
- **服務**: Azure SQL Database
- **連線庫**: Microsoft.Data.SqlClient (最新安全版本)
- **遷移工具**: EF Core Migrations

**部署環境**:
- **前端**: Azure Static Web Apps 或 Azure App Service (Linux)
- **後端**: Azure App Service (Linux, .NET 8 Runtime)
- **CI/CD**: GitHub Actions
- **儲存**: Azure Blob Storage（教材上傳）
- **訊息佇列**: Azure Service Bus（取代 MSMQ）

### 1.2 遷移原則

1. **漸進式遷移**: 優先建立 API 層，前端可分模組逐步遷移
2. **向下相容**: 初期保留 MVC Views，逐步以 React SPA 取代
3. **最小變動**: 資料模型結構保持一致，減少資料庫 schema 變更
4. **雲端優先**: 所有新元件優先考慮 Azure 服務
5. **安全性優先**: 所有套件需通過安全性掃描，無已知高危漏洞

---

## 2. 相容性評估

### 2.1 System.Web 依賴盤點與替代方案

#### 2.1.1 高風險依賴（需完全替換）

| 遺留依賴 | 使用位置 | .NET 8 替代方案 | 遷移難度 |
|---------|---------|----------------|---------|
| `System.Web.Mvc` | 所有 Controllers, Views | ASP.NET Core MVC / Web API | **高** - 需重寫所有 Controllers |
| `System.Web.Optimization` | BundleConfig.cs | Vite 建置（前端）、無需後端 bundling | **中** - 前端工具鏈處理 |
| `System.Web.Routing` | RouteConfig.cs | ASP.NET Core Routing | **低** - 相似概念 |
| `System.Messaging` (MSMQ) | NotificationService.cs | Azure Service Bus | **高** - 完全不同的 API |
| `HttpContext.Current` | BaseController.cs, 可能的輔助方法 | `IHttpContextAccessor` | **中** - DI 模式變更 |
| `HttpServerUtility` | 檔案上傳相關 | `IWebHostEnvironment`, Azure Blob | **中** - 需重構檔案處理 |

#### 2.1.2 中風險依賴（需調整但有對應方案）

| 遺留依賴 | 使用位置 | .NET 8 替代方案 | 遷移難度 |
|---------|---------|----------------|---------|
| `System.Configuration.ConfigurationManager` | Global.asax.cs, Services | `IConfiguration` (appsettings.json) | **低** - 直接對應 |
| `System.Web.Security.FormsAuthentication` | （如有使用）| ASP.NET Core Identity + JWT | **中** - 認證機制重構 |
| `Session State` | （如有使用）| Redis Cache / JWT Claims | **中** - 無狀態設計 |

#### 2.1.3 低風險依賴（.NET 8 原生支援）

| 遺留依賴 | .NET 8 狀態 | 備註 |
|---------|------------|------|
| `System.ComponentModel.DataAnnotations` | ✅ 完全支援 | 驗證特性可直接使用 |
| `System.Linq` | ✅ 完全支援 | LINQ 語法一致 |
| `System.Collections.Generic` | ✅ 完全支援 | 無需變更 |
| `Newtonsoft.Json` | ✅ 可繼續使用 | 建議遷移到 `System.Text.Json` |

### 2.2 Web.config 移轉需求

#### 2.2.1 配置項目對應表

| Web.config 項目 | appsettings.json 對應 | 說明 |
|----------------|----------------------|------|
| `<connectionStrings>` | `"ConnectionStrings": { "DefaultConnection": "..." }` | 連線字串格式一致 |
| `<appSettings>` → NotificationQueuePath | `"AzureServiceBus": { "ConnectionString": "...", "QueueName": "..." }` | 改用 Azure Service Bus |
| `<compilation debug="true">` | 環境變數 `ASPNETCORE_ENVIRONMENT` | Development/Production |
| `<httpRuntime maxRequestLength>` | `Kestrel:Limits:MaxRequestBodySize` | 上傳大小限制 |
| `<authentication mode="Windows">` | （移除）| 改用 JWT 或 Azure AD |
| `<customErrors>` | Middleware（ExceptionHandler） | 統一例外處理 |

#### 2.2.2 appsettings.json 結構建議

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:{server}.database.windows.net,1433;Database={db};User ID={user};Password={password};Encrypt=True;TrustServerCertificate=False;"
  },
  "AzureServiceBus": {
    "ConnectionString": "Endpoint=sb://{namespace}.servicebus.windows.net/;SharedAccessKeyName=...;SharedAccessKey=...",
    "NotificationQueueName": "contoso-notifications"
  },
  "AzureBlobStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...",
    "ContainerName": "teaching-materials"
  },
  "FileUpload": {
    "MaxSizeInMB": 10,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".pdf"],
    "UseAzureBlob": true
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "https://{your-frontend}.azurestaticapps.net"
    ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### 2.2.3 環境特定配置

- `appsettings.Development.json`: 本地開發（LocalDB, Azurite）
- `appsettings.Staging.json`: 測試環境
- `appsettings.Production.json`: 生產環境（僅非敏感配置）
- **敏感配置**: Azure Key Vault 或 GitHub Secrets

---

## 3. 前後端拆分策略

### 3.1 UI 與業務邏輯耦合點分析

#### 3.1.1 強耦合點（需解耦）

| 模組 | 當前耦合情況 | 解耦策略 | API 端點設計 |
|-----|------------|---------|-------------|
| **學生管理** | Controller 直接返回 Razor View + Model | 分離為 RESTful API + React 元件 | `GET /api/students`, `POST /api/students`, `PUT /api/students/{id}`, `DELETE /api/students/{id}` |
| **課程管理** | 教材上傳混合 Server-Side 檔案處理 | 前端上傳 → API 接收 → Azure Blob | `POST /api/courses/{id}/materials` (multipart/form-data) |
| **分頁與搜尋** | Server-Side 生成 HTML | API 返回 JSON（分頁 metadata） | `GET /api/students?page=1&pageSize=10&search=...` |
| **表單驗證** | Server + Client 雙重驗證（Unobtrusive JS） | API 驗證 + React Hook Form 客戶端驗證 | 統一錯誤格式 `{ "errors": { "field": ["message"] } }` |
| **通知系統** | MSMQ + Polling | SignalR (WebSocket) + Azure Service Bus | Hub: `/notificationHub`, API: `GET /api/notifications` |

#### 3.1.2 中度耦合點（需重構）

| 模組 | 當前耦合情況 | 解耦策略 |
|-----|------------|---------|
| **待辦事項** | 混合使用 Stored Procedure 和 EF | 統一為 EF Core Repository Pattern |
| **統計報表** | Razor View 直接呈現圖表 | API 返回統計數據 + Chart.js/Recharts 前端繪圖 |
| **錯誤處理** | 自訂錯誤頁面 | API 返回標準化錯誤 JSON + React Error Boundary |

### 3.2 API 化可行性評估

#### 3.2.1 API 設計原則

1. **RESTful 風格**: 使用標準 HTTP 方法（GET, POST, PUT, DELETE, PATCH）
2. **資源導向**: URL 代表資源，避免動詞（`/api/students` ✅, `/api/getStudents` ❌）
3. **版本控制**: URL 版本化（`/api/v1/...`）或 Header 版本化
4. **統一回應格式**:
   ```json
   {
     "success": true,
     "data": { ... },
     "message": "操作成功",
     "errors": null
   }
   ```
5. **HATEOAS（可選）**: 回應包含相關資源連結

#### 3.2.2 現有功能 API 化影響範圍

| 功能模組 | API 端點數量估計 | 複雜度 | 相依服務 | 影響範圍評估 |
|---------|----------------|--------|---------|------------|
| 學生管理 | 7-10 個 | 中 | EF Core, 分頁, 搜尋 | **中** - 標準 CRUD |
| 課程管理 | 8-12 個 | 高 | EF Core, Azure Blob, 檔案驗證 | **高** - 檔案上傳複雜 |
| 講師管理 | 8-10 個 | 中 | EF Core, 多對多關係 | **中** - 關聯數據處理 |
| 部門管理 | 6-8 個 | 中 | EF Core, 並行控制 | **中** - 需實作樂觀鎖 |
| 通知系統 | 4-6 個 | 高 | Azure Service Bus, SignalR | **高** - 實時推送機制 |
| 待辦事項 | 5-7 個 | 低 | EF Core（移除 SP） | **低** - 簡單 CRUD |
| 統計報表 | 2-3 個 | 低 | EF Core, LINQ 聚合 | **低** - 唯讀查詢 |

#### 3.2.3 API 端點規劃（範例：學生管理）

```
# 學生資源
GET    /api/v1/students              # 列表（支援分頁、搜尋、排序）
GET    /api/v1/students/{id}         # 詳情
POST   /api/v1/students              # 建立
PUT    /api/v1/students/{id}         # 完整更新
PATCH  /api/v1/students/{id}         # 部分更新
DELETE /api/v1/students/{id}         # 刪除

# 學生關聯資源
GET    /api/v1/students/{id}/enrollments  # 學生的註冊課程
POST   /api/v1/students/{id}/enrollments  # 註冊新課程
DELETE /api/v1/students/{studentId}/enrollments/{enrollmentId}  # 退選課程
```

### 3.3 資料傳輸物件 (DTO) 設計

#### 3.3.1 為什麼需要 DTO？

- **安全性**: 避免過度曝露實體屬性（如 RowVersion, 內部 ID）
- **效能**: 減少不必要的數據傳輸（避免循環參照）
- **解耦**: API 合約與資料庫 Schema 獨立演化
- **驗證**: 針對不同操作定制驗證規則

#### 3.3.2 DTO 命名與分類

| DTO 類型 | 命名規範 | 範例 | 用途 |
|---------|---------|------|------|
| 讀取 DTO | `{Entity}Dto` | `StudentDto` | API 回應、列表 |
| 建立 DTO | `Create{Entity}Dto` | `CreateStudentDto` | POST 請求 |
| 更新 DTO | `Update{Entity}Dto` | `UpdateStudentDto` | PUT/PATCH 請求 |
| 摘要 DTO | `{Entity}SummaryDto` | `StudentSummaryDto` | 列表項目（簡化版） |

#### 3.3.3 DTO 範例（學生模組）

```csharp
// 讀取 DTO
public class StudentDto
{
    public int Id { get; set; }
    public string LastName { get; set; }
    public string FirstMidName { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public List<EnrollmentDto> Enrollments { get; set; }
}

// 建立 DTO
public class CreateStudentDto
{
    [Required]
    [StringLength(50)]
    public string LastName { get; set; }
    
    [Required]
    [StringLength(50)]
    public string FirstMidName { get; set; }
    
    [Required]
    public DateTime EnrollmentDate { get; set; }
}

// 更新 DTO
public class UpdateStudentDto
{
    [Required]
    [StringLength(50)]
    public string LastName { get; set; }
    
    [Required]
    [StringLength(50)]
    public string FirstMidName { get; set; }
    
    [Required]
    public DateTime EnrollmentDate { get; set; }
}

// 列表摘要 DTO
public class StudentSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } // 組合欄位
    public DateTime EnrollmentDate { get; set; }
    public int EnrollmentCount { get; set; } // 計算欄位
}
```

---

## 4. 風險與替代方案

### 4.1 套件版本風險

#### 4.1.1 Entity Framework Core 遷移風險

| 風險項目 | 當前版本 | 目標版本 | 風險等級 | 緩解措施 |
|---------|---------|---------|---------|---------|
| EF Core 版本跳躍 | 3.1.32 (EOL) | 8.0.x | **高** | 階段式升級：3.1 → 6.0 → 8.0，每階段完整測試 |
| 變更追蹤行為差異 | N/A | 8.0 更嚴格 | **中** | 複查所有 `.AsNoTracking()` 使用 |
| SQL 生成差異 | N/A | 可能不同 | **低** | 開啟 SQL 日誌比對，效能測試 |
| 遷移相容性 | Code-First | 可能需調整 | **中** | 備份資料庫，測試環境先行 |

#### 4.1.2 前端套件選型風險

| 套件 | 風險 | 替代方案 | 建議 |
|-----|------|---------|------|
| React Router | v6+ 重大變更 | TanStack Router | 優先 React Router（社群最大） |
| Material-UI | 版本更新頻繁 | Ant Design, Chakra UI | MUI（Azure 生態整合佳）|
| React Query | 品牌改為 TanStack Query | SWR, RTK Query | TanStack Query（最佳實踐） |
| Axios | Fetch API 已成熟 | Fetch API | Axios（攔截器、取消請求更方便）|

### 4.2 MSMQ 替代方案詳細評估

#### 4.2.1 Azure Service Bus 對應方案

| MSMQ 特性 | Azure Service Bus 對應 | 遷移建議 |
|----------|----------------------|---------|
| 本地私有佇列 `.\Private$\...` | Service Bus Queue | 使用命名佇列 `contoso-notifications` |
| `MessageQueue.Send()` | `ServiceBusSender.SendMessageAsync()` | 非同步 API |
| `MessageQueue.Receive()` | `ServiceBusReceiver.ReceiveMessageAsync()` | 支援 Peek-Lock 模式 |
| 交易支援 | Session-based Queues | 需額外配置 Session |
| 死信佇列 | 自動支援 | 無需手動處理 |
| 訊息優先級 | 不支援 | 使用多個佇列模擬 |

#### 4.2.2 替代方案比較

| 方案 | 優點 | 缺點 | 適用場景 | 成本估算 |
|-----|------|------|---------|---------|
| **Azure Service Bus** | 雲端原生、高可用、自動擴展 | 成本較高、學習曲線 | **推薦** - 生產環境 | 基礎層 ~$0.05/月 + 訊息費用 |
| **Azure Storage Queues** | 成本低、簡單 | 功能較少、無進階路由 | 簡單非同步工作 | ~$0.0036/10萬次操作 |
| **SignalR (僅推送)** | 即時、雙向通訊 | 非持久化、需訊息儲存 | 即時通知顯示（搭配 Service Bus） | App Service 內含 |
| **Database Polling** | 無需額外服務 | 效能差、延遲高 | 不推薦 | 無額外成本 |

#### 4.2.3 推薦架構：Azure Service Bus + SignalR

```
┌──────────────┐
│  API Actions │ (CRUD 操作)
└──────┬───────┘
       │ 1. 執行業務邏輯
       ▼
┌──────────────────┐
│ Service Bus      │ 2. 發送訊息到佇列
│ Queue (持久化)    │
└──────┬───────────┘
       │ 3. Background Service 訂閱
       ▼
┌──────────────────┐
│ Background Worker│ 4. 處理訊息、儲存到 DB
│ (BackgroundSvc)  │
└──────┬───────────┘
       │ 5. 通知已連線客戶端
       ▼
┌──────────────────┐
│ SignalR Hub      │ 6. 推送到前端
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│ React Client     │ 7. 即時顯示通知
└──────────────────┘
```

**關鍵優勢**:
- 持久化：Service Bus 保證訊息不遺失
- 即時性：SignalR 提供即時推送體驗
- 解耦：API 不直接依賴 SignalR 連線狀態
- 可靠性：Worker 失敗時訊息仍在佇列中

### 4.3 檔案上傳遷移風險

#### 4.3.1 本地檔案系統 → Azure Blob Storage

| 風險項目 | 影響 | 緩解措施 |
|---------|------|---------|
| 現有檔案遷移 | 需將 `/Uploads` 資料夾內容上傳到 Blob | 建立遷移腳本，使用 AzCopy 批次上傳 |
| URL 路徑變更 | `/Uploads/xxx.jpg` → `https://{account}.blob.core.windows.net/{container}/xxx.jpg` | 資料庫儲存完整 Blob URL 或相對路徑 + 組合邏輯 |
| 存取控制 | 本地無限制 → Blob 需 SAS Token | 使用 SAS Token 或 Azure CDN + Public Access (限讀取) |
| 上傳流程變更 | `Server.MapPath()` → `BlobClient.UploadAsync()` | 使用 `Azure.Storage.Blobs` SDK |

#### 4.3.2 Blob Storage 配置建議

```csharp
// appsettings.json
{
  "AzureBlobStorage": {
    "ConnectionString": "...",
    "ContainerName": "teaching-materials",
    "PublicAccess": false,  // 使用 SAS Token
    "SasTokenExpiryMinutes": 60,
    "CdnEndpoint": "https://{cdn}.azureedge.net"  // 可選
  }
}

// Service 實作
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, overwrite: false);
        
        // 生成 SAS Token URL（限時存取）
        return blobClient.GenerateSasUri(
            BlobSasPermissions.Read, 
            DateTimeOffset.UtcNow.AddHours(1)
        ).ToString();
    }
}
```

### 4.4 並行控制風險

#### 4.4.1 樂觀鎖實作需求

**當前狀態**: 部分實體（如 `Department`）使用 `RowVersion` 進行並行控制

**遷移需求**:
1. 保留 `RowVersion` (Timestamp) 欄位
2. API 更新端點需驗證 `RowVersion`
3. 前端需傳遞 `rowVersion` 並處理並行衝突

**實作範例**:
```csharp
// UpdateDepartmentDto
public class UpdateDepartmentDto
{
    public string Name { get; set; }
    public decimal Budget { get; set; }
    [Required]
    public byte[] RowVersion { get; set; }  // 必須傳遞
}

// Controller 處理
try
{
    _context.Entry(department).Property(d => d.RowVersion).OriginalValue = dto.RowVersion;
    await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException)
{
    return Conflict(new { message = "資料已被其他使用者修改，請重新載入後再試" });
}
```

### 4.5 認證授權遷移風險

#### 4.5.1 當前狀態

- **認證**: Windows Authentication（IIS Express）
- **授權**: 無角色管理（所有功能公開）

#### 4.5.2 目標狀態

**選項 A - JWT Bearer Token（建議先行）**:
- 簡單、無狀態、適合 SPA
- 前端儲存 Token（LocalStorage/SessionStorage）
- 每次請求帶 `Authorization: Bearer {token}`

**選項 B - Azure AD B2C（企業級）**:
- SSO、MFA、完整使用者管理
- 適合未來多租戶擴展
- 成本較高、配置複雜

**選項 C - ASP.NET Core Identity（中期）**:
- 完整使用者管理功能
- 支援角色、Claim-based 授權
- 資料庫儲存使用者

**建議路徑**: 初期不實作認證（僅 CORS） → 中期 JWT + Identity → 長期考慮 Azure AD

---

## 5. 遷移階段規劃

### 5.1 Phase 1: 基礎架構準備（Week 1-2）

**目標**: 建立後端 API 骨架與 CI/CD Pipeline

**任務**:
- [ ] 建立 ASP.NET Core 8 Web API 專案
- [ ] 配置 EF Core 8.0 並驗證資料庫連線
- [ ] 設定 `appsettings.json` 並遷移 Web.config 關鍵配置
- [ ] 實作基礎 DI 容器與服務註冊
- [ ] 設定 CORS 允許前端開發環境
- [ ] 建立 GitHub Actions 工作流程（build + test）
- [ ] 部署測試環境到 Azure App Service

**產出**:
- 可運行的空 API 專案
- Swagger UI 可訪問
- CI/CD 自動部署就緒

### 5.2 Phase 2: 核心 API 開發（Week 3-6）

**目標**: 實作所有核心模組的 RESTful API

**優先順序**:
1. 學生管理 API（最簡單，作為範本）
2. 課程管理 API（含檔案上傳，Blob Storage 整合）
3. 講師管理 API
4. 部門管理 API
5. 統計報表 API

**每個模組交付標準**:
- ✅ 完整 CRUD 端點
- ✅ DTO 與 AutoMapper 配置
- ✅ 單元測試覆蓋率 > 70%
- ✅ Swagger 文件完整
- ✅ 整合測試通過

### 5.3 Phase 3: 前端 React 應用開發（Week 7-10）

**目標**: 建立 React SPA 並逐模組遷移

**初始化**:
- [ ] 使用 Vite 建立 React + TypeScript 專案
- [ ] 配置 Axios 與 API Client（OpenAPI Generator）
- [ ] 建立通用元件（Layout, Table, Form, Modal）
- [ ] 實作 React Router 路由結構

**模組遷移順序**:
1. 學生管理頁面
2. 課程管理頁面
3. 講師管理頁面
4. 部門管理頁面
5. 統計儀表板

**每個模組交付標準**:
- ✅ 功能與舊版一致
- ✅ 響應式設計（RWD）
- ✅ 錯誤處理與 Loading 狀態
- ✅ E2E 測試通過（Playwright）

### 5.4 Phase 4: 進階功能遷移（Week 11-13）

**目標**: 遷移通知系統、待辦事項、檔案上傳

**關鍵任務**:
- [ ] 實作 Azure Service Bus 佇列服務
- [ ] 建立 Background Worker 處理訊息
- [ ] 實作 SignalR Hub 進行即時推送
- [ ] 前端整合 SignalR Client
- [ ] 待辦事項移除 Stored Procedure，改用 EF
- [ ] Azure Blob Storage 檔案上傳完整流程
- [ ] 遷移現有 `/Uploads` 檔案到 Blob

### 5.5 Phase 5: 測試與優化（Week 14-15）

**目標**: 完整測試與效能優化

**任務**:
- [ ] 負載測試（Azure Load Testing）
- [ ] 安全性掃描（GitHub Advanced Security, OWASP ZAP）
- [ ] 效能調優（API 快取、資料庫索引）
- [ ] 使用者驗收測試（UAT）
- [ ] 文件撰寫（API 文件、部署指南、使用者手冊）

### 5.6 Phase 6: 上線與監控（Week 16）

**目標**: 生產環境部署與監控建立

**任務**:
- [ ] 生產環境 Azure 資源配置
- [ ] 資料庫遷移腳本執行
- [ ] DNS 與 SSL 憑證配置
- [ ] Application Insights 監控設定
- [ ] 建立 Runbook（故障排除手冊）
- [ ] 正式上線與公告

---

## 6. 成功指標

### 6.1 功能性指標

- [ ] 所有現有功能在新架構中可用
- [ ] API 回應時間 < 200ms（95th percentile）
- [ ] 前端首次載入 < 3 秒
- [ ] 零功能回歸（Regression）

### 6.2 品質指標

- [ ] 後端單元測試覆蓋率 > 80%
- [ ] 前端元件測試覆蓋率 > 70%
- [ ] 無 Critical/High 安全漏洞
- [ ] Lighthouse 效能評分 > 90

### 6.3 維護性指標

- [ ] API 文件完整（Swagger）
- [ ] 程式碼符合 ESLint/SonarQube 規範
- [ ] 部署自動化（一鍵部署）
- [ ] 監控與告警配置完成

---

## 7. 參考資源

### 7.1 官方文件

- [ASP.NET Core 8 遷移指南](https://learn.microsoft.com/aspnet/core/migration/proper-to-2x/)
- [EF Core 遷移指南](https://learn.microsoft.com/ef/core/what-is-new/ef-core-8.0/whatsnew)
- [Azure App Service 部署](https://learn.microsoft.com/azure/app-service/quickstart-dotnetcore)
- [Azure Service Bus 快速入門](https://learn.microsoft.com/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues)
- [Vite 官方文件](https://vitejs.dev/guide/)
- [React 18 文件](https://react.dev/)

### 7.2 最佳實踐

- [Microsoft REST API Guidelines](https://github.com/microsoft/api-guidelines)
- [Azure Architecture Center](https://learn.microsoft.com/azure/architecture/)
- [.NET Microservices eBook](https://dotnet.microsoft.com/learn/aspnet/microservices-architecture)

---

## 附錄 A: 關鍵決策記錄 (ADR)

### ADR-001: 選擇 React 而非 Vue/Angular

**背景**: 需選擇前端框架

**決策**: React + TypeScript

**理由**:
- 社群最大、生態系最成熟
- 與 Azure Static Web Apps 整合最佳
- 團隊學習資源豐富
- TypeScript 支援完善

**後果**: 需投入時間學習 React Hooks 與函數式程式設計

### ADR-002: 使用 Azure Service Bus 取代 MSMQ

**背景**: MSMQ 無法在 Azure App Service Linux 上運行

**決策**: Azure Service Bus + Background Worker + SignalR

**理由**:
- 雲端原生、無需自建佇列伺服器
- 自動擴展與高可用性
- 與 Azure 生態整合（Azure Monitor, Key Vault）

**後果**: 額外成本（基礎層約 $10/月），需學習 Azure SDK

### ADR-003: 階段式遷移 EF Core 版本

**背景**: EF Core 3.1 → 8.0 跨度大

**決策**: 先升級到 6.0，測試穩定後再升級到 8.0

**理由**:
- 降低風險
- 每階段可完整回歸測試
- 6.0 是 LTS 版本，可作為穩定中繼站

**後果**: 遷移時程延長約 1-2 週

---

## 附錄 B: Glossary（術語表）

| 術語 | 說明 |
|-----|------|
| **DTO** | Data Transfer Object，資料傳輸物件 |
| **SPA** | Single Page Application，單頁應用程式 |
| **CORS** | Cross-Origin Resource Sharing，跨來源資源共享 |
| **JWT** | JSON Web Token，用於認證的 Token 格式 |
| **SAS Token** | Shared Access Signature，Azure Blob 的限時存取憑證 |
| **SignalR** | ASP.NET 即時通訊函式庫（WebSocket） |
| **ADR** | Architecture Decision Record，架構決策記錄 |
| **RWD** | Responsive Web Design，響應式網頁設計 |
| **UAT** | User Acceptance Testing，使用者驗收測試 |

---

**文件版本**: 1.0  
**最後更新**: 2026-02-05  
**作者**: Contoso University 遷移團隊  
**審核者**: (待指定)
