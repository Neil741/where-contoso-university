@echo off
REM Contoso University - Quick Start Script (Windows)
REM This script helps you quickly set up and run the application locally

echo ================================================
echo   Contoso University - .NET 8.0 Quick Start
echo ================================================
echo.

REM Check if Docker is installed
where docker >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Docker is not installed. Please install Docker Desktop first.
    echo        Download from: https://www.docker.com/products/docker-desktop
    pause
    exit /b 1
)

REM Check if .NET 8 SDK is installed
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET 8 SDK is not installed. Please install it first.
    echo        Download from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo ✓ Prerequisites check passed
echo.

REM Step 1: Start SQL Server
echo Step 1: Starting SQL Server container...
echo         This may take a few minutes on first run...
docker-compose up -d

echo         Waiting for SQL Server to be ready...
timeout /t 15 /nobreak >nul

REM Check if SQL Server is running
docker-compose ps | findstr "Up" >nul
if %ERRORLEVEL% EQU 0 (
    echo ✓ SQL Server is running
) else (
    echo ERROR: Failed to start SQL Server
    pause
    exit /b 1
)

echo.

REM Step 2: Restore NuGet packages
echo Step 2: Restoring NuGet packages...
cd ContosoUniversity
dotnet restore

echo.

REM Step 3: Build the application
echo Step 3: Building the application...
dotnet build -c Debug

echo.

REM Step 4: Run the application
echo Step 4: Starting the application...
echo.
echo ================================================
echo   Application will start shortly...
echo   Open your browser and navigate to:
echo   - https://localhost:5001
echo   - http://localhost:5000
echo.
echo   Press Ctrl+C to stop the application
echo ================================================
echo.

set ASPNETCORE_ENVIRONMENT=Development
dotnet run
