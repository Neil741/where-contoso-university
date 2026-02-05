#!/bin/bash

# Contoso University - Quick Start Script
# This script helps you quickly set up and run the application locally

set -e

echo "================================================"
echo "  Contoso University - .NET 8.0 Quick Start"
echo "================================================"
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker Desktop first."
    echo "   Download from: https://www.docker.com/products/docker-desktop"
    exit 1
fi

# Check if .NET 8 SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET 8 SDK is not installed. Please install it first."
    echo "   Download from: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
if [[ $DOTNET_VERSION < "8.0" ]]; then
    echo "⚠️  Warning: You have .NET $DOTNET_VERSION, but .NET 8.0 is required."
    echo "   The application may not work correctly."
fi

echo "✅ Prerequisites check passed"
echo ""

# Step 1: Start SQL Server
echo "📦 Step 1: Starting SQL Server container..."
echo "   This may take a few minutes on first run..."
docker-compose up -d

echo "   Waiting for SQL Server to be ready..."
sleep 15

# Check if SQL Server is running
if docker-compose ps | grep -q "Up"; then
    echo "✅ SQL Server is running"
else
    echo "❌ Failed to start SQL Server"
    exit 1
fi

echo ""

# Step 2: Restore NuGet packages
echo "📦 Step 2: Restoring NuGet packages..."
cd ContosoUniversity
dotnet restore

echo ""

# Step 3: Build the application
echo "🔨 Step 3: Building the application..."
dotnet build -c Debug

echo ""

# Step 4: Run the application
echo "🚀 Step 4: Starting the application..."
echo ""
echo "================================================"
echo "  Application will start shortly..."
echo "  Open your browser and navigate to:"
echo "  - https://localhost:5001"
echo "  - http://localhost:5000"
echo ""
echo "  Press Ctrl+C to stop the application"
echo "================================================"
echo ""

export ASPNETCORE_ENVIRONMENT=Development
dotnet run
