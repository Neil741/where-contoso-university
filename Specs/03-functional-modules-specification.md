# 功能模組規格

## 概述

本文件詳細描述 Contoso University 系統的核心功能模組，包括學生管理、課程管理、講師管理、部門管理和統計報表功能。

## 模組架構

```
┌─────────────────────────────────────────────────────┐
│                   首頁模組                            │
│  - 歡迎頁面                                          │
│  - 註冊統計儀表板                                     │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│                 學生管理模組                          │
│  - 學生列表（分頁、搜尋、排序）                       │
│  - 學生詳情                                          │
│  - 新增學生                                          │
│  - 編輯學生                                          │
│  - 刪除學生                                          │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│                 課程管理模組                          │
│  - 課程列表（分頁）                                   │
│  - 課程詳情                                          │
│  - 新增課程                                          │
│  - 編輯課程（含教材上傳）                             │
│  - 刪除課程                                          │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│                 講師管理模組                          │
│  - 講師列表（含課程和註冊資訊）                       │
│  - 講師詳情                                          │
│  - 新增講師                                          │
│  - 編輯講師（含課程分配和辦公室）                     │
│  - 刪除講師                                          │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│                 部門管理模組                          │
│  - 部門列表                                          │
│  - 部門詳情                                          │
│  - 新增部門                                          │
│  - 編輯部門                                          │
│  - 刪除部門                                          │
└─────────────────────────────────────────────────────┘
```

---

## 1. 首頁模組 (HomeController)

### 1.1 模組目標

提供系統歡迎頁面和學生註冊統計資訊的視覺化儀表板。

### 1.2 功能列表

#### 功能 1.1: 首頁顯示

**路由**: `/` 或 `/Home/Index`

**描述**: 顯示系統歡迎頁面

**使用者介面**:
- 系統標題和歡迎訊息
- 導航連結到各功能模組

**控制器方法**: `Index()`

**視圖**: `Views/Home/Index.cshtml`

**業務邏輯**:
- 無特殊業務邏輯
- 純展示頁面

#### 功能 1.2: 關於頁面

**路由**: `/Home/About`

**描述**: 顯示學生按註冊日期分組的統計資訊

**使用者介面**:
- 標題: "Student Body Statistics"
- 表格顯示：
  - 註冊日期
  - 該日期註冊的學生數量

**控制器方法**: `About()`

**視圖**: `Views/Home/About.cshtml`

**資料模型**: `EnrollmentDateGroup`

**業務邏輯**:
```csharp
var data = from student in _context.Students
           group student by student.EnrollmentDate into dateGroup
           select new EnrollmentDateGroup()
           {
               EnrollmentDate = dateGroup.Key,
               StudentCount = dateGroup.Count()
           };
```

**資料驗證**: 無

**權限控制**: 無限制

---

## 2. 學生管理模組 (StudentsController)

### 2.1 模組目標

提供完整的學生資訊 CRUD 操作，包括分頁、搜尋和排序功能。

### 2.2 功能列表

#### 功能 2.1: 學生列表

**路由**: `/Students` 或 `/Students/Index`

**描述**: 顯示學生列表，支援分頁、搜尋和排序

**使用者介面**:
- 搜尋框（依姓名搜尋）
- 表格顯示：
  - Last Name（可排序）
  - First Name（可排序）
  - Enrollment Date（可排序）
  - 操作按鈕（Edit, Details, Delete）
- 分頁導航（上一頁、下一頁、頁碼）
- "Create New" 按鈕

**控制器方法**: `Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)`

**URL 參數**:
- `sortOrder`: 排序欄位和方向（如 "name_desc"）
- `currentFilter`: 當前搜尋條件
- `searchString`: 新的搜尋字串
- `pageNumber`: 頁碼（預設 1）

**業務邏輯**:
1. 獲取所有學生資料
2. 如果有搜尋字串，依 LastName 或 FirstMidName 過濾
3. 根據 sortOrder 排序（預設 LastName 升冪）
4. 使用 PaginatedList 進行分頁（每頁 3 筆）

**資料驗證**: 無

**權限控制**: 無限制

**通知觸發**: 無

#### 功能 2.2: 學生詳情

**路由**: `/Students/Details/{id}`

**描述**: 顯示特定學生的詳細資訊和註冊課程

**使用者介面**:
- 學生基本資訊：
  - First Name
  - Last Name
  - Enrollment Date
- 註冊課程列表：
  - 課程名稱
  - 成績
- "Back to List" 和 "Edit" 按鈕

**控制器方法**: `Details(int? id)`

**業務邏輯**:
1. 根據 ID 查詢學生
2. 使用 Include 預先載入 Enrollments 和 Course
3. 如果找不到，返回 NotFound

**資料驗證**: 
- ID 必須提供且有效

**權限控制**: 無限制

**通知觸發**: 無

#### 功能 2.3: 新增學生

**路由**: 
- GET: `/Students/Create`
- POST: `/Students/Create`

**描述**: 建立新學生記錄

**使用者介面**:
- 表單欄位：
  - Last Name（必填）
  - First Name（必填）
  - Enrollment Date（必填）
- "Create" 和 "Back to List" 按鈕

**控制器方法**: 
- GET: `Create()`
- POST: `Create([Bind("LastName,FirstMidName,EnrollmentDate")] Student student)`

**業務邏輯**:
1. 驗證模型狀態
2. 將學生添加到 DbContext
3. 儲存變更
4. 發送創建通知到 MSMQ
5. 重定向到 Index

**資料驗證**:
- LastName: 必填，最大長度 50
- FirstMidName: 必填，最大長度 50
- EnrollmentDate: 必填，有效日期

**權限控制**: 無限制

**通知觸發**: 
- 操作: CREATE
- 實體: Student
- 訊息: "Student '{LastName}' has been created"

**錯誤處理**:
- 模型驗證失敗: 重新顯示表單
- 資料庫錯誤: 顯示錯誤訊息

#### 功能 2.4: 編輯學生

**路由**: 
- GET: `/Students/Edit/{id}`
- POST: `/Students/Edit/{id}`

**描述**: 編輯現有學生資訊

**使用者介面**:
- 同新增學生表單，但預填現有資料
- "Save" 和 "Back to List" 按鈕

**控制器方法**: 
- GET: `Edit(int? id)`
- POST: `Edit(int id, [Bind("ID,LastName,FirstMidName,EnrollmentDate")] Student student)`

**業務邏輯**:
1. GET: 根據 ID 查詢學生並顯示
2. POST: 驗證模型狀態
3. 標記實體為已修改
4. 儲存變更
5. 發送更新通知到 MSMQ
6. 重定向到 Index

**資料驗證**:
- 同新增學生
- ID 必須匹配 URL 參數

**權限控制**: 無限制

**通知觸發**: 
- 操作: UPDATE
- 實體: Student
- 訊息: "Student '{LastName}' has been updated"

**錯誤處理**:
- 並發衝突: 顯示錯誤訊息
- 找不到記錄: 返回 NotFound

#### 功能 2.5: 刪除學生

**路由**: 
- GET: `/Students/Delete/{id}`
- POST: `/Students/Delete/{id}`

**描述**: 刪除學生記錄（含確認頁面）

**使用者介面**:
- GET: 顯示學生資訊和確認訊息
- POST: 執行刪除後重定向

**控制器方法**: 
- GET: `Delete(int? id)`
- POST: `DeleteConfirmed(int id)`

**業務邏輯**:
1. GET: 根據 ID 查詢並顯示學生資訊
2. POST: 查詢學生
3. 從 DbContext 移除學生
4. 儲存變更
5. 發送刪除通知到 MSMQ
6. 重定向到 Index

**資料驗證**: 
- ID 必須有效

**權限控制**: 無限制

**通知觸發**: 
- 操作: DELETE
- 實體: Student
- 訊息: "Student '{LastName}' has been deleted"

**錯誤處理**:
- 找不到記錄: 返回 NotFound
- 外鍵約束: 顯示錯誤訊息

---

## 3. 課程管理模組 (CoursesController)

### 3.1 模組目標

管理課程資訊，包括課程基本資料、部門關聯和教材上傳功能。

### 3.2 功能列表

#### 功能 3.1: 課程列表

**路由**: `/Courses` 或 `/Courses/Index`

**描述**: 顯示所有課程的分頁列表

**使用者介面**:
- 表格顯示：
  - Course Number
  - Title
  - Credits
  - Department
  - Teaching Material（縮圖 50x50px）
  - 操作按鈕（Edit, Details, Delete）
- 分頁導航
- "Create New" 按鈕

**控制器方法**: `Index(int? pageNumber)`

**業務邏輯**:
1. 查詢所有課程
2. Include Department 資訊
3. 使用 PaginatedList 分頁（每頁大小可配置）

**資料驗證**: 無

**權限控制**: 無限制

**通知觸發**: 無

#### 功能 3.2: 課程詳情

**路由**: `/Courses/Details/{id}`

**描述**: 顯示課程詳細資訊、註冊學生和授課講師

**使用者介面**:
- 課程基本資訊：
  - Course Number
  - Title
  - Credits
  - Department
  - Teaching Material（全圖 300x300px）
- 註冊學生列表（姓名、成績）
- 授課講師列表（姓名）
- "Back to List" 和 "Edit" 按鈕

**控制器方法**: `Details(int? id)`

**業務邏輯**:
1. 根據 ID 查詢課程
2. Include: Department, Enrollments.Student, CourseAssignments.Instructor
3. 如果找不到，返回 NotFound

**資料驗證**: 
- ID 必須提供且有效

**權限控制**: 無限制

**通知觸發**: 無

#### 功能 3.3: 新增課程

**路由**: 
- GET: `/Courses/Create`
- POST: `/Courses/Create`

**描述**: 建立新課程記錄

**使用者介面**:
- 表單欄位：
  - Course Number（必填）
  - Title（必填）
  - Credits（必填，0-5）
  - Department（下拉選單，必填）
- "Create" 和 "Back to List" 按鈕

**控制器方法**: 
- GET: `Create()`
- POST: `Create([Bind("CourseID,Title,Credits,DepartmentID")] Course course)`

**業務邏輯**:
1. GET: 載入部門列表到 ViewData
2. POST: 驗證模型狀態
3. 添加課程到 DbContext
4. 儲存變更
5. 發送創建通知到 MSMQ
6. 重定向到 Index

**資料驗證**:
- CourseID: 必填，唯一
- Title: 必填，最大長度 50
- Credits: 必填，範圍 0-5
- DepartmentID: 必填，必須存在

**權限控制**: 無限制

**通知觸發**: 
- 操作: CREATE
- 實體: Course
- 訊息: "Course '{Title}' has been created"

#### 功能 3.4: 編輯課程（含教材上傳）

**路由**: 
- GET: `/Courses/Edit/{id}`
- POST: `/Courses/Edit/{id}`

**描述**: 編輯課程資訊，包括上傳教材圖片

**使用者介面**:
- 同新增課程表單，但預填現有資料
- 額外欄位：
  - 教材圖片上傳（檔案選擇器）
  - 當前教材預覽（如果存在）
- "Save" 和 "Back to List" 按鈕

**控制器方法**: 
- GET: `Edit(int? id)`
- POST: `Edit(int id, [Bind("CourseID,Title,Credits,DepartmentID")] Course course, IFormFile teachingMaterial)`

**業務邏輯**:
1. GET: 根據 ID 查詢課程並顯示
2. POST: 驗證模型狀態
3. 如果有上傳教材：
   - 驗證檔案類型（JPG, JPEG, PNG, GIF, BMP）
   - 驗證檔案大小（最大 5MB）
   - 刪除舊的教材檔案（如果存在）
   - 生成新檔案名：`course_{CourseID}_{GUID}.{extension}`
   - 儲存到 `/Uploads/TeachingMaterials/`
   - 更新 TeachingMaterialImagePath
4. 更新課程資料
5. 儲存變更
6. 發送更新通知到 MSMQ
7. 重定向到 Index

**資料驗證**:
- 同新增課程
- 教材檔案（如果提供）：
  - 格式: JPG, JPEG, PNG, GIF, BMP
  - 大小: 最大 5MB

**權限控制**: 
- 需要 Admin 或 Teacher 角色才能上傳教材

**通知觸發**: 
- 操作: UPDATE
- 實體: Course
- 訊息: "Course '{Title}' has been updated"

**錯誤處理**:
- 檔案類型不支援: 顯示錯誤訊息
- 檔案過大: 顯示錯誤訊息
- 儲存失敗: 顯示錯誤訊息

#### 功能 3.5: 刪除課程

**路由**: 
- GET: `/Courses/Delete/{id}`
- POST: `/Courses/Delete/{id}`

**描述**: 刪除課程記錄（含確認）

**控制器方法**: 
- GET: `Delete(int? id)`
- POST: `DeleteConfirmed(int id)`

**業務邏輯**:
1. GET: 根據 ID 查詢並顯示課程資訊
2. POST: 查詢課程
3. 刪除關聯的教材檔案（如果存在）
4. 從 DbContext 移除課程
5. 儲存變更
6. 發送刪除通知到 MSMQ
7. 重定向到 Index

**通知觸發**: 
- 操作: DELETE
- 實體: Course
- 訊息: "Course '{Title}' has been deleted"

---

## 4. 講師管理模組 (InstructorsController)

### 4.1 模組目標

管理講師資訊，包括課程分配和辦公室分配功能。

### 4.2 功能列表

#### 功能 4.1: 講師列表（含關聯資料）

**路由**: `/Instructors` 或 `/Instructors/Index`

**描述**: 顯示講師列表，並可查看其授課課程和該課程的註冊學生

**使用者介面**:
- 講師表格：
  - Last Name
  - First Name
  - Hire Date
  - Office Location
  - Courses（授課課程列表）
  - 操作按鈕（Select, Edit, Delete）
- 選定講師時，顯示其授課課程
- 選定課程時，顯示註冊該課程的學生
- "Create New" 按鈕

**控制器方法**: `Index(int? id, int? courseID)`

**URL 參數**:
- `id`: 選定的講師 ID
- `courseID`: 選定的課程 ID

**業務邏輯**:
1. 查詢所有講師
2. Include: OfficeAssignment, CourseAssignments.Course
3. 如果提供 id，查詢該講師的課程
4. 如果提供 courseID，查詢該課程的註冊學生
5. 將資料封裝到 InstructorIndexData 視圖模型

**視圖模型**: `InstructorIndexData`
- Instructors: IEnumerable<Instructor>
- Courses: IEnumerable<Course>
- Enrollments: IEnumerable<Enrollment>

**資料驗證**: 無

**權限控制**: 無限制

**通知觸發**: 無

#### 功能 4.2: 講師詳情

**路由**: `/Instructors/Details/{id}`

**描述**: 顯示講師詳細資訊

**控制器方法**: `Details(int? id)`

#### 功能 4.3: 新增講師

**路由**: 
- GET: `/Instructors/Create`
- POST: `/Instructors/Create`

**描述**: 建立新講師記錄，可同時分配課程

**使用者介面**:
- 表單欄位：
  - Last Name（必填）
  - First Name（必填）
  - Hire Date（必填）
  - Office Location（選填）
  - Courses（多選核取方塊）
- "Create" 和 "Back to List" 按鈕

**控制器方法**: 
- GET: `Create()`
- POST: `Create([Bind("FirstMidName,LastName,HireDate,OfficeAssignment")] Instructor instructor, string[] selectedCourses)`

**業務邏輯**:
1. GET: 載入所有課程用於分配
2. POST: 驗證模型狀態
3. 如果提供 selectedCourses，建立 CourseAssignment 關聯
4. 如果提供 Office Location，建立 OfficeAssignment
5. 添加講師到 DbContext
6. 儲存變更
7. 發送創建通知到 MSMQ
8. 重定向到 Index

**通知觸發**: 
- 操作: CREATE
- 實體: Instructor
- 訊息: "Instructor '{LastName}' has been created"

#### 功能 4.4: 編輯講師

**路由**: 
- GET: `/Instructors/Edit/{id}`
- POST: `/Instructors/Edit/{id}`

**描述**: 編輯講師資訊、課程分配和辦公室分配

**控制器方法**: 
- GET: `Edit(int? id)`
- POST: `Edit(int id, [Bind("ID,FirstMidName,LastName,HireDate,OfficeAssignment")] Instructor instructor, string[] selectedCourses)`

**業務邏輯**:
1. GET: 查詢講師及其課程分配
2. 載入所有課程並標記已分配的
3. POST: 驗證模型狀態
4. 更新課程分配（添加新的，移除取消的）
5. 更新辦公室分配
6. 儲存變更
7. 發送更新通知到 MSMQ
8. 重定向到 Index

**通知觸發**: 
- 操作: UPDATE
- 實體: Instructor
- 訊息: "Instructor '{LastName}' has been updated"

#### 功能 4.5: 刪除講師

**路由**: 
- GET: `/Instructors/Delete/{id}`
- POST: `/Instructors/Delete/{id}`

**描述**: 刪除講師記錄

**控制器方法**: 
- GET: `Delete(int? id)`
- POST: `DeleteConfirmed(int id)`

**業務邏輯**:
1. 檢查講師是否為任何部門的主任
2. 如果是，需要先移除部門主任關聯
3. 刪除講師記錄（級聯刪除課程分配和辦公室分配）
4. 發送刪除通知到 MSMQ

**通知觸發**: 
- 操作: DELETE
- 實體: Instructor
- 訊息: "Instructor '{LastName}' has been deleted"

**錯誤處理**:
- 講師為部門主任: 顯示錯誤訊息

---

## 5. 部門管理模組 (DepartmentsController)

### 5.1 模組目標

管理學術部門資訊，包括預算和主任分配。

### 5.2 功能列表

#### 功能 5.1: 部門列表

**路由**: `/Departments` 或 `/Departments/Index`

**描述**: 顯示所有部門的列表

**使用者介面**:
- 表格顯示：
  - Name
  - Budget（貨幣格式）
  - Start Date
  - Administrator（主任姓名）
  - 操作按鈕（Edit, Details, Delete）
- "Create New" 按鈕

**控制器方法**: `Index()`

**業務邏輯**:
1. 查詢所有部門
2. Include Administrator 資訊

#### 功能 5.2: 部門詳情

**路由**: `/Departments/Details/{id}`

**描述**: 顯示部門詳細資訊和該部門的課程

**控制器方法**: `Details(int? id)`

#### 功能 5.3: 新增部門

**路由**: 
- GET: `/Departments/Create`
- POST: `/Departments/Create`

**描述**: 建立新部門記錄

**使用者介面**:
- 表單欄位：
  - Name（必填）
  - Budget（必填，貨幣格式）
  - Start Date（必填）
  - Administrator（下拉選單，選填）
- "Create" 和 "Back to List" 按鈕

**控制器方法**: 
- GET: `Create()`
- POST: `Create([Bind("Name,Budget,StartDate,InstructorID")] Department department)`

**業務邏輯**:
1. GET: 載入講師列表用於選擇主任
2. POST: 驗證模型狀態
3. 添加部門到 DbContext
4. 儲存變更
5. 發送創建通知到 MSMQ
6. 重定向到 Index

**通知觸發**: 
- 操作: CREATE
- 實體: Department
- 訊息: "Department '{Name}' has been created"

#### 功能 5.4: 編輯部門

**路由**: 
- GET: `/Departments/Edit/{id}`
- POST: `/Departments/Edit/{id}`

**描述**: 編輯部門資訊，包含並發控制

**控制器方法**: 
- GET: `Edit(int? id)`
- POST: `Edit(int id, byte[] rowVersion, [Bind("DepartmentID,Name,Budget,StartDate,InstructorID,RowVersion")] Department department)`

**業務邏輯**:
1. GET: 查詢部門並顯示
2. POST: 驗證模型狀態
3. 檢查 RowVersion 以防止並發衝突
4. 更新部門資料
5. 儲存變更
6. 發送更新通知到 MSMQ
7. 重定向到 Index

**通知觸發**: 
- 操作: UPDATE
- 實體: Department
- 訊息: "Department '{Name}' has been updated"

**錯誤處理**:
- 並發衝突: 顯示當前值和嘗試更新的值，讓使用者選擇

#### 功能 5.5: 刪除部門

**路由**: 
- GET: `/Departments/Delete/{id}`
- POST: `/Departments/Delete/{id}`

**描述**: 刪除部門記錄（含並發控制）

**控制器方法**: 
- GET: `Delete(int? id)`
- POST: `DeleteConfirmed(int id, byte[] rowVersion)`

**業務邏輯**:
1. 檢查部門是否有課程
2. 檢查 RowVersion 以防止並發衝突
3. 刪除部門記錄
4. 發送刪除通知到 MSMQ

**通知觸發**: 
- 操作: DELETE
- 實體: Department
- 訊息: "Department '{Name}' has been deleted"

**錯誤處理**:
- 部門有課程: 顯示錯誤訊息
- 並發衝突: 顯示錯誤訊息

---

## 共用功能

### 分頁機制

**實作**: PaginatedList<T> 類別

**功能**:
- 支援任何類型的集合分頁
- 計算總頁數
- 判斷是否有上一頁/下一頁
- 非同步資料載入

**使用方式**:
```csharp
var students = await PaginatedList<Student>.CreateAsync(
    studentsIQ.AsNoTracking(), 
    pageNumber ?? 1, 
    pageSize);
```

### 通知機制

**觸發時機**: 所有 CREATE、UPDATE、DELETE 操作

**實作位置**: BaseController

**通知方法**:
```csharp
protected void SendNotification(string entityType, int entityId, string operation, string message)
```

**通知格式**:
```json
{
    "EntityType": "Student",
    "EntityId": 123,
    "Operation": "CREATE",
    "Message": "Student 'Smith' has been created",
    "CreatedAt": "2026-02-05T10:30:00Z"
}
```

### 錯誤處理

**全域錯誤處理**: 在 Global.asax.cs 中配置

**模型驗證**: 
- 伺服器端: ModelState.IsValid
- 客戶端: jQuery Validation

**資料庫錯誤**: 
- 捕獲 DbUpdateException
- 顯示使用者友善的錯誤訊息

## 使用者體驗考量

### 回應提示

- 操作成功: 重定向到列表頁
- 操作失敗: 顯示錯誤訊息並保留表單資料

### 資料載入策略

- 列表頁: 延遲載入（Lazy Loading）
- 詳情頁: 預先載入（Eager Loading with Include）

### 效能優化

- 使用 AsNoTracking() 進行唯讀查詢
- 分頁減少資料傳輸量
- 選擇性載入關聯資料

## 參考文件

- [系統架構概覽](./01-system-architecture-overview.md)
- [資料模型規格](./02-data-model-specification.md)
- [特殊功能規格](./04-special-features-specification.md)
