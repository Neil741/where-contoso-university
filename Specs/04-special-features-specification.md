# 特殊功能規格

## 概述

本文件詳細描述 Contoso University 系統中三個特殊功能模組的實作規格：
1. 即時通知系統（基於 MSMQ）
2. 待辦事項系統（基於存儲程序）
3. 教材上傳系統（檔案管理）

---

## 1. 即時通知系統 (Notification System)

### 1.1 系統目標

提供管理員即時的資料變更通知，當學生、課程、講師或部門資料發生 CREATE、UPDATE 或 DELETE 操作時，透過訊息佇列發送通知並在前端顯示。

### 1.2 架構設計

```
┌───────────────┐
│  Controller   │ (Create/Update/Delete)
│   Actions     │
└───────┬───────┘
        │
        ▼
┌───────────────┐
│BaseController │
│SendNotification│
└───────┬───────┘
        │
        ▼
┌───────────────┐
│ Notification  │
│   Service     │ (NotificationService.cs)
└───────┬───────┘
        │
        ▼
┌───────────────┐
│     MSMQ      │ (訊息佇列)
│   Message     │
│    Queue      │
└───────┬───────┘
        │
        ▼
┌───────────────┐
│ Notification  │
│  Controller   │ (API Endpoint)
│ GetNotific... │
└───────┬───────┘
        │
        ▼
┌───────────────┐
│  JavaScript   │ (每5秒輪詢)
│   Polling     │
└───────┬───────┘
        │
        ▼
┌───────────────┐
│  UI Display   │ (浮動通知，1分鐘自動關閉)
└───────────────┘
```

### 1.3 資料模型

#### Notification 實體

| 欄位 | 類型 | 必填 | 說明 |
|------|------|------|------|
| Id | int | 是 | 主鍵 |
| EntityType | string | 是 | 實體類型（Student, Course, Instructor, Department） |
| EntityId | int | 是 | 實體 ID |
| Operation | string | 是 | 操作類型（CREATE, UPDATE, DELETE） |
| Message | string | 是 | 通知訊息內容 |
| CreatedAt | DateTime | 是 | 通知創建時間 |

### 1.4 訊息佇列配置

#### Web.config 配置

```xml
<appSettings>
    <add key="NotificationQueuePath" value=".\private$\ContosoUniversityNotifications" />
</appSettings>
```

#### MSMQ 佇列建立

**佇列名稱**: `.\private$\ContosoUniversityNotifications`

**佇列類型**: 私有佇列（Private Queue）

**訊息格式**: JSON

**訊息範例**:
```json
{
    "EntityType": "Student",
    "EntityId": 123,
    "Operation": "CREATE",
    "Message": "Student 'John Smith' has been created",
    "CreatedAt": "2026-02-05T10:30:00Z"
}
```

### 1.5 後端實作

#### NotificationService 類別

**位置**: `Services/NotificationService.cs`

**主要方法**:

##### 1. SendNotificationToQueue

```csharp
public void SendNotificationToQueue(string entityType, int entityId, string operation, string message)
```

**功能**: 發送通知到 MSMQ 佇列

**參數**:
- `entityType`: 實體類型（"Student", "Course", "Instructor", "Department"）
- `entityId`: 實體 ID
- `operation`: 操作類型（"CREATE", "UPDATE", "DELETE"）
- `message`: 通知訊息

**實作邏輯**:
1. 建立 Notification 物件
2. 序列化為 JSON
3. 建立 MessageQueue 連接
4. 發送訊息到佇列
5. 錯誤處理與日誌記錄

**錯誤處理**:
- 佇列不存在: 自動建立佇列
- 連接失敗: 記錄錯誤並繼續（不中斷主要業務流程）
- 序列化失敗: 記錄錯誤

##### 2. GetNotificationsFromQueue

```csharp
public List<Notification> GetNotificationsFromQueue()
```

**功能**: 從 MSMQ 佇列讀取所有待處理的通知

**實作邏輯**:
1. 建立 MessageQueue 連接
2. 設定訊息讀取格式為 JSON
3. 使用 GetAllMessages() 讀取所有訊息
4. 反序列化每個訊息為 Notification 物件
5. 清空佇列（訊息已讀取）
6. 返回通知列表

**錯誤處理**:
- 佇列為空: 返回空列表
- 反序列化失敗: 跳過該訊息並記錄錯誤
- 連接失敗: 返回空列表並記錄錯誤

#### BaseController 整合

**位置**: `Controllers/BaseController.cs`

**通知方法**:

```csharp
protected void SendNotification(string entityType, int entityId, string operation, string message)
{
    try
    {
        var notificationService = new NotificationService();
        notificationService.SendNotificationToQueue(entityType, entityId, operation, message);
    }
    catch (Exception ex)
    {
        // Log error but don't fail the main operation
        LoggingService.LogError($"Failed to send notification: {ex.Message}");
    }
}
```

**使用範例**:

```csharp
// 在 StudentsController.Create 方法中
await _context.SaveChangesAsync();
SendNotification("Student", student.ID, "CREATE", $"Student '{student.LastName}' has been created");
```

#### NotificationsController API

**位置**: `Controllers/NotificationsController.cs`

##### API 端點 1: 取得通知

**路由**: GET `/Notifications/GetNotifications`

**回應格式**: JSON

**功能**: 從 MSMQ 讀取並返回所有新通知

**實作**:
```csharp
public JsonResult GetNotifications()
{
    var notificationService = new NotificationService();
    var notifications = notificationService.GetNotificationsFromQueue();
    return Json(notifications, JsonRequestBehavior.AllowGet);
}
```

**回應範例**:
```json
[
    {
        "Id": 0,
        "EntityType": "Student",
        "EntityId": 123,
        "Operation": "CREATE",
        "Message": "Student 'John Smith' has been created",
        "CreatedAt": "2026-02-05T10:30:00Z"
    }
]
```

##### API 端點 2: 通知儀表板

**路由**: GET `/Notifications/Index`

**功能**: 顯示通知管理頁面（管理員用）

**視圖**: `Views/Notifications/Index.cshtml`

### 1.6 前端實作

#### JavaScript 輪詢機制

**位置**: `Views/Shared/_Layout.cshtml`

**實作邏輯**:

```javascript
// 每 5 秒輪詢一次新通知
setInterval(function() {
    $.ajax({
        url: '/Notifications/GetNotifications',
        type: 'GET',
        dataType: 'json',
        success: function(notifications) {
            if (notifications && notifications.length > 0) {
                notifications.forEach(function(notification) {
                    showNotification(notification);
                });
            }
        },
        error: function(error) {
            console.error('Failed to fetch notifications:', error);
        }
    });
}, 5000); // 5 seconds
```

#### 通知顯示

**功能**: 在頁面右上角顯示浮動通知

**通知樣式**:
- **CREATE**: 綠色背景（success）
- **UPDATE**: 藍色背景（info）
- **DELETE**: 橙色背景（warning）

**顯示時長**: 1 分鐘（60000 毫秒）後自動關閉

**實作**:

```javascript
function showNotification(notification) {
    var notificationClass = getNotificationClass(notification.Operation);
    var notificationHtml = `
        <div class="alert alert-${notificationClass} alert-dismissible notification-popup" role="alert">
            <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
            <strong>${notification.Operation}:</strong> ${notification.Message}
        </div>
    `;
    
    $('#notification-container').append(notificationHtml);
    
    // 1 分鐘後自動移除
    setTimeout(function() {
        $('.notification-popup').first().fadeOut(function() {
            $(this).remove();
        });
    }, 60000);
}

function getNotificationClass(operation) {
    switch(operation) {
        case 'CREATE': return 'success';
        case 'UPDATE': return 'info';
        case 'DELETE': return 'warning';
        default: return 'secondary';
    }
}
```

#### CSS 樣式

```css
.notification-popup {
    position: fixed;
    top: 70px;
    right: 20px;
    min-width: 300px;
    max-width: 500px;
    z-index: 9999;
    margin-bottom: 10px;
    box-shadow: 0 4px 8px rgba(0,0,0,0.1);
}
```

### 1.7 權限控制

**可見性**: 僅管理員（Admin 角色）

**實作方式**: 
- 前端檢查使用者角色
- 非管理員不顯示通知容器
- API 端點可加入角色驗證（選用）

### 1.8 系統需求

#### Windows 功能需求

**MSMQ 安裝**:
1. 開啟 "Windows 功能"
2. 勾選 "Microsoft Message Queue (MSMQ) Server"
3. 勾選 "MSMQ Server Core"
4. 重新啟動電腦

#### 佇列初始化

**自動建立**: NotificationService 會在首次使用時自動建立佇列（如果不存在）

**手動建立** (選用):
```csharp
if (!MessageQueue.Exists(queuePath))
{
    MessageQueue.Create(queuePath);
}
```

### 1.9 測試場景

#### 測試案例 1: 學生建立通知

1. 登入為管理員
2. 建立新學生
3. 驗證通知在 1 分鐘內顯示在右上角
4. 驗證通知顯示正確的訊息和綠色背景

#### 測試案例 2: 課程更新通知

1. 登入為管理員
2. 編輯課程資訊
3. 驗證通知顯示藍色背景

#### 測試案例 3: 部門刪除通知

1. 登入為管理員
2. 刪除部門
3. 驗證通知顯示橙色背景

#### 測試案例 4: 多個通知

1. 快速執行多個操作
2. 驗證所有通知都正確顯示
3. 驗證通知依序自動關閉

### 1.10 限制與注意事項

**限制**:
- 僅支援 Windows 平台（MSMQ 依賴）
- 訊息持久性依賴 MSMQ 配置
- 通知不會保存到資料庫（僅透過佇列）

**注意事項**:
- MSMQ 佇列需要定期監控
- 如果佇列累積過多訊息，可能影響效能
- 非管理員使用者不會收到通知

**改進建議**:
- 考慮將通知保存到資料庫以供歷史查詢
- 實作 SignalR 以實現真正的即時推送（替代輪詢）
- 雲端遷移時使用 Azure Service Bus 替代 MSMQ

---

## 2. 待辦事項系統 (ToDo System)

### 2.1 系統目標

提供簡單的任務管理功能，使用存儲程序進行 CRUD 操作，示範 ADO.NET 直接資料庫訪問。

### 2.2 架構設計

```
┌───────────────┐
│ ToDosController│
└───────┬───────┘
        │
        ▼
┌───────────────┐
│   ADO.NET     │ (SqlConnection, SqlCommand)
│  Direct Call  │
└───────┬───────┘
        │
        ▼
┌───────────────┐
│   Stored      │ (SQL Server Stored Procedures)
│  Procedures   │
│               │
│ - sp_GetAll   │
│ - sp_GetById  │
│ - sp_Create   │
│ - sp_Update   │
│ - sp_Delete   │
└───────────────┘
```

### 2.3 資料模型

#### ToDo 實體

| 欄位 | 類型 | 必填 | 說明 |
|------|------|------|------|
| Id | int | 是 | 主鍵，自動遞增 |
| Title | string | 是 | 待辦事項標題，最大長度 255 |
| Description | string | - | 詳細描述（無長度限制） |
| IsCompleted | bool | 是 | 完成狀態，預設 false |
| CreatedDate | DateTime | 是 | 創建日期 |
| CompletedDate | DateTime | - | 完成日期（可為 null） |

### 2.4 存儲程序定義

#### sp_GetAllToDos

**功能**: 取得所有待辦事項，依完成狀態和創建日期排序

**SQL 定義**:
```sql
CREATE PROCEDURE sp_GetAllToDos
AS
BEGIN
    SELECT Id, Title, Description, IsCompleted, CreatedDate, CompletedDate
    FROM ToDos
    ORDER BY IsCompleted ASC, CreatedDate DESC
END
```

**排序邏輯**:
1. 未完成的項目排在前面
2. 相同完成狀態的項目按創建日期降序排列

#### sp_GetToDoById

**功能**: 根據 ID 取得特定待辦事項

**參數**:
- `@Id INT`

**SQL 定義**:
```sql
CREATE PROCEDURE sp_GetToDoById
    @Id INT
AS
BEGIN
    SELECT Id, Title, Description, IsCompleted, CreatedDate, CompletedDate
    FROM ToDos
    WHERE Id = @Id
END
```

#### sp_CreateToDo

**功能**: 建立新的待辦事項

**參數**:
- `@Title NVARCHAR(255)` - 必填
- `@Description NVARCHAR(MAX)` - 選填
- `@IsCompleted BIT` - 預設 0

**SQL 定義**:
```sql
CREATE PROCEDURE sp_CreateToDo
    @Title NVARCHAR(255),
    @Description NVARCHAR(MAX) = NULL,
    @IsCompleted BIT = 0
AS
BEGIN
    INSERT INTO ToDos (Title, Description, IsCompleted, CreatedDate)
    VALUES (@Title, @Description, @IsCompleted, GETDATE())
    
    SELECT SCOPE_IDENTITY() AS Id
END
```

**返回值**: 新建立的待辦事項 ID

#### sp_UpdateToDo

**功能**: 更新待辦事項

**參數**:
- `@Id INT` - 必填
- `@Title NVARCHAR(255)` - 必填
- `@Description NVARCHAR(MAX)` - 選填
- `@IsCompleted BIT` - 必填

**SQL 定義**:
```sql
CREATE PROCEDURE sp_UpdateToDo
    @Id INT,
    @Title NVARCHAR(255),
    @Description NVARCHAR(MAX) = NULL,
    @IsCompleted BIT
AS
BEGIN
    UPDATE ToDos
    SET Title = @Title,
        Description = @Description,
        IsCompleted = @IsCompleted,
        CompletedDate = CASE WHEN @IsCompleted = 1 AND IsCompleted = 0 
                            THEN GETDATE() 
                            WHEN @IsCompleted = 0 
                            THEN NULL 
                            ELSE CompletedDate 
                        END
    WHERE Id = @Id
END
```

**業務邏輯**:
- 當從未完成變為完成時，設定 CompletedDate 為當前時間
- 當從完成變為未完成時，清除 CompletedDate
- 如果狀態未變，保持原 CompletedDate

#### sp_DeleteToDo

**功能**: 刪除待辦事項

**參數**:
- `@Id INT`

**SQL 定義**:
```sql
CREATE PROCEDURE sp_DeleteToDo
    @Id INT
AS
BEGIN
    DELETE FROM ToDos WHERE Id = @Id
END
```

### 2.5 控制器實作

#### ToDosController

**位置**: `Controllers/ToDosController.cs`

**資料庫連接**: 使用 ADO.NET 直接連接

##### 方法 1: Index (列表)

**路由**: GET `/ToDos`

**功能**: 顯示所有待辦事項

**實作**:
```csharp
public ActionResult Index()
{
    var todos = new List<ToDo>();
    using (var connection = new SqlConnection(_connectionString))
    {
        var command = new SqlCommand("sp_GetAllToDos", connection);
        command.CommandType = CommandType.StoredProcedure;
        
        connection.Open();
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                todos.Add(new ToDo
                {
                    Id = (int)reader["Id"],
                    Title = reader["Title"].ToString(),
                    Description = reader["Description"] as string,
                    IsCompleted = (bool)reader["IsCompleted"],
                    CreatedDate = (DateTime)reader["CreatedDate"],
                    CompletedDate = reader["CompletedDate"] as DateTime?
                });
            }
        }
    }
    return View(todos);
}
```

##### 方法 2: Details (詳情)

**路由**: GET `/ToDos/Details/{id}`

**功能**: 顯示特定待辦事項的詳細資訊

**實作**: 調用 `sp_GetToDoById`

##### 方法 3: Create (建立)

**路由**: 
- GET `/ToDos/Create`
- POST `/ToDos/Create`

**功能**: 建立新待辦事項

**POST 實作**:
```csharp
[HttpPost]
public ActionResult Create(ToDo todo)
{
    if (ModelState.IsValid)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand("sp_CreateToDo", connection);
            command.CommandType = CommandType.StoredProcedure;
            
            command.Parameters.AddWithValue("@Title", todo.Title);
            command.Parameters.AddWithValue("@Description", todo.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsCompleted", todo.IsCompleted);
            
            connection.Open();
            var newId = command.ExecuteScalar();
            todo.Id = Convert.ToInt32(newId);
        }
        return RedirectToAction("Index");
    }
    return View(todo);
}
```

##### 方法 4: Edit (編輯)

**路由**: 
- GET `/ToDos/Edit/{id}`
- POST `/ToDos/Edit/{id}`

**功能**: 編輯待辦事項

**POST 實作**: 調用 `sp_UpdateToDo`

##### 方法 5: Delete (刪除)

**路由**: 
- GET `/ToDos/Delete/{id}`
- POST `/ToDos/Delete/{id}`

**功能**: 刪除待辦事項

**POST 實作**: 調用 `sp_DeleteToDo`

### 2.6 前端視圖

#### Index.cshtml (列表頁)

**顯示內容**:
- 待辦事項表格
  - 標題
  - 描述（截斷顯示）
  - 完成狀態（✓ 或 ✗）
  - 創建日期
  - 完成日期
  - 操作按鈕（Edit, Details, Delete）
- "Create New" 按鈕

**特殊顯示**:
- 已完成的項目以不同樣式顯示（刪除線、灰色文字）
- 未完成的項目高亮顯示

#### Create.cshtml / Edit.cshtml (表單頁)

**表單欄位**:
- Title（文字輸入，必填）
- Description（文字區域，選填）
- IsCompleted（核取方塊）

**驗證**:
- 客戶端: jQuery Validation
- 伺服器端: ModelState 驗證

### 2.7 資料驗證

#### 模型驗證屬性

```csharp
public class ToDo
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Title is required")]
    [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime? CompletedDate { get; set; }
}
```

### 2.8 測試場景

#### 測試案例 1: 建立待辦事項

1. 點擊 "Create New"
2. 輸入標題和描述
3. 提交表單
4. 驗證項目出現在列表中
5. 驗證 CreatedDate 自動設定
6. 驗證 IsCompleted 預設為 false

#### 測試案例 2: 完成待辦事項

1. 編輯未完成的待辦事項
2. 勾選 IsCompleted
3. 儲存
4. 驗證 CompletedDate 自動設定
5. 驗證項目在列表中以完成狀態顯示

#### 測試案例 3: 取消完成

1. 編輯已完成的待辦事項
2. 取消勾選 IsCompleted
3. 儲存
4. 驗證 CompletedDate 被清除

---

## 3. 教材上傳系統 (Teaching Material Upload System)

### 3.1 系統目標

允許教師和管理員上傳課程教材圖片，並在課程列表和詳情頁顯示。

### 3.2 架構設計

```
┌───────────────┐
│CoursesController│
│ Edit Method   │
└───────┬───────┘
        │
        ▼
┌───────────────┐
│  File Upload  │ (IFormFile)
│  Validation   │
└───────┬───────┘
        │
        ▼
┌───────────────┐
│  Delete Old   │ (如果存在)
│    Image      │
└───────┬───────┘
        │
        ▼
┌───────────────┐
│ Generate New  │ (course_{CourseID}_{GUID}.ext)
│  File Name    │
└───────┬───────┘
        │
        ▼
┌───────────────┐
│  Save to FS   │ (/Uploads/TeachingMaterials/)
└───────┬───────┘
        │
        ▼
┌───────────────┐
│  Update DB    │ (TeachingMaterialImagePath)
└───────────────┘
```

### 3.3 檔案上傳規格

#### 支援的檔案格式

- JPG / JPEG
- PNG
- GIF
- BMP

#### 檔案大小限制

**單一檔案**: 5 MB

**請求限制**: 10 MB（Web.config 配置）

**Web.config 配置**:
```xml
<system.web>
    <httpRuntime maxRequestLength="10240" /> <!-- 10MB in KB -->
</system.web>

<system.webServer>
    <security>
        <requestFiltering>
            <requestLimits maxAllowedContentLength="10485760" /> <!-- 10MB in bytes -->
        </requestFiltering>
    </security>
</system.webServer>
```

### 3.4 檔案儲存

#### 儲存路徑

**實體路徑**: `/Uploads/TeachingMaterials/`

**URL 路徑**: `/Uploads/TeachingMaterials/{filename}`

#### 檔案命名規則

**格式**: `course_{CourseID}_{GUID}.{extension}`

**範例**: 
- `course_1050_a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg`
- `course_2021_12345678-abcd-ef12-3456-789012345678.png`

**目的**:
- CourseID: 易於識別所屬課程
- GUID: 確保檔案名唯一性，避免衝突
- Extension: 保留原始檔案格式

### 3.5 上傳流程

#### 步驟 1: 表單提交

**視圖**: `Views/Courses/Edit.cshtml`

**HTML 表單**:
```html
@using (Html.BeginForm("Edit", "Courses", FormMethod.Post, new { enctype = "multipart/form-data" }))
{
    <!-- ... 其他欄位 ... -->
    
    <div class="form-group">
        <label for="teachingMaterial">Teaching Material</label>
        <input type="file" name="teachingMaterial" id="teachingMaterial" 
               accept=".jpg,.jpeg,.png,.gif,.bmp" class="form-control" />
        @Html.ValidationMessage("teachingMaterial", "", new { @class = "text-danger" })
    </div>
    
    @if (!string.IsNullOrEmpty(Model.TeachingMaterialImagePath))
    {
        <div class="form-group">
            <label>Current Teaching Material:</label>
            <img src="@Url.Content(Model.TeachingMaterialImagePath)" 
                 alt="Teaching Material" style="max-width: 200px;" />
        </div>
    }
    
    <!-- ... -->
}
```

#### 步驟 2: 伺服器端驗證

**控制器方法**: `CoursesController.Edit`

**驗證邏輯**:
```csharp
[HttpPost]
[Authorize(Roles = "Admin,Teacher")]
public async Task<ActionResult> Edit(int id, Course course, IFormFile teachingMaterial)
{
    if (teachingMaterial != null && teachingMaterial.Length > 0)
    {
        // 驗證檔案類型
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        var extension = Path.GetExtension(teachingMaterial.FileName).ToLower();
        
        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError("teachingMaterial", 
                "Only image files (JPG, PNG, GIF, BMP) are allowed.");
            return View(course);
        }
        
        // 驗證檔案大小 (5MB)
        if (teachingMaterial.Length > 5 * 1024 * 1024)
        {
            ModelState.AddModelError("teachingMaterial", 
                "File size cannot exceed 5MB.");
            return View(course);
        }
        
        // 處理檔案上傳...
    }
    
    // ... 其他邏輯 ...
}
```

#### 步驟 3: 刪除舊檔案

**邏輯**:
```csharp
if (!string.IsNullOrEmpty(course.TeachingMaterialImagePath))
{
    var oldFilePath = Server.MapPath(course.TeachingMaterialImagePath);
    if (System.IO.File.Exists(oldFilePath))
    {
        System.IO.File.Delete(oldFilePath);
    }
}
```

#### 步驟 4: 儲存新檔案

**邏輯**:
```csharp
// 生成唯一檔案名
var extension = Path.GetExtension(teachingMaterial.FileName);
var fileName = $"course_{course.CourseID}_{Guid.NewGuid()}{extension}";

// 確保目錄存在
var uploadDir = Server.MapPath("~/Uploads/TeachingMaterials");
if (!Directory.Exists(uploadDir))
{
    Directory.CreateDirectory(uploadDir);
}

// 儲存檔案
var filePath = Path.Combine(uploadDir, fileName);
await teachingMaterial.SaveAsAsync(filePath);

// 更新資料庫路徑
course.TeachingMaterialImagePath = $"/Uploads/TeachingMaterials/{fileName}";
```

#### 步驟 5: 更新資料庫

**邏輯**:
```csharp
_context.Entry(course).State = EntityState.Modified;
await _context.SaveChangesAsync();

SendNotification("Course", course.CourseID, "UPDATE", 
    $"Course '{course.Title}' has been updated");
```

### 3.6 前端顯示

#### 課程列表顯示

**視圖**: `Views/Courses/Index.cshtml`

**顯示**: 縮圖 (50x50 像素)

**HTML**:
```html
@if (!string.IsNullOrEmpty(item.TeachingMaterialImagePath))
{
    <img src="@Url.Content(item.TeachingMaterialImagePath)" 
         alt="Teaching Material" 
         style="width: 50px; height: 50px; object-fit: cover;" />
}
else
{
    <span class="text-muted">No image</span>
}
```

#### 課程詳情顯示

**視圖**: `Views/Courses/Details.cshtml`

**顯示**: 完整圖片 (300x300 像素)

**HTML**:
```html
@if (!string.IsNullOrEmpty(Model.TeachingMaterialImagePath))
{
    <div class="form-group">
        <label>Teaching Material:</label>
        <div>
            <img src="@Url.Content(Model.TeachingMaterialImagePath)" 
                 alt="Teaching Material" 
                 class="img-fluid" 
                 style="max-width: 300px; max-height: 300px;" />
        </div>
    </div>
}
```

### 3.7 權限控制

**上傳權限**: Admin 或 Teacher 角色

**實作方式**:
```csharp
[Authorize(Roles = "Admin,Teacher")]
public async Task<ActionResult> Edit(int id, Course course, IFormFile teachingMaterial)
{
    // ...
}
```

**檢查邏輯**:
- 未登入: 重定向到登入頁
- 無權限: 顯示 "Access Denied" 錯誤

### 3.8 錯誤處理

#### 檔案類型錯誤

**訊息**: "Only image files (JPG, PNG, GIF, BMP) are allowed."

**處理**: 顯示錯誤訊息，保留表單資料

#### 檔案過大錯誤

**訊息**: "File size cannot exceed 5MB."

**處理**: 顯示錯誤訊息，保留表單資料

#### 儲存失敗錯誤

**可能原因**:
- 磁碟空間不足
- 目錄權限不足
- 檔案系統錯誤

**處理**:
```csharp
try
{
    await teachingMaterial.SaveAsAsync(filePath);
}
catch (Exception ex)
{
    ModelState.AddModelError("", $"Failed to save file: {ex.Message}");
    return View(course);
}
```

### 3.9 安全考量

#### 檔案類型驗證

- 檢查副檔名
- 可考慮檢查 MIME 類型
- 可考慮檢查檔案內容（魔術數字）

#### 檔案名稱安全

- 使用 GUID 避免路徑遍歷攻擊
- 不直接使用使用者提供的檔案名

#### 存取控制

- 上傳目錄不應允許執行腳本
- Web.config 可配置禁止執行

**範例**:
```xml
<location path="Uploads">
    <system.webServer>
        <handlers>
            <clear />
            <add name="StaticFile" path="*" verb="*" 
                 modules="StaticFileModule" resourceType="File" 
                 requireAccess="Read" />
        </handlers>
    </system.webServer>
</location>
```

### 3.10 測試場景

#### 測試案例 1: 上傳新教材

1. 以 Teacher 角色登入
2. 編輯沒有教材的課程
3. 選擇圖片檔案（JPG, < 5MB）
4. 儲存
5. 驗證圖片顯示在課程列表和詳情頁
6. 驗證檔案存在於 `/Uploads/TeachingMaterials/`

#### 測試案例 2: 替換教材

1. 編輯有教材的課程
2. 上傳新圖片
3. 儲存
4. 驗證新圖片顯示
5. 驗證舊檔案已被刪除

#### 測試案例 3: 檔案類型驗證

1. 嘗試上傳 PDF 檔案
2. 驗證顯示錯誤訊息
3. 驗證檔案未被儲存

#### 測試案例 4: 檔案大小驗證

1. 嘗試上傳 > 5MB 的圖片
2. 驗證顯示錯誤訊息
3. 驗證檔案未被儲存

#### 測試案例 5: 權限驗證

1. 以非 Teacher/Admin 角色登入
2. 嘗試編輯課程並上傳教材
3. 驗證顯示權限錯誤

### 3.11 維護與清理

#### 孤立檔案清理

**問題**: 如果課程被刪除但檔案未刪除，會產生孤立檔案

**解決方案**:
1. 在課程刪除時同時刪除檔案
2. 定期執行清理腳本檢查孤立檔案

**實作** (在 Delete 方法中):
```csharp
[HttpPost]
public async Task<ActionResult> DeleteConfirmed(int id)
{
    var course = await _context.Courses.FindAsync(id);
    
    // 刪除關聯的教材檔案
    if (!string.IsNullOrEmpty(course.TeachingMaterialImagePath))
    {
        var filePath = Server.MapPath(course.TeachingMaterialImagePath);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }
    }
    
    _context.Courses.Remove(course);
    await _context.SaveChangesAsync();
    
    SendNotification("Course", id, "DELETE", 
        $"Course '{course.Title}' has been deleted");
    
    return RedirectToAction("Index");
}
```

---

## 總結

這三個特殊功能展示了系統的多樣性：

1. **即時通知系統**: 使用 MSMQ 實現非同步訊息處理
2. **待辦事項系統**: 使用存儲程序直接資料庫訪問
3. **教材上傳系統**: 檔案管理和安全驗證

每個系統都有其特定的實作方式和最佳實踐，共同構成完整的功能體系。

## 參考文件

- [系統架構概覽](./01-system-architecture-overview.md)
- [功能模組規格](./03-functional-modules-specification.md)
- [技術架構規格](./05-technical-architecture-specification.md)
