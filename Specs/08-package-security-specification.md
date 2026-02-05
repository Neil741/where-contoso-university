# 套件與安全性規格

## 概述

本文件定義 Contoso University 遷移至 .NET 8 與 React 架構後的套件管理策略、安全性要求、漏洞管理流程，以及持續安全監控機制。

---

## 1. 套件管理策略

### 1.1 後端套件管理 (.NET 8)

#### 1.1.1 核心套件清單

| 套件名稱 | 目標版本 | 用途 | 授權 | 安全性評級 |
|---------|---------|------|------|-----------|
| `Microsoft.AspNetCore.App` | 8.0.x（Framework） | ASP.NET Core 執行環境 | MIT | ✅ 官方維護 |
| `Microsoft.EntityFrameworkCore` | 8.0.x | ORM 框架 | MIT | ✅ 官方維護 |
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.0.x | SQL Server 提供者 | MIT | ✅ 官方維護 |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.x | 遷移工具 | MIT | ✅ 官方維護 |
| `Microsoft.Data.SqlClient` | 5.2.x+ | SQL Server 資料提供者 | MIT | ✅ 需定期更新 |
| `AutoMapper` | 13.0.x | DTO 對映 | MIT | ✅ 社群活躍 |
| `FluentValidation.AspNetCore` | 11.3.x | 驗證框架 | Apache 2.0 | ✅ 社群活躍 |
| `Swashbuckle.AspNetCore` | 6.5.x | Swagger/OpenAPI | MIT | ✅ 廣泛使用 |
| `Azure.Storage.Blobs` | 12.x | Azure Blob 儲存 | MIT | ✅ 官方維護 |
| `Azure.Messaging.ServiceBus` | 7.x | Azure Service Bus | MIT | ✅ 官方維護 |
| `Microsoft.AspNetCore.SignalR` | 8.0.x（Framework） | 即時通訊 | MIT | ✅ 官方維護 |
| `Serilog.AspNetCore` | 8.0.x | 結構化日誌 | Apache 2.0 | ✅ 廣泛使用 |
| `Serilog.Sinks.ApplicationInsights` | 4.0.x | Azure 日誌整合 | Apache 2.0 | ✅ 官方整合 |

#### 1.1.2 開發與測試套件

| 套件名稱 | 目標版本 | 用途 | 授權 |
|---------|---------|------|------|
| `xUnit` | 2.6.x | 單元測試框架 | Apache 2.0 |
| `xUnit.runner.visualstudio` | 2.5.x | Test Explorer 整合 | Apache 2.0 |
| `Moq` | 4.20.x | Mocking 框架 | BSD-3 |
| `FluentAssertions` | 6.12.x | 斷言函式庫 | Apache 2.0 |
| `Microsoft.AspNetCore.Mvc.Testing` | 8.0.x | 整合測試 | MIT |
| `Testcontainers` | 3.7.x | 容器化測試（可選）| MIT |
| `Bogus` | 35.x | 假資料生成 | MIT |

#### 1.1.3 .NET 8 Framework 包含套件（無需額外安裝）

- `System.Text.Json` - JSON 序列化（優先於 Newtonsoft.Json）
- `System.ComponentModel.DataAnnotations` - 驗證特性
- `Microsoft.Extensions.Configuration` - 配置管理
- `Microsoft.Extensions.DependencyInjection` - DI 容器
- `Microsoft.Extensions.Logging` - 日誌抽象

### 1.2 前端套件管理 (React + TypeScript)

#### 1.2.1 核心套件清單

| 套件名稱 | 目標版本 | 用途 | 授權 | 套件大小 |
|---------|---------|------|------|---------|
| `react` | ^18.3.0 | 核心函式庫 | MIT | ~6 KB (gzip) |
| `react-dom` | ^18.3.0 | DOM 渲染 | MIT | ~130 KB (gzip) |
| `typescript` | ^5.4.0 | 型別系統 | Apache 2.0 | DevDep |
| `react-router-dom` | ^6.22.0 | 路由管理 | MIT | ~11 KB (gzip) |
| `@tanstack/react-query` | ^5.28.0 | 伺服器狀態管理 | MIT | ~40 KB (gzip) |
| `axios` | ^1.6.8 | HTTP 客戶端 | MIT | ~13 KB (gzip) |
| `@mui/material` | ^5.15.0 | UI 元件庫 | MIT | ~300 KB (gzip) |
| `@emotion/react` | ^11.11.0 | CSS-in-JS（MUI 依賴）| MIT | ~13 KB (gzip) |
| `@emotion/styled` | ^11.11.0 | Styled Components | MIT | ~6 KB (gzip) |
| `react-hook-form` | ^7.51.0 | 表單管理 | MIT | ~25 KB (gzip) |
| `zod` | ^3.22.0 | Schema 驗證 | MIT | ~15 KB (gzip) |
| `@hookform/resolvers` | ^3.3.0 | 驗證整合 | MIT | ~3 KB (gzip) |
| `date-fns` | ^3.6.0 | 日期處理 | MIT | ~20 KB (gzip) |
| `react-hot-toast` | ^2.4.0 | 通知提示 | MIT | ~5 KB (gzip) |
| `recharts` | ^2.12.0 | 圖表元件 | MIT | ~150 KB (gzip) |
| `@microsoft/signalr` | ^8.0.0 | SignalR 客戶端 | MIT | ~45 KB (gzip) |

#### 1.2.2 開發與建置工具

| 套件名稱 | 目標版本 | 用途 | 授權 |
|---------|---------|------|------|
| `vite` | ^5.2.0 | 建置工具 | MIT |
| `@vitejs/plugin-react` | ^4.2.0 | React 支援 | MIT |
| `eslint` | ^8.57.0 | 程式碼檢查 | MIT |
| `@typescript-eslint/parser` | ^7.4.0 | TypeScript ESLint | MIT |
| `prettier` | ^3.2.5 | 程式碼格式化 | MIT |
| `vitest` | ^1.4.0 | 單元測試框架 | MIT |
| `@testing-library/react` | ^14.2.0 | React 測試工具 | MIT |
| `@playwright/test` | ^1.43.0 | E2E 測試 | Apache 2.0 |

#### 1.2.3 型別定義套件

| 套件名稱 | 目標版本 | 用途 |
|---------|---------|------|
| `@types/react` | ^18.3.0 | React 型別定義 |
| `@types/react-dom` | ^18.3.0 | ReactDOM 型別定義 |
| `@types/node` | ^20.12.0 | Node.js 型別定義 |

### 1.3 套件版本管理原則

#### 1.3.1 語意化版本控制 (Semantic Versioning)

遵循 `MAJOR.MINOR.PATCH` 規則：

```json
{
  "dependencies": {
    "react": "^18.3.0",           // 允許 MINOR 與 PATCH 更新（18.x.x）
    "axios": "~1.6.8",            // 僅允許 PATCH 更新（1.6.x）
    "typescript": "5.4.5"         // 鎖定確切版本
  }
}
```

**版本範圍規則**:
- `^` (Caret): 允許不改變最左邊非零版本號的更新（預設，建議用於穩定套件）
- `~` (Tilde): 僅允許 PATCH 版本更新（用於關鍵套件）
- 無前綴: 鎖定確切版本（用於已知問題套件或 build 工具）

#### 1.3.2 Lock File 管理

**後端 (.NET)**:
- 使用 `packages.lock.json` 鎖定傳遞依賴
- 啟用方式: `.csproj` 中設定 `<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>`

**前端 (Node.js)**:
- 使用 `package-lock.json` (npm) 或 `pnpm-lock.yaml` (pnpm)
- **必須提交到 Git** 確保跨環境一致性

#### 1.3.3 套件更新週期

| 更新類型 | 週期 | 檢查工具 | 自動化 |
|---------|------|---------|--------|
| **安全性更新** | 即時修復 | Dependabot, Snyk | ✅ 自動 PR |
| **PATCH 更新** | 每月 | `npm outdated`, `dotnet list package --outdated` | ⚠️ 手動審查 |
| **MINOR 更新** | 每季 | 同上 | ❌ 完整測試後 |
| **MAJOR 更新** | 年度或按需 | 同上 + 遷移指南 | ❌ 專案規劃 |

---

## 2. 安全性要求

### 2.1 套件安全性掃描

#### 2.1.1 掃描工具與整合

| 工具 | 掃描範圍 | 整合方式 | 頻率 |
|-----|---------|---------|------|
| **GitHub Dependabot** | 後端 + 前端 | `.github/dependabot.yml` | 每日 |
| **GitHub Advanced Security** | 程式碼 + 依賴 | GitHub Actions | 每次 PR + Push |
| **Snyk** | 依賴 + 容器映像 | CLI + GitHub Action | CI/CD Pipeline |
| **OWASP Dependency-Check** | 後端 NuGet | MSBuild Plugin | 每週排程 |
| **npm audit** | 前端 | `npm audit --audit-level=high` | 每次建置 |

#### 2.1.2 Dependabot 配置範例

```yaml
# .github/dependabot.yml
version: 2
updates:
  # .NET 後端
  - package-ecosystem: "nuget"
    directory: "/ContosoUniversity.Api"
    schedule:
      interval: "daily"
    open-pull-requests-limit: 10
    reviewers:
      - "team-backend"
    labels:
      - "dependencies"
      - "security"
    # 忽略 MAJOR 版本自動更新
    ignore:
      - dependency-name: "*"
        update-types: ["version-update:semver-major"]

  # React 前端
  - package-ecosystem: "npm"
    directory: "/ContosoUniversity.Web"
    schedule:
      interval: "daily"
    open-pull-requests-limit: 10
    reviewers:
      - "team-frontend"
    labels:
      - "dependencies"
      - "javascript"
```

### 2.2 已知漏洞管理

#### 2.2.1 漏洞嚴重性分級

| 級別 | CVSS 分數 | 處理時效 | 處理策略 |
|-----|----------|---------|---------|
| **Critical** | 9.0-10.0 | 24 小時內 | 立即更新或移除套件 |
| **High** | 7.0-8.9 | 7 天內 | 盡快更新，評估替代方案 |
| **Medium** | 4.0-6.9 | 30 天內 | 排程更新，監控狀態 |
| **Low** | 0.1-3.9 | 季度更新 | 例行維護時處理 |

#### 2.2.2 漏洞處理流程

```
┌─────────────────┐
│ 1. 漏洞發現      │ (Dependabot Alert / Snyk Scan)
└────────┬────────┘
         ▼
┌─────────────────┐
│ 2. 影響評估      │ (檢查套件使用範圍與功能影響)
└────────┬────────┘
         ▼
    ┌────┴────┐
    │ 可更新？ │
    └────┬────┘
         │
    ┌────┴─────────────┐
    │ Yes              │ No
    ▼                  ▼
┌──────────┐      ┌──────────────┐
│ 3. 更新   │      │ 3. 尋找替代   │
│ 套件版本  │      │    - 替代套件 │
└────┬─────┘      │    - 修補程式 │
     │            │    - 功能移除 │
     │            └───────┬──────┘
     │                    │
     └────────┬───────────┘
              ▼
    ┌─────────────────┐
    │ 4. 測試驗證      │ (單元測試 + 整合測試)
    └────────┬────────┘
             ▼
    ┌─────────────────┐
    │ 5. 部署更新      │ (PR Review → Merge → Deploy)
    └────────┬────────┘
             ▼
    ┌─────────────────┐
    │ 6. 關閉 Alert    │ (GitHub / Snyk 標記已修復)
    └─────────────────┘
```

#### 2.2.3 無法立即修復的漏洞處理

**情境**: 套件有漏洞但無更新版本，且移除影響大

**暫時性緩解措施**:
1. **隔離漏洞代碼路徑**: 使用 Middleware/Guard 限制觸發條件
2. **WAF 規則**: Azure Application Gateway 設定攔截惡意請求
3. **監控告警**: Application Insights 監控異常行為
4. **文件記錄**: 在 `SECURITY.md` 記錄已知風險與緩解措施

**範例**:
```markdown
## 已知風險

### CVE-2024-XXXXX - Microsoft.Data.SqlClient 4.x
- **影響**: 特定條件下可能的 SQL Injection
- **暫時緩解**: 
  - 所有查詢已使用參數化查詢（無動態 SQL）
  - 資料庫連線使用最小權限帳戶（無 DROP/ALTER 權限）
  - Application Gateway WAF 開啟 SQL Injection 規則
- **計劃**: 等待官方修補版本（預計 2024 Q2）
```

### 2.3 授權合規性

#### 2.3.1 允許的開源授權

| 授權類型 | 允許商業使用 | 需揭露原始碼 | 備註 |
|---------|------------|------------|------|
| MIT | ✅ | ❌ | 最寬鬆，推薦 |
| Apache 2.0 | ✅ | ❌ | 需保留 NOTICE 檔案 |
| BSD (2-Clause/3-Clause) | ✅ | ❌ | 類似 MIT |
| ISC | ✅ | ❌ | 簡化版 MIT |
| MPL 2.0 | ✅ | ⚠️ 僅修改部分 | 檔案級 Copyleft |

#### 2.3.2 禁止的授權

| 授權類型 | 禁用原因 |
|---------|---------|
| **GPL v2/v3** | Copyleft，需開源整個專案 |
| **AGPL** | 網路服務也需開源 |
| **SSPL** | MongoDB 專用，限制雲端服務 |
| **Commons Clause** | 限制商業使用 |

#### 2.3.3 授權檢查工具

```bash
# 後端 - 使用 dotnet-project-licenses
dotnet tool install --global dotnet-project-licenses
dotnet-project-licenses -i ./ContosoUniversity.Api -o -e --format json

# 前端 - 使用 license-checker
npm install -g license-checker
license-checker --production --json --out licenses.json
```

---

## 3. 特定套件安全性指南

### 3.1 Microsoft.Data.SqlClient

#### 3.1.1 安全性考量

**版本需求**: >= 5.2.0（修復多個 CVE）

**關鍵 CVE 歷史**:
- CVE-2024-0056: 資訊洩露漏洞（已在 5.2.0 修復）
- CVE-2024-21319: 權限提升漏洞（已在 5.1.5 修復）

**安全配置**:
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;User ID=...;Password=...;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;MultipleActiveResultSets=False;"
  }
}
```

**必須開啟的選項**:
- `Encrypt=True` - 強制加密連線
- `TrustServerCertificate=False` - 驗證伺服器憑證
- `MultipleActiveResultSets=False` - 避免潛在的並行問題（除非必要）

**禁止的做法**:
- ❌ 使用動態 SQL 字串拼接
- ❌ 連線字串包含明文密碼（使用 Azure Key Vault）
- ❌ 使用 `sa` 或高權限帳戶

### 3.2 Azure SDK 套件

#### 3.2.1 Azure.Storage.Blobs

**版本需求**: >= 12.19.0

**安全最佳實踐**:
```csharp
// 使用 Azure Managed Identity（生產環境）
var blobServiceClient = new BlobServiceClient(
    new Uri("https://{account}.blob.core.windows.net"),
    new DefaultAzureCredential()  // 自動使用 Managed Identity
);

// 開發環境可使用連線字串
var blobServiceClient = new BlobServiceClient(
    Configuration["AzureBlobStorage:ConnectionString"]
);

// 生成 SAS Token 時限制權限與時效
BlobSasBuilder sasBuilder = new BlobSasBuilder
{
    BlobContainerName = containerName,
    BlobName = blobName,
    Resource = "b",  // b = blob
    StartsOn = DateTimeOffset.UtcNow,
    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),  // 最多 1 小時
};
sasBuilder.SetPermissions(BlobSasPermissions.Read);  // 僅讀取
```

**禁止的做法**:
- ❌ 公開 Storage Account Key
- ❌ SAS Token 有效期超過 24 小時
- ❌ 使用帳戶級 SAS Token（應用容器或 Blob 級）

#### 3.2.2 Azure.Messaging.ServiceBus

**版本需求**: >= 7.17.0

**安全最佳實踐**:
```csharp
// 使用 Managed Identity
var client = new ServiceBusClient(
    "{namespace}.servicebus.windows.net",
    new DefaultAzureCredential()
);

// 最小權限原則
// - 傳送端僅需 "Send" 權限
// - 接收端僅需 "Listen" 權限
// 避免使用 "Manage" 權限
```

### 3.3 React 前端安全套件

#### 3.3.1 防止 XSS (Cross-Site Scripting)

**React 內建保護**: `dangerouslySetInnerHTML` 應避免使用

**如需渲染 HTML**: 使用 `DOMPurify`

```bash
npm install dompurify @types/dompurify
```

```typescript
import DOMPurify from 'dompurify';

function SafeHtml({ html }: { html: string }) {
  const clean = DOMPurify.sanitize(html, { 
    ALLOWED_TAGS: ['b', 'i', 'em', 'strong', 'a'],
    ALLOWED_ATTR: ['href']
  });
  return <div dangerouslySetInnerHTML={{ __html: clean }} />;
}
```

#### 3.3.2 防止 CSRF (Cross-Site Request Forgery)

**後端配置**: ASP.NET Core 自動防護（使用 Anti-forgery Token）

**前端配合**: Axios 自動傳送 CSRF Token

```typescript
// axios 配置
import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  withCredentials: true,  // 允許發送 Cookie
  xsrfCookieName: 'XSRF-TOKEN',  // ASP.NET Core 預設名稱
  xsrfHeaderName: 'X-XSRF-TOKEN'
});
```

#### 3.3.3 敏感資料處理

**禁止在前端儲存**:
- ❌ 密碼（即使雜湊後）
- ❌ API Keys / Secrets
- ❌ 個人敏感資料（身分證字號、信用卡）

**JWT Token 儲存**:
- ✅ `HttpOnly Cookie`（最安全，防 XSS）
- ⚠️ `sessionStorage`（頁面關閉即清除）
- ❌ `localStorage`（避免，易受 XSS 攻擊）

---

## 4. 持續安全監控

### 4.1 CI/CD Pipeline 安全檢查

#### 4.1.1 GitHub Actions 工作流程

```yaml
# .github/workflows/security-scan.yml
name: Security Scan

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]
  schedule:
    - cron: '0 2 * * 1'  # 每週一凌晨 2 點

jobs:
  dependency-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      # 後端掃描
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore ./ContosoUniversity.Api
      
      - name: Run dotnet list package vulnerabilities
        run: |
          dotnet list ./ContosoUniversity.Api package --vulnerable --include-transitive
      
      # 前端掃描
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
      
      - name: Install dependencies
        run: cd ./ContosoUniversity.Web && npm ci
      
      - name: Run npm audit
        run: cd ./ContosoUniversity.Web && npm audit --audit-level=high
      
      # Snyk 掃描（需設定 SNYK_TOKEN）
      - name: Run Snyk Security Scan
        uses: snyk/actions/dotnet@master
        env:
          SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
        with:
          args: --severity-threshold=high

  code-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      # CodeQL 分析
      - name: Initialize CodeQL
        uses: github/codeql-action/init@v3
        with:
          languages: 'csharp, javascript'
      
      - name: Build Project
        run: dotnet build ./ContosoUniversity.Api
      
      - name: Perform CodeQL Analysis
        uses: github/codeql-action/analyze@v3
```

### 4.2 執行階段監控

#### 4.2.1 Application Insights 安全事件

**監控項目**:
- 異常的 API 呼叫頻率（DDoS / Brute Force）
- 未授權存取嘗試（401/403 錯誤激增）
- SQL 異常查詢（潛在 SQL Injection）
- 異常的檔案上傳行為

**告警規則範例**:
```kql
// Application Insights KQL 查詢
// 偵測 5 分鐘內超過 100 次 401 錯誤
requests
| where timestamp > ago(5m)
| where resultCode == 401
| summarize Count = count() by bin(timestamp, 1m)
| where Count > 100
```

#### 4.2.2 Azure Security Center

**啟用功能**:
- ✅ Microsoft Defender for App Service
- ✅ Microsoft Defender for SQL
- ✅ Microsoft Defender for Storage
- ✅ Vulnerability Assessment

---

## 5. 安全性檢查清單

### 5.1 開發階段

- [ ] 所有套件來自官方或可信任來源
- [ ] 套件版本鎖定在 `lock file`
- [ ] 無使用已棄用的套件
- [ ] 程式碼通過 ESLint/SonarQube 安全規則
- [ ] 敏感資料不提交到 Git（使用 `.gitignore`）
- [ ] API 端點有適當的驗證與授權

### 5.2 建置階段

- [ ] 依賴掃描通過（無 Critical/High 漏洞）
- [ ] 程式碼掃描通過（CodeQL）
- [ ] 容器映像掃描通過（如使用 Docker）
- [ ] 前端 Bundle 大小符合預期（< 500KB gzip）
- [ ] Source Map 不包含在生產建置中

### 5.3 部署階段

- [ ] 使用 HTTPS（Azure App Service 自動憑證）
- [ ] CORS 設定僅允許已知來源
- [ ] 資料庫連線使用最小權限帳戶
- [ ] 敏感配置儲存在 Azure Key Vault
- [ ] Managed Identity 已啟用（App Service → Azure 資源）
- [ ] Application Insights 監控已配置

### 5.4 執行階段

- [ ] 定期檢查 Dependabot Alerts（至少每週）
- [ ] 安全性更新在 SLA 內完成
- [ ] Application Insights 告警規則運作正常
- [ ] 存取日誌定期審查
- [ ] 災難復原計劃已測試

---

## 6. 事件應變計劃

### 6.1 安全漏洞應變流程

```
┌─────────────────┐
│ 1. 漏洞通報      │ (來源: Dependabot / 社群回報 / 內部發現)
└────────┬────────┘
         ▼
┌─────────────────┐
│ 2. 初步評估      │
│  - 確認影響範圍  │ (1 小時內)
│  - 判定嚴重性    │
└────────┬────────┘
         ▼
    ┌────┴────┐
    │ 嚴重性？ │
    └────┬────┘
         │
    Critical / High        Medium / Low
         │                      │
         ▼                      ▼
┌─────────────────┐    ┌─────────────────┐
│ 3a. 緊急處理     │    │ 3b. 排程處理     │
│  - 暫停部署      │    │  - 加入 Backlog  │
│  - 評估回滾      │    │  - 排定修復時程  │
│  - 通知團隊      │    └─────────────────┘
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 4. 修復實作      │ (依嚴重性 SLA)
│  - 更新套件      │
│  - 程式碼修補    │
│  - 測試驗證      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 5. 緊急部署      │
│  - Hot Fix PR    │
│  - 快速審查      │
│  - 生產部署      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 6. 事後檢討      │
│  - 撰寫事件報告  │ (Post-Mortem)
│  - 改進預防措施  │
│  - 更新文件      │
└─────────────────┘
```

### 6.2 聯絡資訊

| 角色 | 負責人 | 聯絡方式 | 備註 |
|-----|-------|---------|------|
| 安全性負責人 | (待指定) | security@contoso.edu | 主要聯絡窗口 |
| 後端技術負責人 | (待指定) | backend-lead@contoso.edu | .NET / Azure 架構 |
| 前端技術負責人 | (待指定) | frontend-lead@contoso.edu | React / Web 安全 |
| DevOps 負責人 | (待指定) | devops@contoso.edu | CI/CD / 部署 |

---

## 7. 參考資源

### 7.1 安全性最佳實踐

- [OWASP Top 10](https://owasp.org/www-project-top-ten/) - Web 應用程式安全風險
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/) - API 安全指南
- [Microsoft Security Development Lifecycle](https://www.microsoft.com/en-us/securityengineering/sdl/) - 微軟 SDL
- [CWE Top 25](https://cwe.mitre.org/top25/) - 常見軟體弱點

### 7.2 套件安全資源

- [NuGet Package Vulnerabilities](https://github.com/advisories?query=ecosystem%3Anuget) - GitHub Advisory Database
- [npm Security Advisories](https://github.com/advisories?query=ecosystem%3Anpm) - npm 漏洞通報
- [Snyk Vulnerability Database](https://security.snyk.io/) - 跨語言漏洞資料庫
- [CVE Database](https://cve.mitre.org/) - 官方 CVE 編號資料庫

### 7.3 Azure 安全文件

- [Azure Security Best Practices](https://learn.microsoft.com/azure/security/fundamentals/best-practices-and-patterns)
- [Azure Security Baseline](https://learn.microsoft.com/security/benchmark/azure/)
- [Microsoft Defender for Cloud Documentation](https://learn.microsoft.com/azure/defender-for-cloud/)

---

## 附錄 A: 套件安全性稽核範本

### 新增套件評估表

| 評估項目 | 檢查點 | 狀態 |
|---------|-------|------|
| **基本資訊** | 套件名稱、版本、授權 | [ ] |
| **來源可信度** | 官方/社群維護？GitHub Stars? | [ ] |
| **活躍度** | 最近更新時間？Issue 回應速度？ | [ ] |
| **安全紀錄** | 是否有 CVE 歷史？已修復？ | [ ] |
| **依賴分析** | 傳遞依賴數量？是否引入高風險套件？ | [ ] |
| **替代方案** | 是否有更安全/輕量的替代品？ | [ ] |
| **Bundle 影響** | 對前端 Bundle 大小影響？ | [ ] |
| **測試覆蓋** | 套件本身測試覆蓋率？ | [ ] |

**決策**: [ ] 核准使用 / [ ] 需進一步評估 / [ ] 拒絕使用

---

## 附錄 B: 常見安全問題與修復

### B.1 SQL Injection 預防

```csharp
// ❌ 錯誤：字串拼接
var query = $"SELECT * FROM Students WHERE LastName = '{lastName}'";
var students = context.Students.FromSqlRaw(query).ToList();

// ✅ 正確：參數化查詢
var students = context.Students
    .Where(s => s.LastName == lastName)
    .ToList();

// ✅ 正確：FromSqlRaw 搭配參數
var students = context.Students
    .FromSqlRaw("SELECT * FROM Students WHERE LastName = {0}", lastName)
    .ToList();
```

### B.2 XSS 預防

```typescript
// ❌ 錯誤：直接渲染使用者輸入
<div>{userComment}</div>  // 如果包含 <script>，React 會自動跳脫（安全）

// ❌ 錯誤：使用 dangerouslySetInnerHTML 無消毒
<div dangerouslySetInnerHTML={{ __html: userComment }} />

// ✅ 正確：使用 DOMPurify
import DOMPurify from 'dompurify';
<div dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(userComment) }} />
```

### B.3 路徑遍歷預防

```csharp
// ❌ 錯誤：直接使用使用者輸入的檔案名稱
var filePath = Path.Combine("Uploads", userFileName);

// ✅ 正確：驗證並清理檔案名稱
var safeFileName = Path.GetFileName(userFileName); // 移除路徑部分
var allowedExtensions = new[] { ".jpg", ".png", ".pdf" };
var extension = Path.GetExtension(safeFileName).ToLowerInvariant();

if (!allowedExtensions.Contains(extension))
    throw new InvalidOperationException("不允許的檔案類型");

var uniqueFileName = $"{Guid.NewGuid()}{extension}";
var filePath = Path.Combine("Uploads", uniqueFileName);
```

---

**文件版本**: 1.0  
**最後更新**: 2026-02-05  
**作者**: Contoso University 安全團隊  
**審核者**: (待指定)  
**下次審查日期**: 2026-05-05 (每季審查)
