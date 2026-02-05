# Contoso University 系統規格文件

## 文件概覽

本資料夾包含從遺留程式碼分析並提取的完整系統規格文件，用於理解、維護和未來的遷移或重構專案。

## 文件結構

### 核心規格文件（舊有系統）

1. **[系統架構概覽](./01-system-architecture-overview.md)**
   - 系統目標與範疇
   - 整體架構設計
   - 關鍵設計原則

2. **[資料模型規格](./02-data-model-specification.md)**
   - 實體關係圖
   - 資料模型詳細定義
   - 實體關聯關係

3. **[功能模組規格](./03-functional-modules-specification.md)**
   - 學生管理模組
   - 課程管理模組
   - 講師管理模組
   - 部門管理模組
   - 首頁與統計模組

4. **[特殊功能規格](./04-special-features-specification.md)**
   - 即時通知系統
   - 待辦事項系統
   - 教材上傳系統

5. **[技術架構規格](./05-technical-architecture-specification.md)**
   - 技術棧
   - 基礎設施依賴
   - 系統需求
   - 配置管理

6. **[資料庫規格](./06-database-specification.md)**
   - 資料庫架構
   - 資料表定義
   - 存儲程序
   - 初始化策略

### 升級與重構規格文件（前後端分離架構）

7. **[架構拆分與升級規格](./07-architecture-migration-specification.md)** ⭐ NEW
   - 架構遷移目標（React + .NET 8）
   - 相容性評估（System.Web 依賴盤點）
   - Web.config → appsettings.json 移轉
   - 前後端拆分策略
   - API 化可行性評估
   - 風險與替代方案（MSMQ → Azure Service Bus）
   - 遷移階段規劃

8. **[套件與安全性規格](./08-package-security-specification.md)** ⭐ NEW
   - 套件管理策略（.NET 8 + React）
   - 安全性要求與漏洞管理
   - 套件版本風險評估
   - Microsoft.Data.SqlClient 安全指南
   - Azure SDK 安全最佳實踐
   - 持續安全監控機制

9. **[DI 與服務設計規格](./09-di-service-design-specification.md)** ⭐ NEW
   - 依賴注入架構設計
   - 服務層架構（Application/Domain/Infrastructure）
   - Repository Pattern 實作
   - Unit of Work Pattern
   - 服務生命週期管理
   - 測試策略

10. **[部署與驗證規格](./10-deployment-verification-specification.md)** ⭐ NEW
    - Azure 資源架構規劃
    - CI/CD Pipeline 設計（GitHub Actions）
    - 環境管理（Dev/Staging/Prod）
    - 監控與告警（Application Insights）
    - 災難復原計劃
    - 上線前驗證準則

## 使用指南

### 閱讀順序建議

#### 了解現有系統

如果您是第一次接觸此系統，建議按照以下順序閱讀舊有系統規格：
- 系統架構概覽 (01) → 資料模型規格 (02) → 功能模組規格 (03) → 特殊功能規格 (04) → 技術架構規格 (05) → 資料庫規格 (06)

#### 規劃升級與重構

如果您需要進行系統升級與前後端分離，建議按照以下順序閱讀：
1. **先閱讀舊有系統規格** (01-06)，理解現有架構
2. **架構拆分與升級規格** (07) - 了解遷移目標與策略
3. **套件與安全性規格** (08) - 了解套件選型與安全要求
4. **DI 與服務設計規格** (09) - 了解服務層架構設計
5. **部署與驗證規格** (10) - 了解部署流程與驗證標準

#### 特定任務導向

- **前端開發者**: 閱讀 07 (API 端點設計) + 08 (前端套件) + 10 (前端部署)
- **後端開發者**: 閱讀 07 (架構拆分) + 08 (後端套件) + 09 (服務設計) + 10 (後端部署)
- **DevOps 工程師**: 閱讀 10 (CI/CD Pipeline) + 08 (安全性監控)
- **架構師**: 閱讀所有升級規格文件 (07-10)
- **QA 測試人員**: 閱讀 07 (功能範圍) + 10 (驗證準則)

### 文件維護

- 當程式碼有重大變更時，應同步更新相關規格文件
- 規格文件應保持與實際實作的一致性
- 新增功能時，應在相應的規格文件中添加說明

## 系統快速概覽

**系統名稱**: Contoso University 管理系統

**系統類型**: ASP.NET MVC 5 Web 應用程式

**主要功能**:
- 學生資訊管理（CRUD、分頁、搜尋）
- 課程管理與教材上傳
- 講師管理與辦公室分配
- 部門管理
- 即時通知系統
- 待辦事項管理
- 註冊統計報表

**技術棧**:
- .NET Framework 4.8.2
- ASP.NET MVC 5
- Entity Framework Core 3.1.32
- SQL Server LocalDB
- MSMQ (Microsoft Message Queuing)
- Bootstrap 5.3.3

## 版本歷史

| 版本 | 日期 | 說明 |
|------|------|------|
| 1.0 | 2026-02-05 | 初始版本 - 從遺留程式碼提取規格 |
| 2.0 | 2026-02-05 | 新增升級與重構規格文件 (07-10) - 前後端分離架構 |

## 文件概要

### 舊有系統 (01-06)

**目標**: 記錄現有 ASP.NET MVC 5 (.NET Framework 4.8) 單體架構系統的完整規格

**技術棧**:
- .NET Framework 4.8.2
- ASP.NET MVC 5
- Entity Framework Core 3.1.32
- SQL Server LocalDB
- MSMQ (Microsoft Message Queuing)
- Bootstrap 5.3.3

### 升級架構 (07-10)

**目標**: 定義遷移到前後端分離現代化架構的完整規格與實施計劃

**新技術棧**:
- **前端**: React 18 + TypeScript + Vite
- **後端**: ASP.NET Core 8.0 Web API
- **資料庫**: Azure SQL Database
- **雲端服務**: 
  - Azure App Service (Linux)
  - Azure Blob Storage
  - Azure Service Bus
  - Application Insights
- **部署**: GitHub Actions CI/CD

## 參考資源

- [原始程式碼](../ContosoUniversity/)
- [操作練習文件](../Lab/)
- [專案 README](../README.md)
