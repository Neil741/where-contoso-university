# Contoso University - .NET 8.0

Contoso University 是一個示範 ASP.NET Core MVC 和 Entity Framework Core 的教學專案。

## 專案狀態

✅ 已成功從 .NET Framework 4.8 遷移至 .NET 8.0  
✅ 使用 ASP.NET Core MVC 架構  
✅ 支援跨平台部署  

## 技術堆疊

- .NET 8.0
- ASP.NET Core MVC 8.0
- Entity Framework Core 8.0
- SQL Server

## 快速開始

詳細指南請參考 [Docs/LOCAL_SETUP_GUIDE.md](Docs/LOCAL_SETUP_GUIDE.md)

```bash
# 啟動 SQL Server
docker-compose up -d

# 運行應用
cd ContosoUniversity
export ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

訪問 http://localhost:5000

## 專案結構

- `ContosoUniversity.Legacy/` - 原 .NET Framework 4.8 專案
- `ContosoUniversity/` - 新 .NET 8.0 專案
- `Docs/` - 文檔
- `Scripts/` - 部署腳本

## 核心功能

- 學生管理 (CRUD + 搜索 + 分頁)
- 課程管理 (CRUD + 教材上傳)
- 教師管理 (CRUD + 課程指派)
- 部門管理 (CRUD + 並行控制)
- 待辦事項 (使用預存程序)
- 通知系統

## 遷移資訊

本專案已從 .NET Framework 4.8 遷移至 .NET 8.0。主要變更：

- System.Web.Mvc → Microsoft.AspNetCore.Mvc
- Web.config → appsettings.json
- Global.asax → Program.cs
- MSMQ → In-memory Queue

詳細報告：[Docs/UPGRADE_REPORT.md](Docs/UPGRADE_REPORT.md)
