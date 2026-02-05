# 場景二：舊有規格轉新規格文件

## 目標
以遺留程式碼及舊規格文件，產出前後端分離之升級及重構規格文件，提供後續開發及維護參考

## 新架構目標
- 前端：React + Vite
- 後端：.NET 8 / ASP.NET Core Web API
- 資料庫：Azure SQL Database
- 部署環境：Azure App Service（Linux）+ CI/CD（GitHub Actions）


### 重點 / Key Points

1. **相容性評估 (Compatibility Assessment)**
   - 盤點 System.Web 依賴並規劃替代
   - Web.config → appsettings.json 的移轉需求

2. **前後端拆分 (Separation Assessment)**
   - UI 與業務邏輯耦合點
   - API 化可行性與影響範圍

3. **風險與替代方案 (Risk & Alternatives)**
   - 套件版本風險與替代方案
   - MSMQ 等不相容元件的替代需求

## 需求
- 閱讀遺留程式碼
- 建立規格文件（聚焦於架構拆分、相容性盤點、套件與安全性目標、DI 與服務設計需求，以及部署與驗證準則）
- 所有規格文件將被放入 Specs/ 資料夾

## 資料夾結構
- 確保專案結構清晰，便於維護與擴展
- 建議資料夾結構如下：
```
/ContosoUniversity
│── /ContosoUniversity              # 遺留之程式碼
│── /Specs                          # 規格文件
│── /Lab                            # 操作練習文件
│── README.md                       # 專案說明文件
|── .gitignore
```

## 注意事項 / Precautions
- **不包含任何程式碼實作** - 僅限需求與規格
- **Web.config 移除** - 所有設定需求需集中至 appsettings.json
- **CORS 設定** - 後端需定義允許前端來源的需求
- 預留 Azure Service Bus 實作介面以備未來擴充


## 參考資源 / References

- [ASP.NET Core Documentation](https://learn.microsoft.com/aspnet/core) - ASP.NET Core 官方文件
- [.NET 8 Migration Guide](https://learn.microsoft.com/dotnet/core/porting/) - .NET 遷移指南
- [Azure App Service Documentation](https://learn.microsoft.com/azure/app-service/) - Azure App Service 文件
- [Azure SQL Documentation](https://learn.microsoft.com/azure/azure-sql/) - Azure SQL 文件
- [Microsoft.Data.SqlClient](https://github.com/dotnet/SqlClient) - SQL Client 版本與安全公告
- [React Documentation](https://react.dev/) - React 官方文件