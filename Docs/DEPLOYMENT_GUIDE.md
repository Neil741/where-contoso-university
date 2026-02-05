# Contoso University - Azure 部署指南

本指南說明如何將 Contoso University 應用程式部署到 Azure App Service。

## 目錄

1. [前置需求](#前置需求)
2. [部署選項](#部署選項)
3. [手動部署](#手動部署)
4. [PowerShell 自動部署](#powershell-自動部署)
5. [GitHub Actions CI/CD](#github-actions-cicd)
6. [ARM Template 部署](#arm-template-部署)
7. [配置管理](#配置管理)
8. [監控與故障排除](#監控與故障排除)

## 前置需求

### 必需工具
- Azure 訂閱
- Azure CLI (`az`) 或 Azure PowerShell
- .NET 8.0 SDK
- Git

### 安裝 Azure CLI

**Windows** (PowerShell):
```powershell
Invoke-WebRequest -Uri https://aka.ms/installazurecliwindows -OutFile .\AzureCLI.msi
Start-Process msiexec.exe -Wait -ArgumentList '/I AzureCLI.msi /quiet'
```

**Linux/Mac**:
```bash
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

### 登入 Azure

```bash
az login
az account set --subscription "your-subscription-id"
```

## 部署選項

| 選項 | 複雜度 | 自動化 | 建議用途 |
|------|--------|--------|----------|
| Azure Portal | 低 | 無 | 首次部署、學習 |
| Azure CLI | 中 | 部分 | 快速部署、測試 |
| PowerShell Script | 中 | 高 | 重複部署 |
| ARM Template | 高 | 高 | 基礎設施即代碼 |
| GitHub Actions | 中 | 完全 | 生產 CI/CD |

## 手動部署

### 步驟 1: 建立資源群組

```bash
az group create \
  --name ContosoUniversity-RG \
  --location eastus
```

### 步驟 2: 建立 SQL Server 和資料庫

```bash
# 建立 SQL Server
az sql server create \
  --name contosouniversity-sql \
  --resource-group ContosoUniversity-RG \
  --location eastus \
  --admin-user sqladmin \
  --admin-password 'YourStrong@Password123'

# 建立資料庫
az sql db create \
  --resource-group ContosoUniversity-RG \
  --server contosouniversity-sql \
  --name ContosoUniversityDB \
  --service-objective S0 \
  --max-size 2GB

# 配置防火牆規則（允許 Azure 服務）
az sql server firewall-rule create \
  --resource-group ContosoUniversity-RG \
  --server contosouniversity-sql \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

### 步驟 3: 建立 App Service 計劃

```bash
az appservice plan create \
  --name ContosoUniversity-ASP \
  --resource-group ContosoUniversity-RG \
  --location eastus \
  --sku B1 \
  --is-linux
```

### 步驟 4: 建立 Web App

```bash
az webapp create \
  --name contosouniversity-app \
  --resource-group ContosoUniversity-RG \
  --plan ContosoUniversity-ASP \
  --runtime "DOTNET|8.0"
```

### 步驟 5: 配置連接字串

```bash
az webapp config connection-string set \
  --name contosouniversity-app \
  --resource-group ContosoUniversity-RG \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:contosouniversity-sql.database.windows.net,1433;Database=ContosoUniversityDB;User ID=sqladmin;Password=YourStrong@Password123;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### 步驟 6: 部署應用程式

```bash
cd ContosoUniversity

# 發佈應用
dotnet publish -c Release -o ./publish

# 壓縮檔案
cd publish
zip -r ../app.zip .
cd ..

# 部署
az webapp deployment source config-zip \
  --resource-group ContosoUniversity-RG \
  --name contosouniversity-app \
  --src app.zip
```

### 步驟 7: 驗證部署

訪問: `https://contosouniversity-app.azurewebsites.net`

## PowerShell 自動部署

創建 `Scripts/deploy-to-azure.ps1`:

```powershell
# Azure 部署腳本
param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$Location = "eastus",
    
    [Parameter(Mandatory=$true)]
    [string]$SqlAdminPassword
)

# 設置變數
$SqlServerName = "$ResourceGroupName-sql".ToLower()
$DatabaseName = "ContosoUniversityDB"
$AppServicePlanName = "$ResourceGroupName-ASP"
$WebAppName = "$ResourceGroupName-app".ToLower()
$SqlAdminUser = "sqladmin"

Write-Host "開始部署 Contoso University..." -ForegroundColor Green

# 1. 建立資源群組
Write-Host "建立資源群組..." -ForegroundColor Yellow
az group create --name $ResourceGroupName --location $Location

# 2. 建立 SQL Server
Write-Host "建立 SQL Server..." -ForegroundColor Yellow
az sql server create `
    --name $SqlServerName `
    --resource-group $ResourceGroupName `
    --location $Location `
    --admin-user $SqlAdminUser `
    --admin-password $SqlAdminPassword

# 3. 建立資料庫
Write-Host "建立資料庫..." -ForegroundColor Yellow
az sql db create `
    --resource-group $ResourceGroupName `
    --server $SqlServerName `
    --name $DatabaseName `
    --service-objective S0 `
    --max-size 2GB

# 4. 配置防火牆
Write-Host "配置防火牆規則..." -ForegroundColor Yellow
az sql server firewall-rule create `
    --resource-group $ResourceGroupName `
    --server $SqlServerName `
    --name AllowAzureServices `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0

# 5. 建立 App Service 計劃
Write-Host "建立 App Service 計劃..." -ForegroundColor Yellow
az appservice plan create `
    --name $AppServicePlanName `
    --resource-group $ResourceGroupName `
    --location $Location `
    --sku B1 `
    --is-linux

# 6. 建立 Web App
Write-Host "建立 Web App..." -ForegroundColor Yellow
az webapp create `
    --name $WebAppName `
    --resource-group $ResourceGroupName `
    --plan $AppServicePlanName `
    --runtime "DOTNET|8.0"

# 7. 配置連接字串
Write-Host "配置連接字串..." -ForegroundColor Yellow
$ConnectionString = "Server=tcp:$SqlServerName.database.windows.net,1433;Database=$DatabaseName;User ID=$SqlAdminUser;Password=$SqlAdminPassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az webapp config connection-string set `
    --name $WebAppName `
    --resource-group $ResourceGroupName `
    --connection-string-type SQLAzure `
    --settings DefaultConnection=$ConnectionString

# 8. 啟用 HTTPS
Write-Host "啟用 HTTPS..." -ForegroundColor Yellow
az webapp update `
    --name $WebAppName `
    --resource-group $ResourceGroupName `
    --https-only true

# 9. 發佈應用
Write-Host "發佈應用程式..." -ForegroundColor Yellow
Push-Location ContosoUniversity
dotnet publish -c Release -o ./publish

# 壓縮檔案
Compress-Archive -Path ./publish/* -DestinationPath ./app.zip -Force

# 部署
az webapp deployment source config-zip `
    --resource-group $ResourceGroupName `
    --name $WebAppName `
    --src ./app.zip

# 清理
Remove-Item -Path ./publish -Recurse -Force
Remove-Item -Path ./app.zip -Force
Pop-Location

Write-Host "部署完成！" -ForegroundColor Green
Write-Host "應用 URL: https://$WebAppName.azurewebsites.net" -ForegroundColor Cyan
```

### 使用腳本

```powershell
.\Scripts\deploy-to-azure.ps1 `
    -ResourceGroupName "ContosoUniversity-RG" `
    -Location "eastus" `
    -SqlAdminPassword "YourStrong@Password123"
```

## GitHub Actions CI/CD

### 步驟 1: 創建 Publish Profile

1. 前往 Azure Portal
2. 選擇你的 Web App
3. 點擊「取得發佈設定檔」
4. 下載並複製內容

### 步驟 2: 設置 GitHub Secret

1. 前往 GitHub Repository Settings
2. Secrets and variables > Actions
3. 新增 Secret: `AZURE_WEBAPP_PUBLISH_PROFILE`
4. 貼上 Publish Profile 內容

### 步驟 3: 創建 Workflow

創建 `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Azure App Service

on:
  push:
    branches: [ main ]
  workflow_dispatch:

env:
  AZURE_WEBAPP_NAME: contosouniversity-app
  DOTNET_VERSION: '8.0.x'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Restore dependencies
      run: dotnet restore ContosoUniversity/ContosoUniversity.csproj
    
    - name: Build
      run: dotnet build ContosoUniversity/ContosoUniversity.csproj --configuration Release --no-restore
    
    - name: Publish
      run: dotnet publish ContosoUniversity/ContosoUniversity.csproj -c Release -o ./publish
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v3
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME }}
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

## ARM Template 部署

創建 `Scripts/arm-template.json`:

```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "sqlAdministratorLogin": {
      "type": "string",
      "metadata": {
        "description": "SQL Server 管理員帳號"
      }
    },
    "sqlAdministratorPassword": {
      "type": "securestring",
      "metadata": {
        "description": "SQL Server 管理員密碼"
      }
    },
    "location": {
      "type": "string",
      "defaultValue": "[resourceGroup().location]",
      "metadata": {
        "description": "資源位置"
      }
    }
  },
  "variables": {
    "sqlServerName": "[concat('contosouniversity-sql-', uniqueString(resourceGroup().id))]",
    "databaseName": "ContosoUniversityDB",
    "appServicePlanName": "ContosoUniversity-ASP",
    "webAppName": "[concat('contosouniversity-app-', uniqueString(resourceGroup().id))]"
  },
  "resources": [
    {
      "type": "Microsoft.Sql/servers",
      "apiVersion": "2022-05-01-preview",
      "name": "[variables('sqlServerName')]",
      "location": "[parameters('location')]",
      "properties": {
        "administratorLogin": "[parameters('sqlAdministratorLogin')]",
        "administratorLoginPassword": "[parameters('sqlAdministratorPassword')]",
        "version": "12.0"
      }
    },
    {
      "type": "Microsoft.Sql/servers/databases",
      "apiVersion": "2022-05-01-preview",
      "name": "[concat(variables('sqlServerName'), '/', variables('databaseName'))]",
      "location": "[parameters('location')]",
      "dependsOn": [
        "[resourceId('Microsoft.Sql/servers', variables('sqlServerName'))]"
      ],
      "sku": {
        "name": "S0",
        "tier": "Standard"
      },
      "properties": {
        "maxSizeBytes": 2147483648
      }
    },
    {
      "type": "Microsoft.Sql/servers/firewallRules",
      "apiVersion": "2022-05-01-preview",
      "name": "[concat(variables('sqlServerName'), '/AllowAzureServices')]",
      "dependsOn": [
        "[resourceId('Microsoft.Sql/servers', variables('sqlServerName'))]"
      ],
      "properties": {
        "startIpAddress": "0.0.0.0",
        "endIpAddress": "0.0.0.0"
      }
    },
    {
      "type": "Microsoft.Web/serverfarms",
      "apiVersion": "2022-03-01",
      "name": "[variables('appServicePlanName')]",
      "location": "[parameters('location')]",
      "sku": {
        "name": "B1",
        "tier": "Basic"
      },
      "kind": "linux",
      "properties": {
        "reserved": true
      }
    },
    {
      "type": "Microsoft.Web/sites",
      "apiVersion": "2022-03-01",
      "name": "[variables('webAppName')]",
      "location": "[parameters('location')]",
      "dependsOn": [
        "[resourceId('Microsoft.Web/serverfarms', variables('appServicePlanName'))]",
        "[resourceId('Microsoft.Sql/servers/databases', variables('sqlServerName'), variables('databaseName'))]"
      ],
      "properties": {
        "serverFarmId": "[resourceId('Microsoft.Web/serverfarms', variables('appServicePlanName'))]",
        "siteConfig": {
          "linuxFxVersion": "DOTNETCORE|8.0",
          "connectionStrings": [
            {
              "name": "DefaultConnection",
              "connectionString": "[concat('Server=tcp:', reference(resourceId('Microsoft.Sql/servers', variables('sqlServerName'))).fullyQualifiedDomainName, ',1433;Database=', variables('databaseName'), ';User ID=', parameters('sqlAdministratorLogin'), ';Password=', parameters('sqlAdministratorPassword'), ';Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;')]",
              "type": "SQLAzure"
            }
          ]
        },
        "httpsOnly": true
      }
    }
  ],
  "outputs": {
    "webAppUrl": {
      "type": "string",
      "value": "[concat('https://', reference(resourceId('Microsoft.Web/sites', variables('webAppName'))).defaultHostName)]"
    }
  }
}
```

### 使用 ARM Template

```bash
az deployment group create \
  --resource-group ContosoUniversity-RG \
  --template-file Scripts/arm-template.json \
  --parameters sqlAdministratorLogin=sqladmin sqlAdministratorPassword='YourStrong@Password123'
```

## 配置管理

### 環境變數

在 Azure Portal 或使用 CLI 設置:

```bash
az webapp config appsettings set \
  --name contosouniversity-app \
  --resource-group ContosoUniversity-RG \
  --settings ASPNETCORE_ENVIRONMENT=Production
```

### 應用程式設置

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "從 Azure 配置"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Azure Key Vault（推薦）

對於敏感資料，使用 Azure Key Vault:

```bash
# 建立 Key Vault
az keyvault create \
  --name contosouniversity-kv \
  --resource-group ContosoUniversity-RG \
  --location eastus

# 添加 Secret
az keyvault secret set \
  --vault-name contosouniversity-kv \
  --name SqlConnectionString \
  --value "Your Connection String"

# 為 Web App 啟用 Managed Identity
az webapp identity assign \
  --name contosouniversity-app \
  --resource-group ContosoUniversity-RG

# 授予權限
az keyvault set-policy \
  --name contosouniversity-kv \
  --object-id <webapp-identity-id> \
  --secret-permissions get list
```

## 監控與故障排除

### 啟用 Application Insights

```bash
# 建立 Application Insights
az monitor app-insights component create \
  --app contosouniversity-ai \
  --location eastus \
  --resource-group ContosoUniversity-RG

# 連接到 Web App
az webapp config appsettings set \
  --name contosouniversity-app \
  --resource-group ContosoUniversity-RG \
  --settings APPINSIGHTS_INSTRUMENTATIONKEY="your-key"
```

### 查看日誌

**Stream logs**:
```bash
az webapp log tail \
  --name contosouniversity-app \
  --resource-group ContosoUniversity-RG
```

**Download logs**:
```bash
az webapp log download \
  --name contosouniversity-app \
  --resource-group ContosoUniversity-RG \
  --log-file logs.zip
```

### 常見問題

#### 1. 應用無法啟動

**檢查**:
- 確認連接字串正確
- 檢查應用日誌
- 驗證 .NET 運行時版本

#### 2. 資料庫連接失敗

**檢查**:
- SQL Server 防火牆規則
- 連接字串格式
- SQL 認證

#### 3. 靜態文件 404

**檢查**:
- 確認 `UseStaticFiles()` middleware
- 檢查 wwwroot 資料夾

## 成本估算

### 基本配置（開發/測試）

| 資源 | SKU | 預估月費用 (USD) |
|------|-----|------------------|
| App Service Plan | B1 (Basic) | ~$13 |
| SQL Database | S0 (Standard) | ~$15 |
| **總計** | | **~$28/月** |

### 生產配置

| 資源 | SKU | 預估月費用 (USD) |
|------|-----|------------------|
| App Service Plan | P1V2 (Premium) | ~$70 |
| SQL Database | S2 (Standard) | ~$75 |
| Application Insights | 基本 | ~$10 |
| **總計** | | **~$155/月** |

## 最佳實踐

1. **使用 Managed Identity** - 避免在代碼中儲存憑證
2. **啟用 HTTPS** - 強制使用安全連接
3. **實施 CI/CD** - 自動化部署流程
4. **設置監控** - Application Insights
5. **定期備份** - SQL 資料庫自動備份
6. **使用 Slot** - 藍綠部署策略
7. **優化性能** - CDN、快取
8. **實施 WAF** - Azure Front Door

## 清除資源

完成測試後清除資源:

```bash
az group delete --name ContosoUniversity-RG --yes --no-wait
```

## 參考資料

- [Azure App Service 文檔](https://docs.microsoft.com/azure/app-service/)
- [Azure SQL Database 文檔](https://docs.microsoft.com/azure/sql-database/)
- [GitHub Actions 文檔](https://docs.github.com/actions)
- [Azure ARM Templates](https://docs.microsoft.com/azure/azure-resource-manager/templates/)
