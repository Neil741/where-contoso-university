# Azure 部署腳本 - Contoso University
# 用途: 自動部署 Contoso University 到 Azure App Service

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus",
    
    [Parameter(Mandatory=$true)]
    [string]$SqlAdminPassword
)

# 設置變數
$SqlServerName = "$ResourceGroupName-sql".ToLower() -replace '[^a-z0-9-]', ''
$DatabaseName = "ContosoUniversityDB"
$AppServicePlanName = "$ResourceGroupName-ASP"
$WebAppName = "$ResourceGroupName-app".ToLower() -replace '[^a-z0-9-]', ''
$SqlAdminUser = "sqladmin"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Contoso University Azure 部署腳本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "資源群組: $ResourceGroupName" -ForegroundColor Yellow
Write-Host "位置: $Location" -ForegroundColor Yellow
Write-Host "SQL Server: $SqlServerName" -ForegroundColor Yellow
Write-Host "Web App: $WebAppName" -ForegroundColor Yellow
Write-Host ""

# 確認繼續
$confirmation = Read-Host "是否繼續部署？ (y/n)"
if ($confirmation -notin @('y', 'Y', 'yes', 'Yes', 'YES')) {
    Write-Host "部署已取消" -ForegroundColor Red
    exit
}

try {
    # 1. 建立資源群組
    Write-Host ""
    Write-Host "[1/9] 建立資源群組..." -ForegroundColor Green
    az group create --name $ResourceGroupName --location $Location
    if ($LASTEXITCODE -ne 0) { throw "建立資源群組失敗" }

    # 2. 建立 SQL Server
    Write-Host ""
    Write-Host "[2/9] 建立 SQL Server..." -ForegroundColor Green
    az sql server create `
        --name $SqlServerName `
        --resource-group $ResourceGroupName `
        --location $Location `
        --admin-user $SqlAdminUser `
        --admin-password $SqlAdminPassword
    if ($LASTEXITCODE -ne 0) { throw "建立 SQL Server 失敗" }

    # 3. 建立資料庫
    Write-Host ""
    Write-Host "[3/9] 建立資料庫..." -ForegroundColor Green
    az sql db create `
        --resource-group $ResourceGroupName `
        --server $SqlServerName `
        --name $DatabaseName `
        --service-objective S0 `
        --max-size 2GB
    if ($LASTEXITCODE -ne 0) { throw "建立資料庫失敗" }

    # 4. 配置防火牆規則
    Write-Host ""
    Write-Host "[4/9] 配置防火牆規則..." -ForegroundColor Green
    az sql server firewall-rule create `
        --resource-group $ResourceGroupName `
        --server $SqlServerName `
        --name AllowAzureServices `
        --start-ip-address 0.0.0.0 `
        --end-ip-address 0.0.0.0
    if ($LASTEXITCODE -ne 0) { throw "配置防火牆失敗" }

    # 5. 建立 App Service 計劃
    Write-Host ""
    Write-Host "[5/9] 建立 App Service 計劃..." -ForegroundColor Green
    az appservice plan create `
        --name $AppServicePlanName `
        --resource-group $ResourceGroupName `
        --location $Location `
        --sku B1 `
        --is-linux
    if ($LASTEXITCODE -ne 0) { throw "建立 App Service 計劃失敗" }

    # 6. 建立 Web App
    Write-Host ""
    Write-Host "[6/9] 建立 Web App..." -ForegroundColor Green
    az webapp create `
        --name $WebAppName `
        --resource-group $ResourceGroupName `
        --plan $AppServicePlanName `
        --runtime "DOTNET|8.0"
    if ($LASTEXITCODE -ne 0) { throw "建立 Web App 失敗" }

    # 7. 配置連接字串
    Write-Host ""
    Write-Host "[7/9] 配置連接字串..." -ForegroundColor Green
    $ConnectionString = "Server=tcp:$SqlServerName.database.windows.net,1433;Database=$DatabaseName;User ID=$SqlAdminUser;Password=$SqlAdminPassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;MultipleActiveResultSets=True"
    
    az webapp config connection-string set `
        --name $WebAppName `
        --resource-group $ResourceGroupName `
        --connection-string-type SQLAzure `
        --settings DefaultConnection="$ConnectionString"
    if ($LASTEXITCODE -ne 0) { throw "配置連接字串失敗" }

    # 啟用 HTTPS
    az webapp update `
        --name $WebAppName `
        --resource-group $ResourceGroupName `
        --https-only true
    if ($LASTEXITCODE -ne 0) { throw "啟用 HTTPS 失敗" }

    # 8. 發佈應用程式
    Write-Host ""
    Write-Host "[8/9] 發佈應用程式..." -ForegroundColor Green
    
    # 確認專案目錄
    if (!(Test-Path "ContosoUniversity/ContosoUniversity.csproj")) {
        throw "找不到 ContosoUniversity 專案檔案"
    }

    Push-Location
    Set-Location ContosoUniversity
    
    # 發佈
    Write-Host "  - 正在編譯..." -ForegroundColor Yellow
    dotnet publish -c Release -o ./publish
    if ($LASTEXITCODE -ne 0) { throw "發佈應用失敗" }

    # 壓縮檔案
    Write-Host "  - 正在壓縮..." -ForegroundColor Yellow
    if (Test-Path "./app.zip") {
        Remove-Item "./app.zip" -Force
    }
    Compress-Archive -Path ./publish/* -DestinationPath ./app.zip -Force

    # 部署到 Azure
    Write-Host "  - 正在上傳至 Azure..." -ForegroundColor Yellow
    az webapp deployment source config-zip `
        --resource-group $ResourceGroupName `
        --name $WebAppName `
        --src ./app.zip
    if ($LASTEXITCODE -ne 0) { throw "部署應用失敗" }

    # 清理
    Write-Host "  - 清理臨時檔案..." -ForegroundColor Yellow
    Remove-Item -Path ./publish -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path ./app.zip -Force -ErrorAction SilentlyContinue
    
    Pop-Location

    # 9. 驗證部署
    Write-Host ""
    Write-Host "[9/9] 驗證部署..." -ForegroundColor Green
    Start-Sleep -Seconds 10
    
    $AppUrl = "https://$WebAppName.azurewebsites.net"
    Write-Host "  - 正在測試連線..." -ForegroundColor Yellow
    
    try {
        $response = Invoke-WebRequest -Uri $AppUrl -Method Head -TimeoutSec 30 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            Write-Host "  ✓ 應用程式正常運行" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "  ⚠ 無法立即連接，應用可能還在啟動中" -ForegroundColor Yellow
    }

    # 部署成功
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "部署完成！" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "資源資訊:" -ForegroundColor Yellow
    Write-Host "  資源群組: $ResourceGroupName" -ForegroundColor White
    Write-Host "  SQL Server: $SqlServerName.database.windows.net" -ForegroundColor White
    Write-Host "  資料庫: $DatabaseName" -ForegroundColor White
    Write-Host "  Web App: $WebAppName" -ForegroundColor White
    Write-Host ""
    Write-Host "應用 URL: $AppUrl" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "下一步:" -ForegroundColor Yellow
    Write-Host "  1. 訪問應用 URL 驗證功能" -ForegroundColor White
    Write-Host "  2. 檢查 Azure Portal 中的資源" -ForegroundColor White
    Write-Host "  3. 配置 Application Insights（可選）" -ForegroundColor White
    Write-Host "  4. 設置自定義域名（可選）" -ForegroundColor White
    Write-Host ""
    Write-Host "清理資源:" -ForegroundColor Yellow
    Write-Host "  az group delete --name $ResourceGroupName --yes --no-wait" -ForegroundColor White
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "部署失敗！" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "錯誤: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "故障排除:" -ForegroundColor Yellow
    Write-Host "  1. 確認 Azure CLI 已安裝並登入" -ForegroundColor White
    Write-Host "  2. 確認資源名稱未被使用" -ForegroundColor White
    Write-Host "  3. 檢查訂閱配額和權限" -ForegroundColor White
    Write-Host "  4. 查看詳細錯誤訊息" -ForegroundColor White
    Write-Host ""
    Write-Host "清理已建立的資源:" -ForegroundColor Yellow
    Write-Host "  az group delete --name $ResourceGroupName --yes --no-wait" -ForegroundColor White
    Write-Host ""
    exit 1
}
