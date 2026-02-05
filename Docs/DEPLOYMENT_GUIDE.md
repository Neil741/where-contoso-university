# Contoso University Azure 部署指南

## 📋 目錄

- [部署概述](#部署概述)
- [先決條件](#先決條件)
- [部署選項](#部署選項)
- [使用 ARM Template 部署](#使用-arm-template-部署)
- [使用 PowerShell 腳本部署](#使用-powershell-腳本部署)
- [使用 GitHub Actions CI/CD 部署](#使用-github-actions-cicd-部署)
- [手動部署步驟](#手動部署步驟)
- [設定 Azure 資源](#設定-azure-資源)
- [部署後驗證](#部署後驗證)
- [故障排除](#故障排除)
- [成本優化](#成本優化)
- [安全性最佳實踐](#安全性最佳實踐)

## 🌐 部署概述

Contoso University 可以部署到多種 Azure 服務：

### 推薦的部署架構

```
┌─────────────────────────────────────────────────────┐
│                  Azure 訂閱                          │
│                                                      │
│  ┌────────────────────────────────────────────┐    │
│  │           Resource Group                   │    │
│  │                                            │    │
│  │  ┌──────────────────┐  ┌───────────────┐  │    │
│  │  │  App Service     │  │  SQL Server   │  │    │
│  │  │  Plan (B1)       │  │  + Database   │  │    │
│  │  └────────┬─────────┘  └───────┬───────┘  │    │
│  │           │                     │          │    │
│  │  ┌────────▼─────────┐          │          │    │
│  │  │  Web App         │──────────┘          │    │
│  │  │  (ASP.NET Core)  │                     │    │
│  │  └──────────────────┘                     │    │
│  │                                            │    │
│  │  ┌──────────────────┐                     │    │
│  │  │  Application     │                     │    │
│  │  │  Insights        │                     │    │
│  │  │  (Optional)      │                     │    │
│  │  └──────────────────┘                     │    │
│  └────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────┘
```

### 部署的資源

| 資源類型 | 名稱 | SKU | 用途 |
|---------|------|-----|------|
| Resource Group | `contoso-university-rg` | N/A | 資源容器 |
| App Service Plan | `contoso-university-plan` | B1 | 應用程式主機計畫 |
| Web App | `contoso-university-app` | Linux/.NET 8 | Web 應用程式 |
| SQL Server | `contoso-university-sql` | 12.0 | 資料庫伺服器 |
| SQL Database | `ContosoUniversity` | Basic | 應用程式資料庫 |
| Application Insights | `contoso-university-ai` | N/A | 監控和分析 (選擇性) |

## 🔧 先決條件

### 必要工具

#### 1. Azure 訂閱
- 有效的 [Azure 訂閱](https://azure.microsoft.com/free/)
- 至少是 Contributor 角色權限

#### 2. Azure CLI
**安裝 Azure CLI**:

**Windows**:
```powershell
# 使用 Windows Installer
# 下載：https://aka.ms/installazurecliwindows

# 或使用 winget
winget install Microsoft.AzureCLI
```

**macOS**:
```bash
brew update && brew install azure-cli
```

**Linux (Ubuntu/Debian)**:
```bash
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

**驗證安裝**:
```bash
az --version
# 應顯示 2.50.0 或更高版本
```

**登入 Azure**:
```bash
az login

# 如果有多個訂閱，設定預設訂閱
az account list --output table
az account set --subscription "您的訂閱 ID"
```

#### 3. .NET 8.0 SDK
```bash
dotnet --version
# 應顯示 8.0.x
```

#### 4. PowerShell 7+（用於 PowerShell 腳本）
**Windows**:
```powershell
winget install Microsoft.PowerShell
```

**macOS/Linux**:
```bash
# 安裝說明：https://docs.microsoft.com/powershell/scripting/install/installing-powershell
```

#### 5. Git
```bash
git --version
```

### 選擇性工具

- **Visual Studio Code** with Azure Extensions
- **Azure Storage Explorer** (用於管理儲存體)
- **SQL Server Management Studio** 或 **Azure Data Studio**

### Azure 配額檢查

確保您的訂閱有足夠的配額：
```bash
# 檢查可用的位置
az account list-locations --output table

# 檢查 App Service 可用性
az appservice list-locations --sku B1 --output table

# 檢查 SQL Server 配額
az sql list-usages --location eastus --output table
```

## 🚀 部署選項

### 選項比較

| 部署方法 | 難度 | 自動化程度 | 適合場景 | 時間 |
|---------|------|-----------|---------|------|
| ARM Template | 中 | 高 | 一次性或初始設定 | ~10 分鐘 |
| PowerShell 腳本 | 中 | 高 | 自動化部署 | ~10 分鐘 |
| GitHub Actions | 低-中 | 最高 | CI/CD 持續部署 | ~15 分鐘（初次） |
| 手動部署 | 低 | 低 | 學習和測試 | ~20 分鐘 |
| Azure Portal | 最低 | 最低 | 快速原型 | ~15 分鐘 |

### 推薦選擇

- **生產環境**: GitHub Actions CI/CD
- **測試環境**: PowerShell 腳本或 ARM Template
- **開發/學習**: 手動部署或 Azure Portal

## 📄 使用 ARM Template 部署

### 什麼是 ARM Template？

Azure Resource Manager (ARM) Template 是 JSON 檔案，定義了要部署的基礎結構和設定。

### 準備 ARM Template

專案已包含 `Scripts/azure-deploy.json`，內容如下：

```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "appName": {
      "type": "string",
      "defaultValue": "[concat('contoso-', uniqueString(resourceGroup().id))]"
    },
    "sqlAdministratorPassword": {
      "type": "securestring"
    }
  },
  "resources": [
    // SQL Server, Database, App Service Plan, Web App
  ]
}
```

### 部署步驟

#### 1. 建立資源群組

```bash
# 設定變數
RESOURCE_GROUP="contoso-university-rg"
LOCATION="eastus"
APP_NAME="contoso-uni-$(openssl rand -hex 4)"

# 建立資源群組
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION
```

#### 2. 驗證 ARM Template

```bash
cd Scripts

# 驗證範本語法
az deployment group validate \
  --resource-group $RESOURCE_GROUP \
  --template-file azure-deploy.json \
  --parameters appName=$APP_NAME \
               sqlAdministratorPassword="YourStrongP@ssw0rd123!"
```

#### 3. 部署 ARM Template

```bash
# 部署資源
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file azure-deploy.json \
  --parameters appName=$APP_NAME \
               sqlAdministratorPassword="YourStrongP@ssw0rd123!" \
  --name ContosoUniversityDeployment

# 追蹤部署進度
az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name ContosoUniversityDeployment \
  --query properties.provisioningState
```

#### 4. 取得部署輸出

```bash
# 取得 Web App URL
az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name ContosoUniversityDeployment \
  --query properties.outputs.webAppUrl.value \
  --output tsv

# 取得 SQL Server 名稱
az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name ContosoUniversityDeployment \
  --query properties.outputs.sqlServerName.value \
  --output tsv
```

### What-If 部署（預覽變更）

```bash
# 檢查將會建立/修改的資源
az deployment group what-if \
  --resource-group $RESOURCE_GROUP \
  --template-file azure-deploy.json \
  --parameters appName=$APP_NAME \
               sqlAdministratorPassword="YourStrongP@ssw0rd123!"
```

## 🔧 使用 PowerShell 腳本部署

### PowerShell 腳本概述

`Scripts/Deploy-ToAzure.ps1` 是一個完整的部署自動化腳本。

### 使用方式

#### Windows PowerShell

```powershell
# 切換到專案根目錄
cd C:\path\to\where-contoso-university

# 執行部署腳本
.\Scripts\Deploy-ToAzure.ps1 `
  -ResourceGroupName "contoso-university-rg" `
  -Location "eastus" `
  -AppName "contoso-uni-prod"

# 腳本會提示輸入 SQL 密碼
```

#### PowerShell Core (跨平台)

```bash
# 切換到專案根目錄
cd /path/to/where-contoso-university

# 執行部署腳本
pwsh ./Scripts/Deploy-ToAzure.ps1 \
  -ResourceGroupName "contoso-university-rg" \
  -Location "eastus" \
  -AppName "contoso-uni-prod"
```

### 腳本參數

| 參數 | 必要 | 預設值 | 說明 |
|------|------|--------|------|
| ResourceGroupName | 是 | N/A | 資源群組名稱 |
| Location | 否 | eastus | Azure 區域 |
| AppName | 否 | 自動生成 | 應用程式基本名稱 |
| SqlAdminPassword | 否 | 提示輸入 | SQL 管理員密碼 |

### 腳本功能

腳本會自動執行：
1. ✅ 驗證 Azure CLI 已安裝
2. ✅ 檢查 Azure 登入狀態
3. ✅ 建立資源群組（如果不存在）
4. ✅ 部署 ARM Template
5. ✅ 建置 .NET 應用程式
6. ✅ 發佈應用程式到 Azure
7. ✅ 設定連線字串
8. ✅ 重啟 Web App
9. ✅ 顯示部署摘要和 URL

### 進階用法

**指定 SQL 密碼（避免提示）**:
```powershell
$securePassword = ConvertTo-SecureString "YourStrongP@ssw0rd123!" -AsPlainText -Force

.\Scripts\Deploy-ToAzure.ps1 `
  -ResourceGroupName "contoso-university-rg" `
  -SqlAdminPassword $securePassword
```

**從設定檔讀取參數**:
```powershell
# 建立 deploy-config.json
{
  "resourceGroupName": "contoso-university-rg",
  "location": "eastus",
  "appName": "contoso-uni-prod"
}

# 在腳本中使用
$config = Get-Content deploy-config.json | ConvertFrom-Json
.\Scripts\Deploy-ToAzure.ps1 @config
```

## 🔄 使用 GitHub Actions CI/CD 部署

### 設定 GitHub Actions

#### 1. 建立 Azure 服務主體

```bash
# 建立服務主體並給予 Contributor 權限
az ad sp create-for-rbac \
  --name "contoso-university-github" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/contoso-university-rg \
  --sdk-auth

# 輸出會類似：
{
  "clientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret": "xxxxx~xxxxxxxxxxxxxxxxxxxxxxxx",
  "subscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  ...
}
```

**重要**: 複製整個 JSON 輸出，您需要將它設定為 GitHub Secret。

#### 2. 設定 GitHub Secrets

前往 GitHub 專案：
1. Settings → Secrets and variables → Actions
2. 點擊 "New repository secret"
3. 新增以下 secrets：

| Secret 名稱 | 值 | 說明 |
|------------|---|------|
| `AZURE_CREDENTIALS` | 上一步的完整 JSON | Azure 服務主體憑證 |
| `AZURE_WEBAPP_NAME` | `contoso-uni-prod` | Web App 名稱 |
| `AZURE_SQL_CONNECTION_STRING` | `Server=tcp:...` | SQL 連線字串 |

#### 3. 建立 GitHub Actions 工作流程

建立 `.github/workflows/deploy-azure.yml`：

```yaml
name: Deploy to Azure

on:
  push:
    branches:
      - main
  workflow_dispatch:

env:
  AZURE_WEBAPP_NAME: contoso-uni-prod
  DOTNET_VERSION: '8.0.x'
  WORKING_DIRECTORY: './ContosoUniversity'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    # Checkout 程式碼
    - name: Checkout code
      uses: actions/checkout@v4
    
    # 設定 .NET
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    # 還原依賴
    - name: Restore dependencies
      run: dotnet restore
      working-directory: ${{ env.WORKING_DIRECTORY }}
    
    # 建置
    - name: Build
      run: dotnet build --configuration Release --no-restore
      working-directory: ${{ env.WORKING_DIRECTORY }}
    
    # 測試（如果有測試專案）
    - name: Test
      run: dotnet test --no-build --verbosity normal
      working-directory: ${{ env.WORKING_DIRECTORY }}
      continue-on-error: true
    
    # 發佈
    - name: Publish
      run: dotnet publish --configuration Release --output ./publish
      working-directory: ${{ env.WORKING_DIRECTORY }}
    
    # 登入 Azure
    - name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
    
    # 部署到 Azure Web App
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME }}
        package: ${{ env.WORKING_DIRECTORY }}/publish
    
    # 設定連線字串
    - name: Set Connection String
      uses: azure/appservice-settings@v1
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME }}
        connection-strings-json: |
          [
            {
              "name": "DefaultConnection",
              "value": "${{ secrets.AZURE_SQL_CONNECTION_STRING }}",
              "type": "SQLAzure",
              "slotSetting": false
            }
          ]
    
    # 登出
    - name: Azure Logout
      run: az logout
```

#### 4. 觸發部署

**自動觸發**:
```bash
git add .
git commit -m "Update application"
git push origin main
```

**手動觸發**:
- 前往 GitHub → Actions
- 選擇 "Deploy to Azure" 工作流程
- 點擊 "Run workflow"

### 進階 CI/CD 設定

#### 多環境部署

```yaml
on:
  push:
    branches:
      - main        # 部署到生產環境
      - develop     # 部署到開發環境

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - name: Determine environment
      id: env
      run: |
        if [[ "${{ github.ref }}" == "refs/heads/main" ]]; then
          echo "environment=production" >> $GITHUB_OUTPUT
          echo "webapp_name=contoso-uni-prod" >> $GITHUB_OUTPUT
        else
          echo "environment=development" >> $GITHUB_OUTPUT
          echo "webapp_name=contoso-uni-dev" >> $GITHUB_OUTPUT
        fi
    
    - name: Deploy to ${{ steps.env.outputs.environment }}
      uses: azure/webapps-deploy@v2
      with:
        app-name: ${{ steps.env.outputs.webapp_name }}
        package: ./publish
```

#### 藍綠部署（Deployment Slots）

```yaml
- name: Deploy to Staging Slot
  uses: azure/webapps-deploy@v2
  with:
    app-name: ${{ env.AZURE_WEBAPP_NAME }}
    slot-name: staging
    package: ./publish

- name: Swap Staging to Production
  run: |
    az webapp deployment slot swap \
      --resource-group ${{ env.RESOURCE_GROUP }} \
      --name ${{ env.AZURE_WEBAPP_NAME }} \
      --slot staging \
      --target-slot production
```

## 🔨 手動部署步驟

### 適合學習和理解部署流程

#### 步驟 1：建立資源群組

```bash
az group create \
  --name contoso-university-rg \
  --location eastus
```

#### 步驟 2：建立 SQL Server 和資料庫

```bash
# 設定變數
SQL_SERVER_NAME="contoso-sql-$(openssl rand -hex 4)"
SQL_ADMIN_USER="sqladmin"
SQL_ADMIN_PASSWORD="YourStrongP@ssw0rd123!"
DATABASE_NAME="ContosoUniversity"

# 建立 SQL Server
az sql server create \
  --resource-group contoso-university-rg \
  --name $SQL_SERVER_NAME \
  --location eastus \
  --admin-user $SQL_ADMIN_USER \
  --admin-password $SQL_ADMIN_PASSWORD

# 允許 Azure 服務存取
az sql server firewall-rule create \
  --resource-group contoso-university-rg \
  --server $SQL_SERVER_NAME \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# 建立資料庫
az sql db create \
  --resource-group contoso-university-rg \
  --server $SQL_SERVER_NAME \
  --name $DATABASE_NAME \
  --service-objective Basic
```

#### 步驟 3：建立 App Service Plan

```bash
az appservice plan create \
  --name contoso-university-plan \
  --resource-group contoso-university-rg \
  --location eastus \
  --sku B1 \
  --is-linux
```

#### 步驟 4：建立 Web App

```bash
WEB_APP_NAME="contoso-uni-$(openssl rand -hex 4)"

az webapp create \
  --resource-group contoso-university-rg \
  --plan contoso-university-plan \
  --name $WEB_APP_NAME \
  --runtime "DOTNET|8.0"
```

#### 步驟 5：設定連線字串

```bash
# 建構連線字串
CONNECTION_STRING="Server=tcp:${SQL_SERVER_NAME}.database.windows.net,1433;Database=${DATABASE_NAME};User ID=${SQL_ADMIN_USER};Password=${SQL_ADMIN_PASSWORD};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# 設定連線字串
az webapp config connection-string set \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="$CONNECTION_STRING"
```

#### 步驟 6：建置和發佈應用程式

```bash
# 切換到專案目錄
cd ContosoUniversity

# 發佈應用程式
dotnet publish --configuration Release --output ./publish

# 壓縮發佈檔案
cd publish
zip -r ../deploy.zip .
cd ..

# 部署到 Azure
az webapp deployment source config-zip \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --src deploy.zip
```

#### 步驟 7：驗證部署

```bash
# 開啟 Web App
az webapp browse \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME

# 或取得 URL
az webapp show \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --query defaultHostName \
  --output tsv
```

## ⚙️ 設定 Azure 資源

### App Service 設定

#### 應用程式設定

```bash
# 設定環境變數
az webapp config appsettings set \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    NotificationSettings__Enabled=false \
    Logging__LogLevel__Default=Information
```

#### 一般設定

```bash
# 啟用 HTTPS Only
az webapp update \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --https-only true

# 設定最小 TLS 版本
az webapp config set \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --min-tls-version 1.2

# 啟用 HTTP/2
az webapp config set \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --http20-enabled true
```

#### 持續部署

```bash
# 啟用從 GitHub 部署
az webapp deployment source config \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --repo-url https://github.com/your-org/where-contoso-university \
  --branch main \
  --manual-integration
```

### SQL Database 設定

#### 防火牆規則

```bash
# 新增您的 IP 位址
MY_IP=$(curl -s ifconfig.me)

az sql server firewall-rule create \
  --resource-group contoso-university-rg \
  --server $SQL_SERVER_NAME \
  --name AllowMyIP \
  --start-ip-address $MY_IP \
  --end-ip-address $MY_IP
```

#### 效能層級調整

```bash
# 升級到標準層
az sql db update \
  --resource-group contoso-university-rg \
  --server $SQL_SERVER_NAME \
  --name $DATABASE_NAME \
  --service-objective S0

# 啟用自動調整
az sql db update \
  --resource-group contoso-university-rg \
  --server $SQL_SERVER_NAME \
  --name $DATABASE_NAME \
  --auto-pause-delay 60 \
  --min-capacity 0.5
```

### Application Insights（選擇性）

#### 建立 Application Insights

```bash
# 建立 Application Insights
az monitor app-insights component create \
  --app contoso-university-ai \
  --location eastus \
  --resource-group contoso-university-rg

# 取得 Instrumentation Key
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
  --app contoso-university-ai \
  --resource-group contoso-university-rg \
  --query instrumentationKey \
  --output tsv)

# 設定到 Web App
az webapp config appsettings set \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --settings APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATION_KEY
```

### Azure Key Vault（預留）

#### 建立 Key Vault

```bash
# 建立 Key Vault
az keyvault create \
  --name contoso-uni-kv \
  --resource-group contoso-university-rg \
  --location eastus

# 儲存 SQL 密碼
az keyvault secret set \
  --vault-name contoso-uni-kv \
  --name SqlAdminPassword \
  --value "$SQL_ADMIN_PASSWORD"
```

#### 授予 Web App 存取權限

```bash
# 啟用 Web App 的受控識別
az webapp identity assign \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME

# 取得受控識別的 Object ID
IDENTITY_ID=$(az webapp identity show \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --query principalId \
  --output tsv)

# 授予 Key Vault 存取權限
az keyvault set-policy \
  --name contoso-uni-kv \
  --object-id $IDENTITY_ID \
  --secret-permissions get list
```

## ✅ 部署後驗證

### 驗證檢查清單

#### 1. Web App 可存取

```bash
# 檢查 Web App 狀態
az webapp show \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --query state \
  --output tsv
# 應該顯示: Running

# 測試 HTTP 回應
curl -I https://${WEB_APP_NAME}.azurewebsites.net
# 應該回傳: HTTP/1.1 200 OK
```

#### 2. 資料庫連線正常

```bash
# 查看應用程式日誌
az webapp log tail \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME

# 尋找以下訊息：
# - "Database initialized successfully"
# - 無 SqlException 錯誤
```

#### 3. 資料初始化成功

**開啟瀏覽器並驗證**:
```
https://${WEB_APP_NAME}.azurewebsites.net/Students
```

應該看到：
- 學生清單頁面
- 範例資料（如果已初始化）
- 分頁功能正常

#### 4. CRUD 操作測試

手動測試：
1. ✅ 建立新學生
2. ✅ 編輯學生資訊
3. ✅ 檢視學生詳情
4. ✅ 刪除學生
5. ✅ 搜尋和篩選功能

#### 5. 效能檢查

```bash
# 使用 Application Insights 檢查效能
az monitor app-insights metrics show \
  --app contoso-university-ai \
  --resource-group contoso-university-rg \
  --metric requests/duration

# 或使用 curl 測試回應時間
time curl https://${WEB_APP_NAME}.azurewebsites.net
```

### 自動化驗證腳本

建立 `verify-deployment.sh`:

```bash
#!/bin/bash

WEBAPP_URL="https://${WEB_APP_NAME}.azurewebsites.net"

echo "🔍 Verifying deployment..."

# 測試首頁
echo "Testing home page..."
STATUS=$(curl -s -o /dev/null -w "%{http_code}" $WEBAPP_URL)
if [ $STATUS -eq 200 ]; then
  echo "✅ Home page: OK"
else
  echo "❌ Home page: FAILED (Status: $STATUS)"
  exit 1
fi

# 測試學生頁面
echo "Testing students page..."
STATUS=$(curl -s -o /dev/null -w "%{http_code}" $WEBAPP_URL/Students)
if [ $STATUS -eq 200 ]; then
  echo "✅ Students page: OK"
else
  echo "❌ Students page: FAILED (Status: $STATUS)"
  exit 1
fi

# 測試課程頁面
echo "Testing courses page..."
STATUS=$(curl -s -o /dev/null -w "%{http_code}" $WEBAPP_URL/Courses)
if [ $STATUS -eq 200 ]; then
  echo "✅ Courses page: OK"
else
  echo "❌ Courses page: FAILED (Status: $STATUS)"
  exit 1
fi

echo "🎉 All verification tests passed!"
```

**執行驗證**:
```bash
chmod +x verify-deployment.sh
./verify-deployment.sh
```

## 🔧 故障排除

### 常見問題

#### 問題 1：部署失敗 - Zip 檔案錯誤

**症狀**:
```
Error: Failed to deploy web package to App Service.
```

**解決方案**:
```bash
# 確保在 publish 目錄內建立 zip
cd ContosoUniversity/publish
zip -r ../deploy.zip *
cd ..

# 然後部署
az webapp deployment source config-zip \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --src deploy.zip
```

#### 問題 2：無法連接到資料庫

**症狀**:
```
SqlException: Cannot open server 'xxx' requested by the login
```

**解決方案**:

**檢查防火牆規則**:
```bash
# 列出防火牆規則
az sql server firewall-rule list \
  --resource-group contoso-university-rg \
  --server $SQL_SERVER_NAME

# 新增 Azure 服務（如果缺少）
az sql server firewall-rule create \
  --resource-group contoso-university-rg \
  --server $SQL_SERVER_NAME \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

**驗證連線字串**:
```bash
# 檢查連線字串
az webapp config connection-string list \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME
```

#### 問題 3：應用程式啟動失敗

**症狀**: Web App 回傳 500 或 502 錯誤

**解決方案**:

**啟用詳細錯誤**:
```bash
az webapp config set \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --remote-debugging-enabled true

az webapp log config \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --application-logging filesystem \
  --detailed-error-messages true \
  --level information
```

**查看日誌**:
```bash
# 即時日誌
az webapp log tail \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME

# 下載日誌
az webapp log download \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --log-file logs.zip
```

#### 問題 4：效能問題

**症狀**: 應用程式回應緩慢

**解決方案**:

**檢查 App Service Plan**:
```bash
# 查看當前 SKU
az appservice plan show \
  --resource-group contoso-university-rg \
  --name contoso-university-plan \
  --query sku

# 升級到更高層級
az appservice plan update \
  --resource-group contoso-university-rg \
  --name contoso-university-plan \
  --sku S1
```

**啟用 Always On**:
```bash
az webapp config set \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --always-on true
```

#### 問題 5：資料庫效能問題

**症狀**: 資料庫查詢緩慢

**解決方案**:

**檢查 DTU 使用率**:
```bash
az sql db show \
  --resource-group contoso-university-rg \
  --server $SQL_SERVER_NAME \
  --name $DATABASE_NAME \
  --query currentServiceObjectiveName
```

**升級資料庫層級**:
```bash
az sql db update \
  --resource-group contoso-university-rg \
  --server $SQL_SERVER_NAME \
  --name $DATABASE_NAME \
  --service-objective S1
```

### 診斷工具

#### Kudu (SCM) 網站

存取 Kudu 診斷控制台：
```
https://${WEB_APP_NAME}.scm.azurewebsites.net
```

功能：
- 檔案系統瀏覽
- 即時日誌串流
- 程序總管
- 環境變數檢視

#### Azure Portal 診斷

前往 Azure Portal → Web App → 診斷並解決問題

可診斷：
- 應用程式效能
- 可用性問題
- 設定問題
- 資源使用率

## 💰 成本優化

### 預估月費用（美國東部區域）

| 資源 | SKU | 預估費用/月 |
|------|-----|-----------|
| App Service Plan (B1) | 1 核心, 1.75 GB RAM | ~$13 |
| SQL Database (Basic) | 2 GB | ~$5 |
| Application Insights | 5 GB/月免費 | $0-$2 |
| **總計** | | **~$18-$20** |

### 節省成本的策略

#### 1. 開發/測試環境使用免費層級

```bash
# 使用免費 F1 App Service Plan（有限制）
az appservice plan create \
  --name contoso-dev-plan \
  --resource-group contoso-dev-rg \
  --sku F1

# 使用 Azure SQL Database 免費優惠（12 個月）
# 或使用 SQLite for 開發環境
```

#### 2. 自動關閉開發環境

```bash
# 建立自動化腳本停止/啟動資源
# stop-dev-resources.sh
az webapp stop --resource-group contoso-dev-rg --name contoso-dev-app
az sql db pause --resource-group contoso-dev-rg --server dev-sql --name ContosoUniversity

# start-dev-resources.sh
az webapp start --resource-group contoso-dev-rg --name contoso-dev-app
az sql db resume --resource-group contoso-dev-rg --server dev-sql --name ContosoUniversity
```

#### 3. 使用 Azure 混合權益

如果您有現有的 Windows Server 授權：
```bash
az appservice plan update \
  --resource-group contoso-university-rg \
  --name contoso-university-plan \
  --hybrid \
  --yes
```

#### 4. 監控和警示

設定成本警示：
```bash
# 建立預算
az consumption budget create \
  --budget-name contoso-monthly-budget \
  --amount 50 \
  --time-grain Monthly \
  --start-date 2024-01-01 \
  --end-date 2025-01-01
```

## 🔒 安全性最佳實踐

### 1. 使用受控識別

**啟用系統指派的受控識別**:
```bash
az webapp identity assign \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME
```

**用於存取 Azure 資源**（如 Key Vault、Storage）:
```csharp
// 在程式碼中使用受控識別
var credential = new DefaultAzureCredential();
var client = new SecretClient(new Uri(keyVaultUrl), credential);
```

### 2. 加密敏感資料

**啟用 SQL Database 透明資料加密（預設啟用）**:
```bash
az sql db tde set \
  --resource-group contoso-university-rg \
  --server $SQL_SERVER_NAME \
  --database $DATABASE_NAME \
  --status Enabled
```

### 3. 網路隔離

**使用虛擬網路整合**（需要進階 SKU）:
```bash
# 建立虛擬網路
az network vnet create \
  --resource-group contoso-university-rg \
  --name contoso-vnet \
  --address-prefix 10.0.0.0/16 \
  --subnet-name app-subnet \
  --subnet-prefix 10.0.1.0/24

# 整合 Web App
az webapp vnet-integration add \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --vnet contoso-vnet \
  --subnet app-subnet
```

### 4. 定期更新

```bash
# 設定自動修補
az webapp config set \
  --resource-group contoso-university-rg \
  --name $WEB_APP_NAME \
  --auto-heal-enabled true
```

### 5. 安全性掃描

**使用 Azure Security Center 建議**:
- 前往 Azure Portal → Security Center
- 檢視建議並實作修正

## 📚 其他資源

### 官方文件
- [Azure App Service 文件](https://docs.microsoft.com/azure/app-service/)
- [Azure SQL Database 文件](https://docs.microsoft.com/azure/azure-sql/)
- [GitHub Actions for Azure](https://github.com/Azure/actions)

### 學習路徑
- [部署 .NET 應用程式到 Azure](https://docs.microsoft.com/learn/paths/deploy-dotnet-apps-to-azure/)
- [Azure 基礎知識](https://docs.microsoft.com/learn/paths/azure-fundamentals/)

### 工具
- [Azure CLI 參考](https://docs.microsoft.com/cli/azure/)
- [Azure Resource Manager 範本參考](https://docs.microsoft.com/azure/templates/)

---

**部署成功後，不要忘記慶祝！** 🎉

如有任何問題，請參考[故障排除](#故障排除)部分或開啟 GitHub Issue。
