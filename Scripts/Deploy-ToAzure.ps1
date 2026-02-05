#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Deploy Contoso University to Azure App Service
.DESCRIPTION
    This script creates all necessary Azure resources and deploys the application
.PARAMETER ResourceGroupName
    Name of the Azure Resource Group
.PARAMETER Location
    Azure region for deployment (default: eastus)
.PARAMETER AppName
    Base name for the application (default: auto-generated)
.PARAMETER SqlAdminPassword
    SQL Server administrator password (if not provided, will prompt)
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus",
    
    [Parameter(Mandatory=$false)]
    [string]$AppName,
    
    [Parameter(Mandatory=$false)]
    [SecureString]$SqlAdminPassword
)

# Ensure Azure CLI is installed
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI is not installed. Please install it from https://aka.ms/azure-cli"
    exit 1
}

# Login check
Write-Host "Checking Azure login status..." -ForegroundColor Cyan
$account = az account show 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Please login to Azure..." -ForegroundColor Yellow
    az login
}

# Get SQL password if not provided
if (-not $SqlAdminPassword) {
    $SqlAdminPassword = Read-Host -AsSecureString "Enter SQL Server administrator password (min 8 chars, must include uppercase, lowercase, numbers, and symbols)"
}

# Convert SecureString to plain text for Azure CLI
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SqlAdminPassword)
$PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

# Generate app name if not provided
if (-not $AppName) {
    $AppName = "contoso-$(Get-Random -Minimum 1000 -Maximum 9999)"
}

Write-Host "`n==== Deployment Configuration ====" -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroupName"
Write-Host "Location: $Location"
Write-Host "App Name: $AppName"
Write-Host "===================================`n"

# Create resource group
Write-Host "Creating resource group..." -ForegroundColor Cyan
az group create --name $ResourceGroupName --location $Location

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create resource group"
    exit 1
}

# Deploy ARM template
Write-Host "Deploying Azure resources (this may take several minutes)..." -ForegroundColor Cyan
$deploymentName = "contoso-deployment-$(Get-Date -Format 'yyyyMMddHHmmss')"

az deployment group create `
    --name $deploymentName `
    --resource-group $ResourceGroupName `
    --template-file "./Scripts/azure-deploy.json" `
    --parameters appName=$AppName location=$Location sqlAdministratorPassword=$PlainPassword

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to deploy Azure resources"
    exit 1
}

# Get deployment outputs
Write-Host "Retrieving deployment information..." -ForegroundColor Cyan
$outputs = az deployment group show `
    --name $deploymentName `
    --resource-group $ResourceGroupName `
    --query properties.outputs | ConvertFrom-Json

$webAppUrl = $outputs.webAppUrl.value
$sqlServerFqdn = $outputs.sqlServerFqdn.value

# Build and publish the application
Write-Host "`nBuilding application..." -ForegroundColor Cyan
dotnet publish ./ContosoUniversity/ContosoUniversity.csproj -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build application"
    exit 1
}

# Deploy to App Service
Write-Host "Deploying application to Azure App Service..." -ForegroundColor Cyan
Push-Location ./publish
Compress-Archive -Path * -DestinationPath ../deploy.zip -Force
Pop-Location

az webapp deployment source config-zip `
    --resource-group $ResourceGroupName `
    --name $AppName `
    --src ./deploy.zip

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to deploy application"
    exit 1
}

# Clean up
Remove-Item ./deploy.zip -Force
Remove-Item ./publish -Recurse -Force

Write-Host "`n==== Deployment Successful! ====" -ForegroundColor Green
Write-Host "Application URL: $webAppUrl"
Write-Host "SQL Server: $sqlServerFqdn"
Write-Host "===================================`n"
Write-Host "The application is now available at: $webAppUrl" -ForegroundColor Cyan
Write-Host "Note: It may take a few minutes for the application to start up." -ForegroundColor Yellow
