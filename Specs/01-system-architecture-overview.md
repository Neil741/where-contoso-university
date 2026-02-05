# 系統架構概覽

## 系統目標

Contoso University 是一個高等教育機構管理系統，旨在提供完整的學生、課程、講師和部門管理功能。系統支援：

- **學術管理**: 管理學生註冊、課程安排、講師分配
- **資料追蹤**: 記錄並追蹤學生成績、課程進度
- **即時通知**: 提供管理員即時的資料變更通知
- **教學支援**: 支援教材上傳與管理
- **任務管理**: 提供待辦事項管理功能

## 系統範疇

### 核心功能範疇

1. **學生管理**
   - 學生基本資料維護
   - 課程註冊管理
   - 學生搜尋與分頁瀏覽

2. **課程管理**
   - 課程資訊維護
   - 教材上傳與管理
   - 課程與部門關聯

3. **講師管理**
   - 講師資料維護
   - 辦公室分配
   - 課程指派

4. **部門管理**
   - 部門資訊維護
   - 預算管理
   - 部門主任指派

5. **統計報表**
   - 學生註冊日期統計
   - 資料概覽儀表板

### 特殊功能範疇

1. **即時通知系統**
   - CRUD 操作通知
   - 僅管理員可見
   - 基於 MSMQ 的訊息佇列

2. **待辦事項系統**
   - 任務建立與追蹤
   - 完成狀態管理
   - 基於存儲程序的資料訪問

3. **教材上傳系統**
   - 圖片格式教材上傳
   - 檔案大小限制
   - 自動檔案管理

## 整體架構設計

### 系統架構層級

```
┌─────────────────────────────────────────────────────────┐
│                    使用者介面層 (Views)                    │
│  - Razor 視圖引擎                                         │
│  - Bootstrap 5.3.3 樣式框架                               │
│  - jQuery 客戶端腳本                                      │
└───────────────────────┬─────────────────────────────────┘
                        │
┌───────────────────────▼─────────────────────────────────┐
│                    控制器層 (Controllers)                 │
│  - HomeController (首頁與統計)                            │
│  - StudentsController (學生管理)                          │
│  - CoursesController (課程管理)                           │
│  - InstructorsController (講師管理)                       │
│  - DepartmentsController (部門管理)                       │
│  - ToDosController (待辦事項)                             │
│  - NotificationsController (通知管理)                     │
│  - BaseController (基礎控制器)                            │
└───────────────────────┬─────────────────────────────────┘
                        │
┌───────────────────────▼─────────────────────────────────┐
│                    服務層 (Services)                      │
│  - NotificationService (通知服務 - MSMQ)                  │
│  - LoggingService (日誌服務)                              │
└───────────────────────┬─────────────────────────────────┘
                        │
┌───────────────────────▼─────────────────────────────────┐
│                    資料訪問層 (Data)                      │
│  - SchoolContext (EF Core DbContext)                     │
│  - DbInitializer (資料庫初始化)                           │
│  - 存儲程序 (ToDo 功能)                                   │
└───────────────────────┬─────────────────────────────────┘
                        │
┌───────────────────────▼─────────────────────────────────┐
│                    資料層 (Database)                      │
│  - SQL Server LocalDB                                    │
│  - Entity Framework Core 管理的資料表                     │
│  - 自訂存儲程序                                          │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│               外部基礎設施 (Infrastructure)                │
│  - MSMQ (訊息佇列 - 通知系統)                             │
│  - 檔案系統 (教材上傳儲存)                                │
└─────────────────────────────────────────────────────────┘
```

### 架構特點

1. **分層架構**
   - 遵循 MVC 模式
   - 清晰的職責分離
   - 易於維護和擴展

2. **混合資料訪問策略**
   - 主要使用 Entity Framework Core 進行 ORM
   - 特定功能（ToDo）使用存儲程序直接訪問
   - 支援高性能和靈活性

3. **基礎控制器模式**
   - BaseController 提供共用功能
   - 統一的資料庫訪問
   - 集中的通知管理

4. **非同步訊息處理**
   - 使用 MSMQ 解耦通知系統
   - 提高系統響應性
   - 支援可靠的訊息傳遞

## 關鍵設計原則

### 1. 關注點分離 (Separation of Concerns)

- **視圖**: 僅負責呈現
- **控制器**: 處理使用者請求和協調
- **服務**: 封裝業務邏輯
- **資料訪問**: 管理資料持久化

### 2. DRY 原則 (Don't Repeat Yourself)

- BaseController 提供共用功能
- 共用的分頁邏輯 (PaginatedList)
- 統一的通知機制

### 3. 單一職責原則 (Single Responsibility Principle)

- 每個控制器專注於單一實體管理
- 服務類別專注於特定功能（通知、日誌）
- 模型僅表示資料結構

### 4. 開放封閉原則 (Open/Closed Principle)

- 透過繼承擴展功能（BaseController）
- 介面分離（資料訪問抽象）

### 5. 依賴反轉原則 (Dependency Inversion Principle)

- 透過 SchoolContext 抽象資料訪問
- 服務注入模式

## 系統整合點

### 外部系統整合

1. **SQL Server LocalDB**
   - 連接字串: `ContosoUniversityNoAuthEFCore`
   - 自動遷移和初始化

2. **MSMQ (Microsoft Message Queuing)**
   - 佇列路徑配置於 Web.config
   - 用於即時通知系統

3. **檔案系統**
   - 教材上傳目錄: `/Uploads/TeachingMaterials/`
   - 檔案命名規則: `course_{CourseID}_{GUID}.{extension}`

### 內部模組整合

1. **控制器 → 服務**
   - 透過 BaseController 存取 NotificationService
   - 直接實例化服務物件

2. **控制器 → 資料訪問**
   - 透過 SchoolContext 進行 CRUD 操作
   - ToDo 功能透過 ADO.NET 調用存儲程序

3. **前端 → 後端**
   - AJAX 調用通知 API
   - 表單提交與驗證
   - 分頁導航

## 部署架構

### 開發環境

- **作業系統**: Windows (MSMQ 相依)
- **開發工具**: Visual Studio 2019+
- **Web 伺服器**: IIS Express
- **資料庫**: SQL Server LocalDB

### 運行時需求

- **.NET Framework**: 4.8.2
- **MSMQ**: 必須啟用 Windows 功能
- **IIS**: 用於生產環境部署
- **SQL Server**: LocalDB (開發) / SQL Server (生產)

## 安全考量

### 角色與權限

- **Admin 角色**: 擁有所有權限
- **Teacher 角色**: 可以上傳教材
- **通知系統**: 僅管理員可見

### 資料驗證

- 用戶端驗證（jQuery Validation）
- 伺服器端模型驗證
- 檔案上傳驗證（格式、大小）

### 並發控制

- 樂觀並發控制（Entity Framework）
- RowVersion 欄位防止更新衝突

## 效能考量

### 資料庫優化

- 適當的索引設計
- 延遲載入與預先載入策略
- 存儲程序用於複雜查詢

### 分頁機制

- PaginatedList 類別提供高效分頁
- 避免一次載入所有資料

### 非同步處理

- MSMQ 非同步通知
- 降低主要業務流程的延遲

## 未來擴展考量

### 可能的遷移路徑

1. **雲端遷移**
   - Azure Container Apps
   - Azure SQL Database
   - Azure Service Bus (替代 MSMQ)
   - Azure Blob Storage (替代本地檔案系統)

2. **現代化技術棧**
   - .NET 6+ / .NET 8+
   - ASP.NET Core
   - 前端框架（React/Angular/Vue）

3. **微服務架構**
   - 將功能模組拆分為獨立服務
   - API Gateway
   - 事件驅動架構

## 限制與約束

### 技術限制

- 依賴 Windows 平台（MSMQ）
- .NET Framework 4.8.2（非跨平台）
- 本地檔案系統存儲

### 功能限制

- 通知系統僅支援管理員
- 教材上傳僅支援圖片格式
- 檔案大小限制 5MB

### 擴展限制

- 單體應用架構
- MSMQ 單機部署限制
- LocalDB 適用於開發環境

## 參考文件

- [資料模型規格](./02-data-model-specification.md)
- [功能模組規格](./03-functional-modules-specification.md)
- [技術架構規格](./05-technical-architecture-specification.md)
