# 部署與驗證規格

## 概述

本文件定義 Contoso University 前後端分離架構的部署策略、Azure 資源配置、CI/CD Pipeline 設計、環境管理、以及上線前的驗證準則。

---

## 1. Azure 資源架構

### 1.1 資源清單與規劃

#### 1.1.1 核心資源

| 資源類型 | Azure 服務 | SKU/層級 | 用途 | 月成本估算 (USD) |
|---------|-----------|---------|------|-----------------|
| **後端 API** | App Service (Linux) | B1 Basic (1 Core, 1.75GB) | .NET 8 Web API 託管 | ~$13 |
| **前端 Web** | Static Web Apps | Free/Standard | React SPA 託管 | Free / ~$9 |
| **資料庫** | Azure SQL Database | Basic (5 DTU) | 關聯式資料儲存 | ~$5 |
| **檔案儲存** | Blob Storage | Hot (LRS) | 教材檔案上傳 | ~$0.02/GB + 操作費 |
| **訊息佇列** | Service Bus | Basic | 通知系統 | ~$0.05 |
| **監控** | Application Insights | Pay-as-you-go | APM 與日誌 | ~$2-5 (視流量) |
| **祕密管理** | Key Vault | Standard | 連線字串儲存 | ~$0.03 |

**總計**: 開發/測試環境約 $25-30/月，生產環境可升級至 $100-150/月

#### 1.1.2 資源命名規範

遵循 [Azure 命名慣例](https://learn.microsoft.com/azure/cloud-adoption-framework/ready/azure-best-practices/resource-naming):

```
{resource-type}-{project}-{environment}-{region}-{instance}

範例：
- Resource Group:    rg-contoso-univ-prod-eastus-001
- App Service:       app-contoso-api-prod-eastus
- SQL Database:      sql-contoso-prod-eastus
- Storage Account:   stcontosoprodeastus (無破折號、小寫)
- Service Bus:       sb-contoso-prod-eastus
- Key Vault:         kv-contoso-prod-eastus
```

#### 1.1.3 資源群組設計

```
┌────────────────────────────────────────────────────────┐
│ rg-contoso-univ-shared-eastus                          │
│ (共享資源)                                              │
│  - Key Vault (所有環境共用)                             │
│  - Log Analytics Workspace                             │
└────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────┐
│ rg-contoso-univ-dev-eastus                             │
│ (開發環境)                                              │
│  - App Service (API)                                   │
│  - Static Web App (前端)                               │
│  - Azure SQL Database (Basic)                          │
│  - Storage Account                                     │
│  - Service Bus Namespace                               │
└────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────┐
│ rg-contoso-univ-staging-eastus                         │
│ (測試環境 - 與生產相同配置)                              │
└────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────┐
│ rg-contoso-univ-prod-eastus                            │
│ (生產環境)                                              │
│  - App Service (P1V3 或更高)                            │
│  - Static Web App (Standard)                           │
│  - Azure SQL Database (Standard S2+)                   │
│  - Storage Account (GRS 異地備援)                      │
│  - Service Bus Namespace (Standard)                    │
│  - Application Insights                                │
└────────────────────────────────────────────────────────┘
```

### 1.2 Azure SQL Database 配置

#### 1.2.1 連線字串配置

**開發環境** (LocalDB):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(LocalDb)\\MSSQLLocalDB;Database=ContosoUniversity;Integrated Security=True;MultipleActiveResultSets=True"
  }
}
```

**Azure 環境**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:sql-contoso-prod-eastus.database.windows.net,1433;Database=ContosoUniversityDB;User ID={username};Password={password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

**最佳實踐**:
- ✅ 使用 Azure Key Vault 儲存連線字串
- ✅ 啟用 Azure AD 驗證（生產環境）
- ✅ 配置防火牆規則僅允許 App Service IP
- ✅ 啟用自動備份（保留 7-35 天）
- ✅ 配置異地複寫（生產環境）

#### 1.2.2 防火牆規則配置

```bash
# 允許 Azure 服務存取
az sql server firewall-rule create \
  --resource-group rg-contoso-univ-prod-eastus \
  --server sql-contoso-prod-eastus \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# 允許特定 IP（開發機器）
az sql server firewall-rule create \
  --resource-group rg-contoso-univ-prod-eastus \
  --server sql-contoso-prod-eastus \
  --name AllowDevMachine \
  --start-ip-address 203.0.113.10 \
  --end-ip-address 203.0.113.10
```

### 1.3 App Service 配置

#### 1.3.1 應用程式設定

```json
{
  "ASPNETCORE_ENVIRONMENT": "Production",
  "ConnectionStrings__DefaultConnection": "@Microsoft.KeyVault(SecretUri=https://kv-contoso-prod-eastus.vault.azure.net/secrets/SqlConnectionString/)",
  "AzureBlobStorage__ConnectionString": "@Microsoft.KeyVault(...)",
  "AzureServiceBus__ConnectionString": "@Microsoft.KeyVault(...)",
  "APPINSIGHTS_INSTRUMENTATIONKEY": "{Application Insights Key}",
  "WEBSITE_TIME_ZONE": "Taipei Standard Time",
  "WEBSITE_RUN_FROM_PACKAGE": "1"
}
```

#### 1.3.2 Managed Identity 配置

```bash
# 啟用 System-assigned Managed Identity
az webapp identity assign \
  --name app-contoso-api-prod-eastus \
  --resource-group rg-contoso-univ-prod-eastus

# 授予 Key Vault 存取權限
az keyvault set-policy \
  --name kv-contoso-prod-eastus \
  --object-id {managed-identity-object-id} \
  --secret-permissions get list

# 授予 Blob Storage 存取權限
az role assignment create \
  --assignee {managed-identity-object-id} \
  --role "Storage Blob Data Contributor" \
  --scope /subscriptions/{subscription-id}/resourceGroups/rg-contoso-univ-prod-eastus/providers/Microsoft.Storage/storageAccounts/stcontosoprodeastus
```

### 1.4 Static Web Apps 配置

#### 1.4.1 靜態網站設定

```json
// staticwebapp.config.json
{
  "routes": [
    {
      "route": "/api/*",
      "methods": ["GET", "POST", "PUT", "DELETE"],
      "allowedRoles": ["authenticated"]
    },
    {
      "route": "/*",
      "serve": "/index.html",
      "statusCode": 200
    }
  ],
  "navigationFallback": {
    "rewrite": "/index.html",
    "exclude": ["/images/*.{png,jpg,gif}", "/css/*"]
  },
  "responseOverrides": {
    "404": {
      "rewrite": "/index.html",
      "statusCode": 200
    }
  },
  "globalHeaders": {
    "X-Content-Type-Options": "nosniff",
    "X-Frame-Options": "DENY",
    "Content-Security-Policy": "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';"
  }
}
```

#### 1.4.2 環境變數配置

```bash
# 前端環境變數（建置時注入）
VITE_API_BASE_URL=https://app-contoso-api-prod-eastus.azurewebsites.net
VITE_ENVIRONMENT=production
VITE_APP_INSIGHTS_KEY={instrumentation-key}
```

---

## 2. CI/CD Pipeline 設計

### 2.1 GitHub Actions 工作流程架構

```
┌──────────────────────────────────────────────────┐
│                   Git Push                        │
│            (feature/* or main branch)             │
└─────────────────────┬────────────────────────────┘
                      │
         ┌────────────┴────────────┐
         │                         │
         ▼                         ▼
┌─────────────────┐       ┌─────────────────┐
│  Pull Request   │       │  Push to Main   │
│  Workflow       │       │  Workflow       │
├─────────────────┤       ├─────────────────┤
│ 1. Lint         │       │ 1. Lint         │
│ 2. Build        │       │ 2. Build        │
│ 3. Test         │       │ 3. Test         │
│ 4. Security Scan│       │ 4. Security Scan│
│                 │       │ 5. Deploy Dev   │
└─────────────────┘       └────────┬────────┘
                                   │
                          ┌────────┴────────┐
                          │ Manual Approval │
                          │ (GitHub Env)    │
                          └────────┬────────┘
                                   │
                          ┌────────▼────────┐
                          │ Deploy Staging  │
                          └────────┬────────┘
                                   │
                          ┌────────▼────────┐
                          │ Smoke Tests     │
                          └────────┬────────┘
                                   │
                          ┌────────┴────────┐
                          │ Manual Approval │
                          └────────┬────────┘
                                   │
                          ┌────────▼────────┐
                          │ Deploy Prod     │
                          └─────────────────┘
```

### 2.2 後端 API CI/CD

#### 2.2.1 工作流程檔案

```yaml
# .github/workflows/backend-ci-cd.yml
name: Backend API CI/CD

on:
  push:
    branches: [main, develop]
    paths:
      - 'ContosoUniversity.Api/**'
      - '.github/workflows/backend-ci-cd.yml'
  pull_request:
    branches: [main]
    paths:
      - 'ContosoUniversity.Api/**'

env:
  DOTNET_VERSION: '8.0.x'
  AZURE_WEBAPP_NAME: app-contoso-api-prod-eastus

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      
      - name: Restore dependencies
        run: dotnet restore ./ContosoUniversity.Api
      
      - name: Build
        run: dotnet build ./ContosoUniversity.Api --configuration Release --no-restore
      
      - name: Run unit tests
        run: dotnet test ./ContosoUniversity.Api.Tests --configuration Release --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage.cobertura.xml
      
      - name: Security scan
        run: |
          dotnet list ./ContosoUniversity.Api package --vulnerable --include-transitive
          if [ $? -ne 0 ]; then exit 1; fi
      
      - name: Publish
        run: dotnet publish ./ContosoUniversity.Api -c Release -o ./publish
      
      - name: Upload artifact
        uses: actions/upload-artifact@v3
        with:
          name: api-artifact
          path: ./publish

  deploy-dev:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/develop'
    environment:
      name: development
      url: https://app-contoso-api-dev-eastus.azurewebsites.net
    
    steps:
      - name: Download artifact
        uses: actions/download-artifact@v3
        with:
          name: api-artifact
          path: ./publish
      
      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      
      - name: Deploy to Azure Web App
        uses: azure/webapps-deploy@v2
        with:
          app-name: app-contoso-api-dev-eastus
          package: ./publish
      
      - name: Run EF Migrations
        run: |
          dotnet tool install --global dotnet-ef
          dotnet ef database update --project ./ContosoUniversity.Api

  deploy-staging:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    environment:
      name: staging
      url: https://app-contoso-api-staging-eastus.azurewebsites.net
    
    steps:
      - name: Download artifact
        uses: actions/download-artifact@v3
        with:
          name: api-artifact
          path: ./publish
      
      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      
      - name: Deploy to Azure Web App
        uses: azure/webapps-deploy@v2
        with:
          app-name: app-contoso-api-staging-eastus
          package: ./publish
      
      - name: Smoke tests
        run: |
          curl -f https://app-contoso-api-staging-eastus.azurewebsites.net/health || exit 1

  deploy-production:
    needs: deploy-staging
    runs-on: ubuntu-latest
    environment:
      name: production
      url: https://app-contoso-api-prod-eastus.azurewebsites.net
    
    steps:
      - name: Download artifact
        uses: actions/download-artifact@v3
        with:
          name: api-artifact
          path: ./publish
      
      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      
      - name: Deploy to Azure Web App (Slot)
        uses: azure/webapps-deploy@v2
        with:
          app-name: ${{ env.AZURE_WEBAPP_NAME }}
          slot-name: staging
          package: ./publish
      
      - name: Swap Slots
        run: |
          az webapp deployment slot swap \
            --name ${{ env.AZURE_WEBAPP_NAME }} \
            --resource-group rg-contoso-univ-prod-eastus \
            --slot staging \
            --target-slot production
```

### 2.3 前端 React CI/CD

```yaml
# .github/workflows/frontend-ci-cd.yml
name: Frontend CI/CD

on:
  push:
    branches: [main, develop]
    paths:
      - 'ContosoUniversity.Web/**'
      - '.github/workflows/frontend-ci-cd.yml'
  pull_request:
    branches: [main]
    paths:
      - 'ContosoUniversity.Web/**'

env:
  NODE_VERSION: '20'

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'
          cache-dependency-path: ./ContosoUniversity.Web/package-lock.json
      
      - name: Install dependencies
        run: cd ./ContosoUniversity.Web && npm ci
      
      - name: Lint
        run: cd ./ContosoUniversity.Web && npm run lint
      
      - name: Type check
        run: cd ./ContosoUniversity.Web && npm run type-check
      
      - name: Run tests
        run: cd ./ContosoUniversity.Web && npm run test:ci
      
      - name: Security audit
        run: cd ./ContosoUniversity.Web && npm audit --audit-level=high
      
      - name: Build
        env:
          VITE_API_BASE_URL: ${{ secrets.VITE_API_BASE_URL_PROD }}
          VITE_APP_INSIGHTS_KEY: ${{ secrets.APP_INSIGHTS_KEY }}
        run: cd ./ContosoUniversity.Web && npm run build
      
      - name: Upload artifact
        uses: actions/upload-artifact@v3
        with:
          name: frontend-dist
          path: ./ContosoUniversity.Web/dist

  deploy-dev:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/develop'
    environment:
      name: development
      url: https://app-contoso-web-dev.azurestaticapps.net
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Download artifact
        uses: actions/download-artifact@v3
        with:
          name: frontend-dist
          path: ./dist
      
      - name: Deploy to Azure Static Web Apps
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN_DEV }}
          repo_token: ${{ secrets.GITHUB_TOKEN }}
          action: "upload"
          app_location: "./dist"
          skip_app_build: true

  deploy-production:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    environment:
      name: production
      url: https://app-contoso-web-prod.azurestaticapps.net
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Download artifact
        uses: actions/download-artifact@v3
        with:
          name: frontend-dist
          path: ./dist
      
      - name: Deploy to Azure Static Web Apps
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN_PROD }}
          repo_token: ${{ secrets.GITHUB_TOKEN }}
          action: "upload"
          app_location: "./dist"
          skip_app_build: true
```

### 2.4 資料庫遷移策略

#### 2.4.1 EF Core Migrations 在 CI/CD 中執行

**選項 A: 啟動時自動遷移**（開發/測試環境）

```csharp
// Program.cs
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<SchoolContext>();
    context.Database.Migrate();  // 自動執行待處理的遷移
}
```

**選項 B: 獨立遷移步驟**（生產環境推薦）

```yaml
# CI/CD Pipeline 中的遷移步驟
- name: Run Database Migrations
  run: |
    dotnet tool install --global dotnet-ef
    dotnet ef database update \
      --project ./ContosoUniversity.Api \
      --connection "${{ secrets.SQL_CONNECTION_STRING }}"
```

**選項 C: SQL Script 方式**（最安全，可審查）

```bash
# 生成 SQL 腳本
dotnet ef migrations script \
  --idempotent \
  --output migrations.sql \
  --project ./ContosoUniversity.Api

# 手動或透過 Azure CLI 執行
az sql db execute \
  --server sql-contoso-prod-eastus \
  --database ContosoUniversityDB \
  --file migrations.sql
```

---

## 3. 環境管理

### 3.1 環境定義與用途

| 環境 | 分支 | Azure 資源 | 用途 | 資料來源 | 存取權限 |
|-----|------|-----------|------|---------|---------|
| **Development** | `develop` | Dev RG | 開發整合測試 | 假資料 / 匿名化生產資料 | 所有開發者 |
| **Staging** | `main` | Staging RG | Pre-production 驗證 | 匿名化生產資料 | QA + 專案負責人 |
| **Production** | `main` (手動核准) | Prod RG | 正式環境 | 真實資料 | 僅運維團隊 |

### 3.2 環境變數管理

#### 3.2.1 GitHub Secrets 結構

```
Repository Secrets:
  - AZURE_CREDENTIALS (Service Principal JSON)
  - CODECOV_TOKEN

Environment Secrets (Development):
  - AZURE_STATIC_WEB_APPS_API_TOKEN_DEV
  - SQL_CONNECTION_STRING_DEV
  - VITE_API_BASE_URL_DEV

Environment Secrets (Staging):
  - SQL_CONNECTION_STRING_STAGING
  - VITE_API_BASE_URL_STAGING

Environment Secrets (Production):
  - AZURE_STATIC_WEB_APPS_API_TOKEN_PROD
  - SQL_CONNECTION_STRING_PROD
  - VITE_API_BASE_URL_PROD
  - APP_INSIGHTS_KEY
```

#### 3.2.2 Azure Key Vault 整合

```csharp
// Program.cs
if (!builder.Environment.IsDevelopment())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());  // 使用 Managed Identity
}
```

### 3.3 功能旗標 (Feature Flags)

使用 Azure App Configuration 或簡易的配置方式：

```json
// appsettings.json
{
  "FeatureFlags": {
    "EnableNotificationSystem": true,
    "EnableFileUpload": true,
    "EnableNewUI": false  // Staging 測試中
  }
}
```

```csharp
// 程式碼中使用
if (_configuration.GetValue<bool>("FeatureFlags:EnableNewUI"))
{
    // 新功能邏輯
}
```

---

## 4. 監控與告警

### 4.1 Application Insights 配置

#### 4.1.1 後端監控

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true;  // 高流量時採樣
    options.EnableQuickPulseMetricStream = true;  // Live Metrics
});

// 自訂遙測
builder.Services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();
```

#### 4.1.2 前端監控

```typescript
// src/lib/appInsights.ts
import { ApplicationInsights } from '@microsoft/applicationinsights-web';

const appInsights = new ApplicationInsights({
  config: {
    connectionString: import.meta.env.VITE_APP_INSIGHTS_CONNECTION_STRING,
    enableAutoRouteTracking: true,  // SPA 路由追蹤
    disableFetchTracking: false,    // HTTP 請求追蹤
    enableCorsCorrelation: true,    // 跨來源關聯
  }
});

appInsights.loadAppInsights();
export default appInsights;

// 使用範例
appInsights.trackEvent({ name: 'StudentCreated', properties: { studentId: '123' } });
```

### 4.2 健康檢查端點

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SchoolContext>()
    .AddAzureBlobStorage(
        builder.Configuration["AzureBlobStorage:ConnectionString"],
        name: "blob-storage")
    .AddAzureServiceBusTopic(
        builder.Configuration["AzureServiceBus:ConnectionString"],
        "notifications",
        name: "service-bus");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### 4.3 告警規則

#### 4.3.1 Azure Monitor 告警

```yaml
# 可用性告警
- alert: API 可用性低於 95%
  metric: availabilityResults/availabilityPercentage
  threshold: < 95
  window: 5 minutes
  action: Email + Teams

# 錯誤率告警
- alert: 伺服器錯誤率過高
  metric: requests/failed
  threshold: > 5% in 5 minutes
  action: Email + Teams + PagerDuty

# 效能告警
- alert: API 回應時間過長
  metric: requests/duration
  threshold: P95 > 2000ms
  window: 10 minutes
  action: Email

# 資料庫 DTU 告警
- alert: SQL Database DTU 使用率高
  metric: dtu_consumption_percent
  threshold: > 80%
  window: 5 minutes
  action: Email
```

### 4.4 日誌查詢範例

```kql
// Application Insights KQL 查詢

// 1. 過去 24 小時的錯誤統計
exceptions
| where timestamp > ago(24h)
| summarize Count = count() by type, outerMessage
| order by Count desc

// 2. 最慢的 API 端點
requests
| where timestamp > ago(1h)
| summarize AvgDuration = avg(duration), Count = count() by name
| order by AvgDuration desc
| take 10

// 3. 使用者操作漏斗分析
customEvents
| where timestamp > ago(7d)
| where name in ("PageView_Students", "Click_CreateStudent", "Submit_StudentForm", "Success_StudentCreated")
| summarize Count = count() by name
| order by Count desc
```

---

## 5. 災難復原計劃

### 5.1 備份策略

#### 5.1.1 Azure SQL Database

```bash
# 自動備份（預設已啟用）
- Full Backup: 每週
- Differential Backup: 每 12 小時
- Log Backup: 每 5-10 分鐘
- 保留期: Basic (7天), Standard (35天)

# 長期保留 (LTR)
az sql db ltr-policy set \
  --resource-group rg-contoso-univ-prod-eastus \
  --server sql-contoso-prod-eastus \
  --database ContosoUniversityDB \
  --weekly-retention P4W \
  --monthly-retention P12M \
  --yearly-retention P5Y \
  --week-of-year 1
```

#### 5.1.2 Blob Storage

```bash
# 啟用 Soft Delete（誤刪復原）
az storage blob service-properties delete-policy update \
  --account-name stcontosoprodeastus \
  --enable true \
  --days-retained 30

# 啟用版本控制
az storage account blob-service-properties update \
  --account-name stcontosoprodeastus \
  --enable-versioning true
```

### 5.2 災難復原演練

#### 5.2.1 RTO/RPO 目標

| 服務 | RTO (Recovery Time Objective) | RPO (Recovery Point Objective) | 策略 |
|-----|-------------------------------|-------------------------------|------|
| API | < 1 小時 | < 5 分鐘 | 重新部署 + 資料庫還原 |
| 前端 | < 30 分鐘 | 0（靜態檔案） | 重新部署 |
| 資料庫 | < 2 小時 | < 5 分鐘 | Point-in-time restore |
| Blob Storage | < 1 小時 | < 15 分鐘 | 異地備援 (GRS) |

#### 5.2.2 復原步驟

```bash
# 1. 還原資料庫到特定時間點
az sql db restore \
  --dest-name ContosoUniversityDB-Restored \
  --resource-group rg-contoso-univ-prod-eastus \
  --server sql-contoso-prod-eastus \
  --source-database ContosoUniversityDB \
  --time "2026-02-05T10:00:00Z"

# 2. 更新連線字串指向還原的資料庫

# 3. 重新部署應用程式（觸發 CI/CD）

# 4. 驗證功能正常

# 5. 切換 DNS 或 Traffic Manager（如使用）
```

---

## 6. 上線前驗證準則

### 6.1 功能驗收清單

#### 6.1.1 核心功能驗證

- [ ] **學生管理**
  - [ ] 建立學生資料成功
  - [ ] 列表分頁與搜尋正常
  - [ ] 編輯學生資料成功
  - [ ] 刪除學生資料成功（含確認對話框）
  - [ ] 檢視學生詳情與註冊課程

- [ ] **課程管理**
  - [ ] 建立課程成功
  - [ ] 教材上傳到 Azure Blob 成功
  - [ ] 教材下載連結有效（SAS Token）
  - [ ] 課程編輯與刪除正常

- [ ] **講師管理**
  - [ ] 建立講師資料成功
  - [ ] 辦公室分配功能正常
  - [ ] 課程指派功能正常
  - [ ] 多對多關係顯示正確

- [ ] **部門管理**
  - [ ] 建立部門成功
  - [ ] 並行控制測試（RowVersion）
  - [ ] 預算與主任欄位驗證

- [ ] **通知系統**
  - [ ] CRUD 操作觸發通知
  - [ ] SignalR 即時推送到前端
  - [ ] 通知列表顯示正確
  - [ ] 標記已讀功能正常

- [ ] **待辦事項**
  - [ ] 建立待辦事項成功
  - [ ] 標記完成功能正常
  - [ ] 列表過濾正常

- [ ] **統計報表**
  - [ ] 註冊日期統計圖表正確
  - [ ] 資料與資料庫一致

### 6.2 非功能性驗證

#### 6.2.1 效能測試

```bash
# 使用 Apache Bench 進行簡單負載測試
ab -n 1000 -c 10 https://app-contoso-api-prod-eastus.azurewebsites.net/api/students

# 或使用 Azure Load Testing
az load test create \
  --name contoso-load-test \
  --resource-group rg-contoso-univ-prod-eastus \
  --test-plan load-test-plan.jmx
```

**驗收標準**:
- [ ] API P95 回應時間 < 500ms
- [ ] API P99 回應時間 < 1000ms
- [ ] 前端 First Contentful Paint < 1.5s
- [ ] 前端 Time to Interactive < 3s
- [ ] Lighthouse Performance Score > 90

#### 6.2.2 安全性測試

- [ ] **HTTPS 強制**
  - [ ] HTTP 自動重導向到 HTTPS
  - [ ] SSL 憑證有效且信任

- [ ] **CORS 設定**
  - [ ] 僅允許已知前端來源
  - [ ] OPTIONS preflight 請求正常

- [ ] **驗證與授權**（如已實作）
  - [ ] 未授權請求返回 401
  - [ ] 權限不足返回 403
  - [ ] JWT Token 過期自動更新

- [ ] **輸入驗證**
  - [ ] SQL Injection 測試通過
  - [ ] XSS 測試通過
  - [ ] 檔案上傳限制有效（大小、類型）

- [ ] **依賴漏洞**
  - [ ] npm audit 無 High/Critical 漏洞
  - [ ] dotnet list package --vulnerable 通過

#### 6.2.3 相容性測試

- [ ] **瀏覽器相容性**
  - [ ] Chrome (最新版)
  - [ ] Firefox (最新版)
  - [ ] Safari (最新版)
  - [ ] Edge (最新版)

- [ ] **響應式設計**
  - [ ] Desktop (1920x1080)
  - [ ] Tablet (768x1024)
  - [ ] Mobile (375x667)

### 6.3 運維就緒檢查

- [ ] **監控配置**
  - [ ] Application Insights 正常收集遙測
  - [ ] 告警規則已設定並測試
  - [ ] Dashboard 顯示關鍵指標

- [ ] **日誌管理**
  - [ ] 結構化日誌正確輸出
  - [ ] 敏感資訊已遮罩
  - [ ] Log Analytics 查詢測試通過

- [ ] **備份驗證**
  - [ ] 資料庫自動備份運行中
  - [ ] 備份還原測試成功
  - [ ] Blob Storage 異地備援啟用

- [ ] **文件完整性**
  - [ ] API 文件（Swagger）完整
  - [ ] 部署手冊（Runbook）完成
  - [ ] 故障排除指南完成
  - [ ] 使用者手冊完成

---

## 7. 上線流程

### 7.1 上線檢查清單

```markdown
## 上線前 (D-7)

- [ ] 完成所有功能驗收測試
- [ ] 完成效能與安全性測試
- [ ] 完成 UAT (使用者驗收測試)
- [ ] 備份當前生產資料庫（如有舊系統）
- [ ] 準備回滾計劃

## 上線前 (D-3)

- [ ] 凍結程式碼變更（Code Freeze）
- [ ] 部署到 Staging 環境最後驗證
- [ ] 確認 DNS/CDN 配置就緒
- [ ] 通知使用者維護時間

## 上線當天 (D-Day)

- [ ] T-1h: 團隊集合，確認角色與職責
- [ ] T-30m: 最終備份生產資料庫
- [ ] T-15m: 開始維護模式（顯示維護頁面）
- [ ] T0: 執行資料庫遷移
- [ ] T+10m: 部署後端 API
- [ ] T+15m: 部署前端應用
- [ ] T+20m: 煙霧測試（Smoke Test）
- [ ] T+30m: 開放使用者存取
- [ ] T+1h: 監控系統穩定性
- [ ] T+4h: 團隊待命結束（如無問題）

## 上線後 (D+1)

- [ ] 檢查過去 24 小時錯誤日誌
- [ ] 確認監控告警正常
- [ ] 收集使用者反饋
- [ ] 撰寫上線報告
```

### 7.2 回滾計劃

**觸發條件**:
- Critical Bug 導致系統無法使用
- 資料遺失或損毀
- 效能嚴重劣化（回應時間 > 5秒）
- 安全性漏洞被發現

**回滾步驟**:
```bash
# 1. 後端回滾（使用 App Service Slot Swap）
az webapp deployment slot swap \
  --name app-contoso-api-prod-eastus \
  --resource-group rg-contoso-univ-prod-eastus \
  --slot production \
  --target-slot staging

# 2. 前端回滾（重新部署前一版本）
cd ContosoUniversity.Web
git checkout {previous-commit-sha}
npm run build
# 部署到 Static Web App

# 3. 資料庫回滾（如有破壞性變更）
az sql db restore \
  --dest-name ContosoUniversityDB \
  --resource-group rg-contoso-univ-prod-eastus \
  --server sql-contoso-prod-eastus \
  --source-database ContosoUniversityDB-Backup \
  --time {backup-time}
```

---

## 8. 參考資源

### 8.1 Azure 部署文件

- [App Service Deployment Best Practices](https://learn.microsoft.com/azure/app-service/deploy-best-practices)
- [Azure SQL Database Backup](https://learn.microsoft.com/azure/azure-sql/database/automated-backups-overview)
- [Static Web Apps Configuration](https://learn.microsoft.com/azure/static-web-apps/configuration)

### 8.2 CI/CD 最佳實踐

- [GitHub Actions for Azure](https://github.com/Azure/actions)
- [Deployment Strategies](https://learn.microsoft.com/azure/architecture/guide/deployment-strategies)
- [Infrastructure as Code with Bicep](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)

### 8.3 監控與告警

- [Application Insights Best Practices](https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview)
- [Kusto Query Language (KQL) Tutorial](https://learn.microsoft.com/azure/data-explorer/kusto/query/)

---

## 附錄 A: Azure CLI 快速指令

```bash
# 建立資源群組
az group create --name rg-contoso-univ-prod-eastus --location eastus

# 建立 App Service Plan (Linux)
az appservice plan create \
  --name plan-contoso-prod-eastus \
  --resource-group rg-contoso-univ-prod-eastus \
  --sku B1 \
  --is-linux

# 建立 Web App (.NET 8)
az webapp create \
  --name app-contoso-api-prod-eastus \
  --resource-group rg-contoso-univ-prod-eastus \
  --plan plan-contoso-prod-eastus \
  --runtime "DOTNET|8.0"

# 建立 Azure SQL Database
az sql server create \
  --name sql-contoso-prod-eastus \
  --resource-group rg-contoso-univ-prod-eastus \
  --location eastus \
  --admin-user sqladmin \
  --admin-password {strong-password}

az sql db create \
  --name ContosoUniversityDB \
  --resource-group rg-contoso-univ-prod-eastus \
  --server sql-contoso-prod-eastus \
  --service-objective S0

# 建立 Storage Account
az storage account create \
  --name stcontosoprodeastus \
  --resource-group rg-contoso-univ-prod-eastus \
  --location eastus \
  --sku Standard_LRS

# 建立 Service Bus Namespace
az servicebus namespace create \
  --name sb-contoso-prod-eastus \
  --resource-group rg-contoso-univ-prod-eastus \
  --location eastus \
  --sku Basic

az servicebus queue create \
  --name contoso-notifications \
  --namespace-name sb-contoso-prod-eastus \
  --resource-group rg-contoso-univ-prod-eastus

# 建立 Key Vault
az keyvault create \
  --name kv-contoso-prod-eastus \
  --resource-group rg-contoso-univ-prod-eastus \
  --location eastus
```

---

## 附錄 B: 煙霧測試腳本

```bash
#!/bin/bash
# smoke-test.sh

API_BASE_URL="https://app-contoso-api-prod-eastus.azurewebsites.net"

echo "Running smoke tests..."

# 1. Health Check
echo "1. Testing /health endpoint..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" ${API_BASE_URL}/health)
if [ $HTTP_CODE -eq 200 ]; then
  echo "✅ Health check passed"
else
  echo "❌ Health check failed (HTTP $HTTP_CODE)"
  exit 1
fi

# 2. API 端點測試
echo "2. Testing /api/students endpoint..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" ${API_BASE_URL}/api/students?page=1&pageSize=10)
if [ $HTTP_CODE -eq 200 ]; then
  echo "✅ Students API passed"
else
  echo "❌ Students API failed (HTTP $HTTP_CODE)"
  exit 1
fi

# 3. Swagger 文件
echo "3. Testing Swagger UI..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" ${API_BASE_URL}/swagger/index.html)
if [ $HTTP_CODE -eq 200 ]; then
  echo "✅ Swagger UI accessible"
else
  echo "❌ Swagger UI failed (HTTP $HTTP_CODE)"
  exit 1
fi

echo "✅ All smoke tests passed!"
```

---

**文件版本**: 1.0  
**最後更新**: 2026-02-05  
**作者**: Contoso University DevOps 團隊  
**審核者**: (待指定)  
**下次審查日期**: 2026-03-05
