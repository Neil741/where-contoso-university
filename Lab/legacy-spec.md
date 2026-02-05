# Contoso University 系統規格文件

## 版本資訊
- **文件版本**: 1.0
- **最後更新**: 2024-01-19
- **維護者**: Documentation Team
- **原始系統**: ContosoUniversity (.NET Framework 4.8.2, ASP.NET MVC 5)

---

## 1. 概述 (Overview)

### 1.1 系統簡介

Contoso University 是一個大學管理系統，用於管理學生、教師、課程、部門等學術資訊。系統提供完整的 CRUD (新增、讀取、更新、刪除) 功能，並包含進階功能如通知系統、教材上傳和待辦事項管理。

### 1.2 系統目標

- 提供完整的學術資訊管理功能
- 支援多角色使用者（管理員、教師、一般使用者）
- 即時通知系統以追蹤重要操作
- 安全可靠的資料存取和檔案管理

### 1.3 系統特點

- **資料持久化**: 使用 Entity Framework Core 3.1.32 與 SQL Server
- **通知機制**: 整合 Microsoft Message Queue (MSMQ) 提供即時通知
- **檔案管理**: 支援教材圖片上傳與管理
- **混合資料存取**: ORM (Entity Framework) + 預存程序並存
- **分頁與排序**: 所有清單頁面支援分頁、排序和搜尋

---

## 2. 系統架構 (System Architecture)

### 2.1 技術堆疊

**前端 (Frontend)**
- ASP.NET MVC 5 Razor Views
- Bootstrap 5.3.3
- jQuery 3.7.1
- jQuery Validation 1.21.0

**後端 (Backend)**
- .NET Framework 4.8.2
- ASP.NET MVC 5.2.9
- Entity Framework Core 3.1.32 *（註：此為不尋常的組合，EF Core 通常搭配 .NET Core/.NET 5+，但此專案使用 EF Core 於 .NET Framework 4.8.2）*
- Microsoft Message Queue (MSMQ)

**資料庫 (Database)**
- SQL Server LocalDB (開發環境)
- Entity Framework Code-First Migrations
- SQL Server 預存程序 (ToDo 功能)

**其他依賴**
- Newtonsoft.Json 13.0.3 (JSON 序列化)
- Microsoft.Data.SqlClient 2.1.4 (資料庫連線)

### 2.2 架構模式

**MVC 架構**
```
Views (Razor) ↔ Controllers ↔ Models/Services ↔ Database
                    ↓
              BaseController
                    ↓
           NotificationService (MSMQ)
```

**資料存取模式**
- Repository Pattern (DbContext)
- Unit of Work (DbContext)
- 直接 ADO.NET (預存程序)

### 2.3 專案結構

```
ContosoUniversity/
├── App_Start/              # 應用程式啟動配置
│   ├── BundleConfig.cs
│   ├── FilterConfig.cs
│   └── RouteConfig.cs
├── Controllers/            # MVC 控制器
│   ├── BaseController.cs
│   ├── HomeController.cs
│   ├── StudentsController.cs
│   ├── InstructorsController.cs
│   ├── CoursesController.cs
│   ├── DepartmentsController.cs
│   ├── NotificationsController.cs
│   └── ToDosController.cs
├── Data/                   # 資料存取層
│   ├── SchoolContext.cs
│   ├── DbInitializer.cs
│   └── SchoolContextFactory.cs
├── Models/                 # 資料模型
│   ├── Student.cs
│   ├── Instructor.cs
│   ├── Course.cs
│   ├── Department.cs
│   ├── Enrollment.cs
│   ├── CourseAssignment.cs
│   ├── OfficeAssignment.cs
│   ├── Notification.cs
│   ├── ToDo.cs
│   └── SchoolViewModels/
├── Services/               # 業務邏輯服務
│   ├── NotificationService.cs
│   └── LoggingService.cs
├── Views/                  # Razor 視圖
├── Content/                # CSS 樣式
├── Scripts/                # JavaScript 腳本
├── Uploads/                # 檔案上傳目錄
│   └── TeachingMaterials/
├── Global.asax             # 應用程式全域事件
└── Web.config              # 組態設定
```

---

## 3. 功能規格 (Functional Specifications)

### 3.1 學生管理 (Student Management)

**功能 ID**: STU-001

#### 3.1.1 功能描述

管理學生資訊，包括個人資料、註冊日期和課程註冊記錄。支援完整的 CRUD 操作，並提供進階的搜尋、排序和分頁功能。

#### 3.1.2 使用者故事

**身為管理員**，我希望能夠：
- 檢視所有學生清單，並依姓名或註冊日期排序
- 搜尋特定姓名的學生
- 新增新學生資料
- 編輯現有學生資訊
- 刪除學生記錄
- 檢視學生的詳細資訊，包括已註冊的課程

#### 3.1.3 接受標準

**學生清單 (Index)**
- ✅ 顯示所有學生，每頁 10 筆資料
- ✅ 支援按姓氏 (Last Name) 排序（升序/降序）
- ✅ 支援按註冊日期 (Enrollment Date) 排序（升序/降序）
- ✅ 支援按姓名搜尋（部分比對）
- ✅ 顯示分頁控制項（上一頁、下一頁、頁碼）
- ✅ 提供「新增學生」、「編輯」、「詳細資料」、「刪除」按鈕

**新增學生 (Create)**
- ✅ 表單包含：姓氏 (Last Name)、名字 (First Name)、註冊日期 (Enrollment Date)
- ✅ 所有欄位為必填
- ✅ 註冊日期必須在 1753-01-01 至 9999-12-31 之間
- ✅ 儲存成功後導向學生清單
- ✅ 發送 CREATE 通知

**編輯學生 (Edit)**
- ✅ 預先填入現有學生資料
- ✅ 允許修改所有欄位
- ✅ 驗證規則與新增相同
- ✅ 儲存成功後導向學生清單
- ✅ 發送 UPDATE 通知

**刪除學生 (Delete)**
- ✅ 顯示確認頁面，包含學生詳細資訊
- ✅ 確認後刪除學生及相關的課程註冊記錄
- ✅ 刪除成功後導向學生清單
- ✅ 發送 DELETE 通知

**學生詳細資料 (Details)**
- ✅ 顯示學生完整資訊
- ✅ 顯示已註冊的課程清單
- ✅ 顯示每門課程的成績（如果已評分）

#### 3.1.4 資料模型

```csharp
public class Student : Person
{
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Enrollment Date")]
    [Required]
    public DateTime EnrollmentDate { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; }
}

// 繼承自 Person
public abstract class Person
{
    public int ID { get; set; }
    
    [Required]
    [StringLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }
    
    [Required]
    [StringLength(50)]
    [Display(Name = "First Name")]
    public string FirstMidName { get; set; }
    
    [Display(Name = "Full Name")]
    public string FullName => $"{LastName}, {FirstMidName}";
}
```

#### 3.1.5 業務規則

- 日期範圍限制：1753-01-01 至 9999-12-31（SQL Server datetime2 限制）
- 姓名最大長度：50 字元
- 刪除學生時級聯刪除其課程註冊記錄
- 所有操作需記錄通知

#### 3.1.6 非功能性需求

- **效能**: 學生清單載入時間 < 2 秒（含 1000 筆資料）
- **分頁**: 每頁固定 10 筆資料
- **搜尋**: 支援姓名部分比對（不區分大小寫）
- **排序**: 預設按姓氏升序排列

---

### 3.2 教師管理 (Instructor Management)

**功能 ID**: INS-001

#### 3.2.1 功能描述

管理教師資訊，包括個人資料、聘僱日期、辦公室分配和課程指派。支援多層次檢視，可同時顯示教師、其授課課程和課程中的學生。

#### 3.2.2 使用者故事

**身為管理員**，我希望能夠：
- 檢視所有教師清單
- 點選教師查看其授課的課程
- 點選課程查看註冊該課程的學生
- 新增新教師並分配辦公室
- 編輯教師資訊和課程指派
- 刪除教師記錄

#### 3.2.3 接受標準

**教師清單 (Index) - 多層檢視**
- ✅ 顯示所有教師的姓名、聘僱日期、辦公室位置
- ✅ 支援分頁（每頁 10 筆）
- ✅ 點選教師後，在右側顯示該教師授課的所有課程
- ✅ 點選課程後，在下方顯示註冊該課程的所有學生及成績
- ✅ 使用 AJAX 或查詢字串實現多層選擇

**新增教師 (Create)**
- ✅ 表單包含：姓氏、名字、聘僱日期、辦公室位置
- ✅ 支援多選課程指派（checkbox 清單）
- ✅ 驗證聘僱日期範圍
- ✅ 辦公室位置為選填
- ✅ 儲存成功後導向教師清單
- ✅ 發送 CREATE 通知

**編輯教師 (Edit)**
- ✅ 預先填入現有教師資料
- ✅ 顯示課程指派狀態（已指派/未指派）
- ✅ 允許修改課程指派
- ✅ 允許修改辦公室位置
- ✅ 儲存成功後導向教師清單
- ✅ 發送 UPDATE 通知

**刪除教師 (Delete)**
- ✅ 顯示確認頁面
- ✅ 顯示警告訊息（如果教師有課程指派）
- ✅ 刪除教師會同時刪除：辦公室分配、課程指派
- ✅ 不會刪除課程本身
- ✅ 發送 DELETE 通知

#### 3.2.4 資料模型

```csharp
public class Instructor : Person
{
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Hire Date")]
    [Required]
    public DateTime HireDate { get; set; }

    public virtual ICollection<CourseAssignment> CourseAssignments { get; set; }
    public virtual OfficeAssignment OfficeAssignment { get; set; }
}

public class OfficeAssignment
{
    [Key]
    [ForeignKey("Instructor")]
    public int InstructorID { get; set; }
    
    [StringLength(50)]
    [Display(Name = "Office Location")]
    public string Location { get; set; }

    public virtual Instructor Instructor { get; set; }
}

public class CourseAssignment
{
    public int InstructorID { get; set; }
    public int CourseID { get; set; }
    
    public virtual Instructor Instructor { get; set; }
    public virtual Course Course { get; set; }
}
```

#### 3.2.5 業務規則

- 一位教師可授課多門課程（多對多關係）
- 一位教師最多一間辦公室（一對一關係）
- 辦公室位置最大長度：50 字元
- 刪除教師時，相關的課程指派和辦公室分配也會被刪除
- 課程本身不會因教師刪除而被刪除

#### 3.2.6 ViewModels

```csharp
public class InstructorIndexData
{
    public IEnumerable<Instructor> Instructors { get; set; }
    public IEnumerable<Course> Courses { get; set; }
    public IEnumerable<Enrollment> Enrollments { get; set; }
}

public class AssignedCourseData
{
    public int CourseID { get; set; }
    public string Title { get; set; }
    public bool Assigned { get; set; }
}
```

---

### 3.3 課程管理 (Course Management)

**功能 ID**: COU-001

#### 3.3.1 功能描述

管理課程資訊，包括課程編號、名稱、學分、所屬部門和教材圖片。支援教材圖片上傳功能，提供完整的 CRUD 操作。

#### 3.3.2 使用者故事

**身為管理員**，我希望能夠：
- 檢視所有課程清單，包括課程編號、名稱、學分和所屬部門
- 新增課程並上傳教材圖片
- 編輯課程資訊並更換教材圖片
- 刪除課程及其教材圖片
- 檢視課程詳細資訊和註冊學生

**身為教師**，我希望能夠：
- 編輯我授課的課程資訊
- 上傳或更換教材圖片

#### 3.3.3 接受標準

**課程清單 (Index)**
- ✅ 顯示所有課程，包括：課程編號、名稱、學分、部門
- ✅ 顯示教材圖片縮圖（50x50px）
- ✅ 支援分頁（每頁 10 筆）
- ✅ 提供「新增課程」、「編輯」、「詳細資料」、「刪除」按鈕

**新增課程 (Create)**
- ✅ 表單包含：課程編號、課程名稱、學分、部門、教材圖片
- ✅ 課程編號為數字，必填
- ✅ 課程名稱長度：3-50 字元
- ✅ 學分範圍：0-5
- ✅ 部門為下拉選單
- ✅ 教材圖片為選填
- ✅ 圖片格式限制：JPG, JPEG, PNG, GIF, BMP
- ✅ 圖片大小限制：5MB
- ✅ 儲存成功後導向課程清單
- ✅ 發送 CREATE 通知

**編輯課程 (Edit)**
- ✅ 預先填入現有課程資料
- ✅ 顯示現有教材圖片（如果有）
- ✅ 允許上傳新圖片替換舊圖片
- ✅ 更換圖片時自動刪除舊圖片
- ✅ 驗證規則與新增相同
- ✅ 儲存成功後導向課程清單
- ✅ 發送 UPDATE 通知

**刪除課程 (Delete)**
- ✅ 顯示確認頁面
- ✅ 顯示課程詳細資訊和教材圖片
- ✅ 刪除課程時自動刪除教材圖片檔案
- ✅ 刪除課程時級聯刪除課程註冊記錄
- ✅ 刪除課程時刪除教師課程指派
- ✅ 發送 DELETE 通知

**課程詳細資料 (Details)**
- ✅ 顯示課程完整資訊
- ✅ 顯示教材圖片（最大 300x300px）
- ✅ 顯示註冊該課程的學生清單

#### 3.3.4 資料模型

```csharp
public class Course
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Display(Name = "Course Number")]
    [Required]
    public int CourseID { get; set; }
    
    [StringLength(50, MinimumLength = 3)]
    [Required]
    public string Title { get; set; }
    
    [Range(0, 5)]
    [Required]
    public int Credits { get; set; }
    
    [Required]
    public int DepartmentID { get; set; }
    
    [StringLength(255)]
    [Display(Name = "Teaching Material Image")]
    public string TeachingMaterialImagePath { get; set; }

    public virtual Department Department { get; set; }
    public virtual ICollection<Enrollment> Enrollments { get; set; }
    public virtual ICollection<CourseAssignment> CourseAssignments { get; set; }
}
```

#### 3.3.5 檔案上傳規格

**儲存路徑**
- 實體路徑：`/Uploads/TeachingMaterials/`
- 檔名格式：`course_{CourseID}_{GUID}.{extension}`
- 資料庫儲存：相對路徑（例如：`/Uploads/TeachingMaterials/course_1050_abc123.jpg`）

**驗證規則**
- 允許的檔案格式：`.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`
- 最大檔案大小：5 MB (5,242,880 bytes)
- 檔案類型檢查：使用 Content-Type 和副檔名雙重驗證

**安全性**
- 使用 GUID 避免檔名衝突
- 驗證檔案類型防止惡意上傳
- 儲存目錄不在 git 版本控制中（.gitignore）
- 需要適當的檔案系統權限

**錯誤處理**
- 檔案過大：顯示「檔案大小超過 5MB 限制」
- 檔案格式錯誤：顯示「僅支援 JPG, PNG, GIF, BMP 格式」
- 儲存失敗：顯示「檔案上傳失敗，請稍後再試」

---

### 3.4 部門管理 (Department Management)

**功能 ID**: DEP-001

#### 3.4.1 功能描述

管理部門資訊，包括部門名稱、預算、成立日期和部門主管。實作並行控制機制，防止多使用者同時編輯造成的資料衝突。

#### 3.4.2 使用者故事

**身為管理員**，我希望能夠：
- 檢視所有部門清單
- 新增新部門並指定部門主管
- 編輯部門資訊
- 刪除部門記錄
- 當發生並行衝突時收到明確的錯誤訊息

#### 3.4.3 接受標準

**部門清單 (Index)**
- ✅ 顯示所有部門，包括：名稱、預算、成立日期、主管姓名
- ✅ 支援分頁
- ✅ 提供「新增部門」、「編輯」、「詳細資料」、「刪除」按鈕

**新增部門 (Create)**
- ✅ 表單包含：部門名稱、預算、成立日期、主管（下拉選單）
- ✅ 部門名稱最大長度：50 字元
- ✅ 預算為貨幣格式
- ✅ 主管從教師清單中選擇
- ✅ 儲存成功後導向部門清單
- ✅ 發送 CREATE 通知

**編輯部門 (Edit)**
- ✅ 預先填入現有部門資料
- ✅ 包含隱藏欄位：RowVersion（並行控制）
- ✅ 允許修改所有欄位
- ✅ 實作樂觀並行控制
- ✅ 衝突時顯示當前資料庫值
- ✅ 允許使用者選擇覆寫或取消
- ✅ 儲存成功後導向部門清單
- ✅ 發送 UPDATE 通知

**刪除部門 (Delete)**
- ✅ 顯示確認頁面
- ✅ 顯示警告訊息（如果部門有課程）
- ✅ 不允許刪除有課程的部門
- ✅ 發送 DELETE 通知

#### 3.4.4 資料模型

```csharp
public class Department
{
    public int DepartmentID { get; set; }
    
    [StringLength(50, MinimumLength = 3)]
    [Required]
    public string Name { get; set; }
    
    [DataType(DataType.Currency)]
    [Column(TypeName = "money")]
    [Required]
    public decimal Budget { get; set; }
    
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Start Date")]
    [Required]
    public DateTime StartDate { get; set; }
    
    [Display(Name = "Administrator")]
    public int? InstructorID { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }

    public virtual Instructor Administrator { get; set; }
    public virtual ICollection<Course> Courses { get; set; }
}
```

#### 3.4.5 並行控制 (Concurrency Control)

**樂觀並行鎖定 (Optimistic Locking)**

使用 `RowVersion` 欄位實作並行控制：

1. **讀取階段**
   - 載入部門資料時，同時載入 RowVersion
   - RowVersion 儲存在隱藏欄位中

2. **更新階段**
   - 提交更新時，檢查 RowVersion 是否與資料庫相符
   - 如果不符，表示其他使用者已修改該筆資料

3. **衝突處理**
   - 捕捉 `DbUpdateConcurrencyException`
   - 顯示當前資料庫值和使用者輸入值
   - 提供選項：
     - 覆寫資料庫值
     - 取消編輯

**實作範例**

```csharp
try
{
    db.Entry(departmentToUpdate).Property("RowVersion").OriginalValue = rowVersion;
    db.SaveChanges();
}
catch (DbUpdateConcurrencyException ex)
{
    var exceptionEntry = ex.Entries.Single();
    var clientValues = (Department)exceptionEntry.Entity;
    var databaseEntry = exceptionEntry.GetDatabaseValues();
    
    if (databaseEntry == null)
    {
        ModelState.AddModelError(string.Empty, "無法儲存變更。該部門已被其他使用者刪除。");
    }
    else
    {
        var databaseValues = (Department)databaseEntry.ToObject();
        // 顯示衝突資訊
        // 允許使用者選擇處理方式
    }
}
```

#### 3.4.6 業務規則

- 部門名稱長度：3-50 字元
- 預算必須為正數
- 一個部門只能有一位主管
- 有課程的部門不可刪除
- 刪除部門時不會刪除主管（教師）

---

### 3.5 通知系統 (Notification System)

**功能 ID**: NOT-001

#### 3.5.1 功能描述

即時通知系統，當實體操作（新增、更新、刪除）發生時，自動通知管理員。使用 Microsoft Message Queue (MSMQ) 作為底層技術，確保訊息可靠傳遞。

#### 3.5.2 使用者故事

**身為管理員**，我希望能夠：
- 當學生、課程、教師或部門被新增時，即時收到通知
- 當實體被更新或刪除時，收到對應的通知
- 查看通知的詳細資訊（實體類型、操作類型、時間）
- 將通知標記為已讀
- 通知自動在一定時間後消失

#### 3.5.3 接受標準

**通知顯示**
- ✅ 通知出現在頁面右上角
- ✅ 不同操作類型使用不同顏色：
  - CREATE：綠色
  - UPDATE：藍色
  - DELETE：橘色
- ✅ 通知包含：實體類型、操作類型、時間戳記
- ✅ 通知自動在 60 秒後消失
- ✅ 使用者可手動關閉通知
- ✅ 最多同時顯示 5 個通知

**通知儀表板**
- ✅ 提供專用的通知頁面
- ✅ 顯示系統說明和快速測試連結
- ✅ 提供通知清單檢視

**後端通知處理**
- ✅ 所有 Controller 繼承自 BaseController
- ✅ 每個 CUD 操作後呼叫 `SendEntityNotification()`
- ✅ 通知發送失敗不影響主要操作
- ✅ 通知訊息存入 MSMQ 佇列
- ✅ 前端每 5 秒輪詢新通知

#### 3.5.4 技術架構

**MSMQ 設定**
- 佇列路徑：`.\Private$\ContosoUniversityNotifications`
- 佇列類型：私有佇列（Private Queue）
- 訊息格式：JSON
- 權限：Everyone（開發環境）

**資料模型**

```csharp
public class Notification
{
    public int ID { get; set; }
    
    [Required]
    [StringLength(50)]
    public string EntityType { get; set; }
    
    [Required]
    [StringLength(50)]
    public string EntityId { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Operation { get; set; }
    
    [StringLength(500)]
    public string Message { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    [StringLength(100)]
    public string CreatedBy { get; set; }
    
    public bool IsRead { get; set; }
}

public enum EntityOperation
{
    CREATE,
    UPDATE,
    DELETE
}
```

**NotificationService**

```csharp
public class NotificationService
{
    private readonly string queuePath;
    private readonly SchoolContext db;

    public void SendNotification(Notification notification)
    {
        // 建立或開啟 MSMQ 佇列
        // 序列化通知為 JSON
        // 發送訊息到佇列
        // 儲存到資料庫
    }

    public List<Notification> ReceiveNotification(int count = 10)
    {
        // 從佇列接收訊息
        // 反序列化 JSON
        // 回傳通知清單
    }

    public void MarkAsRead(int notificationId)
    {
        // 更新通知狀態為已讀
    }
}
```

**BaseController**

```csharp
public class BaseController : Controller
{
    protected SchoolContext db;
    protected NotificationService notificationService;

    protected void SendEntityNotification(string entityType, string entityId, EntityOperation operation)
    {
        try
        {
            var notification = new Notification
            {
                EntityType = entityType,
                EntityId = entityId,
                Operation = operation.ToString(),
                Message = $"{entityType} {operation} operation completed",
                CreatedAt = DateTime.Now,
                CreatedBy = User.Identity.Name,
                IsRead = false
            };
            
            notificationService.SendNotification(notification);
        }
        catch (Exception ex)
        {
            // 記錄錯誤但不中斷主要操作
            Debug.WriteLine($"Notification failed: {ex.Message}");
        }
    }
}
```

#### 3.5.5 前端實作

**notifications.js**

```javascript
$(document).ready(function() {
    // 每 5 秒輪詢一次
    setInterval(checkForNotifications, 5000);
});

function checkForNotifications() {
    $.ajax({
        url: '/Notifications/GetNotifications',
        type: 'GET',
        success: function(data) {
            if (data && data.length > 0) {
                displayNotifications(data);
            }
        }
    });
}

function displayNotifications(notifications) {
    notifications.forEach(function(notification) {
        var cssClass = getNotificationClass(notification.Operation);
        var html = createNotificationHtml(notification, cssClass);
        $('#notification-container').append(html);
        
        // 60 秒後自動移除
        setTimeout(function() {
            $('#notification-' + notification.ID).fadeOut();
        }, 60000);
    });
}
```

#### 3.5.6 系統需求

**Windows 功能**
- Microsoft Message Queue (MSMQ) Server Core
- 不需要 Active Directory 整合
- 不需要 HTTP 支援

**權限**
- 應用程式池身份需要建立私有佇列的權限
- 佇列需要讀寫權限

**效能考量**
- 輪詢間隔：5 秒
- 每次最多接收 10 個通知
- 通知自動過期：60 秒
- 佇列不會無限增長（已讀通知可定期清理）

---

### 3.6 待辦事項 (ToDo List)

**功能 ID**: TODO-001

#### 3.6.1 功能描述

待辦事項管理功能，展示使用 SQL Server 預存程序進行資料存取的方式。與其他功能不同，此功能不使用 Entity Framework ORM，而是直接使用 ADO.NET 呼叫預存程序。

#### 3.6.2 使用者故事

**身為使用者**，我希望能夠：
- 檢視所有待辦事項，未完成的項目顯示在前面
- 新增新的待辦事項
- 編輯待辦事項的內容和狀態
- 將待辦事項標記為已完成
- 刪除待辦事項
- 檢視待辦事項的詳細資訊

#### 3.6.3 接受標準

**待辦清單 (Index)**
- ✅ 顯示所有待辦事項
- ✅ 未完成項目排在前面
- ✅ 依建立日期排序
- ✅ 顯示完成狀態（已完成/未完成）
- ✅ 描述若超過 100 字元則截斷
- ✅ 完成的項目以不同顏色標示（綠色）
- ✅ 支援分頁

**新增待辦 (Create)**
- ✅ 表單包含：標題、描述、完成狀態、建立日期、完成日期
- ✅ 標題為必填，最大長度 200 字元
- ✅ 描述為選填，最大長度 1000 字元
- ✅ 建立日期預設為今天
- ✅ 完成日期在未完成時為空
- ✅ 使用預存程序 `sp_CreateToDo` 建立資料
- ✅ 儲存成功後導向待辦清單

**編輯待辦 (Edit)**
- ✅ 預先填入現有資料
- ✅ 允許修改所有欄位
- ✅ 勾選完成狀態時，可設定完成日期
- ✅ 使用預存程序 `sp_UpdateToDo` 更新資料
- ✅ 儲存成功後導向待辦清單

**刪除待辦 (Delete)**
- ✅ 顯示確認頁面
- ✅ 使用預存程序 `sp_DeleteToDo` 刪除資料
- ✅ 刪除成功後導向待辦清單

**待辦詳細資料 (Details)**
- ✅ 顯示完整資訊
- ✅ 使用預存程序 `sp_GetToDoById` 取得資料

#### 3.6.4 資料模型

```csharp
public class ToDo
{
    public int ID { get; set; }
    
    [Required]
    [StringLength(200)]
    [Display(Name = "Title")]
    public string Title { get; set; }
    
    [StringLength(1000)]
    [Display(Name = "Description")]
    public string Description { get; set; }
    
    [Required]
    [Display(Name = "Is Completed")]
    public bool IsCompleted { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; }
    
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Completed Date")]
    public DateTime? CompletedDate { get; set; }
}
```

#### 3.6.5 預存程序規格

**sp_GetAllToDos**
```sql
CREATE PROCEDURE sp_GetAllToDos
AS
BEGIN
    SELECT ID, Title, Description, IsCompleted, CreatedDate, CompletedDate
    FROM ToDo
    ORDER BY IsCompleted ASC, CreatedDate DESC;
END
```

**sp_GetToDoById**
```sql
CREATE PROCEDURE sp_GetToDoById
    @ID INT
AS
BEGIN
    SELECT ID, Title, Description, IsCompleted, CreatedDate, CompletedDate
    FROM ToDo
    WHERE ID = @ID;
END
```

**sp_CreateToDo**
```sql
CREATE PROCEDURE sp_CreateToDo
    @Title NVARCHAR(200),
    @Description NVARCHAR(1000),
    @IsCompleted BIT,
    @CreatedDate DATETIME2,
    @CompletedDate DATETIME2 = NULL
AS
BEGIN
    INSERT INTO ToDo (Title, Description, IsCompleted, CreatedDate, CompletedDate)
    VALUES (@Title, @Description, @IsCompleted, @CreatedDate, @CompletedDate);
    
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewID;
END
```

**sp_UpdateToDo**
```sql
CREATE PROCEDURE sp_UpdateToDo
    @ID INT,
    @Title NVARCHAR(200),
    @Description NVARCHAR(1000),
    @IsCompleted BIT,
    @CreatedDate DATETIME2,
    @CompletedDate DATETIME2 = NULL
AS
BEGIN
    UPDATE ToDo
    SET Title = @Title,
        Description = @Description,
        IsCompleted = @IsCompleted,
        CreatedDate = @CreatedDate,
        CompletedDate = @CompletedDate
    WHERE ID = @ID;
END
```

**sp_DeleteToDo**
```sql
CREATE PROCEDURE sp_DeleteToDo
    @ID INT
AS
BEGIN
    DELETE FROM ToDo WHERE ID = @ID;
END
```

#### 3.6.6 Controller 實作（ADO.NET）

```csharp
public class ToDosController : BaseController
{
    // Index: 使用 sp_GetAllToDos
    public ActionResult Index()
    {
        var todos = new List<ToDo>();
        using (var command = db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "sp_GetAllToDos";
            command.CommandType = CommandType.StoredProcedure;
            
            db.Database.OpenConnection();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    todos.Add(new ToDo
                    {
                        ID = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                        IsCompleted = reader.GetBoolean(3),
                        CreatedDate = reader.GetDateTime(4),
                        CompletedDate = reader.IsDBNull(5) ? null : (DateTime?)reader.GetDateTime(5)
                    });
                }
            }
        }
        return View(todos);
    }

    // Create: 使用 sp_CreateToDo
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(ToDo todo)
    {
        if (ModelState.IsValid)
        {
            using (var command = db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "sp_CreateToDo";
                command.CommandType = CommandType.StoredProcedure;
                
                command.Parameters.Add(new SqlParameter("@Title", todo.Title));
                command.Parameters.Add(new SqlParameter("@Description", (object)todo.Description ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@IsCompleted", todo.IsCompleted));
                command.Parameters.Add(new SqlParameter("@CreatedDate", todo.CreatedDate));
                command.Parameters.Add(new SqlParameter("@CompletedDate", (object)todo.CompletedDate ?? DBNull.Value));
                
                db.Database.OpenConnection();
                var newId = (int)command.ExecuteScalar();
                
                SendEntityNotification("ToDo", newId.ToString(), EntityOperation.CREATE);
            }
            return RedirectToAction("Index");
        }
        return View(todo);
    }

    // 其他方法類似...
}
```

#### 3.6.7 業務規則

- 標題為必填，最大 200 字元
- 描述為選填，最大 1000 字元
- 建立日期不可為空
- 完成日期在未完成時應為 NULL
- 未完成的項目顯示在已完成項目之前
- 所有資料存取必須透過預存程序

---

### 3.7 統計功能 (Statistics)

**功能 ID**: STAT-001

#### 3.7.1 功能描述

顯示按註冊日期分組的學生統計資訊，以視覺化方式呈現學生註冊趨勢。

#### 3.7.2 接受標準

- ✅ 顯示每個註冊日期的學生數量
- ✅ 按日期降序排列
- ✅ 使用 LINQ 進行分組查詢

#### 3.7.3 資料模型

```csharp
public class EnrollmentDateGroup
{
    [DataType(DataType.Date)]
    public DateTime? EnrollmentDate { get; set; }

    public int StudentCount { get; set; }
}
```

---

## 4. 資料模型 (Data Models)

### 4.1 Entity Relationship Diagram

```
Person (抽象基類)
├── Student
│   └── Enrollments (1:N)
│       └── Course (N:1)
└── Instructor
    ├── OfficeAssignment (1:1)
    ├── CourseAssignments (1:N)
    │   └── Course (N:1)
    └── Department (1:N, 作為 Administrator)

Department (1:N) → Course
Course (N:N) ↔ Instructor (via CourseAssignment)
Course (1:N) → Enrollment (N:1) → Student

Notification (獨立實體)
ToDo (獨立實體)
```

### 4.2 Table-Per-Hierarchy (TPH) 繼承

Person 資料表使用 TPH 繼承模式：

```sql
CREATE TABLE Person (
    ID INT PRIMARY KEY IDENTITY,
    Discriminator NVARCHAR(128) NOT NULL,  -- 'Student' or 'Instructor'
    LastName NVARCHAR(50) NOT NULL,
    FirstMidName NVARCHAR(50) NOT NULL,
    EnrollmentDate DATETIME2 NULL,         -- Student only
    HireDate DATETIME2 NULL                -- Instructor only
);
```

### 4.3 資料庫結構

**主要資料表**
- Person (Students & Instructors)
- Course
- Department
- Enrollment
- CourseAssignment
- OfficeAssignment
- Notification
- ToDo

**關聯**
- Enrollment: StudentID (FK) → Person.ID, CourseID (FK) → Course.CourseID
- CourseAssignment: InstructorID (FK) → Person.ID, CourseID (FK) → Course.CourseID
- OfficeAssignment: InstructorID (FK, PK) → Person.ID
- Course: DepartmentID (FK) → Department.DepartmentID
- Department: InstructorID (FK) → Person.ID (as Administrator)

---

## 5. 介面規格 (Interface Specifications)

### 5.1 API 端點

**NotificationsController**

| 方法 | 路由 | 說明 | 回傳類型 |
|------|------|------|----------|
| GET | /Notifications/Index | 通知儀表板頁面 | View |
| GET | /Notifications/GetNotifications | 取得未讀通知 (AJAX) | JSON |
| POST | /Notifications/MarkAsRead/{id} | 標記通知為已讀 | JSON |

**其他 Controllers**

所有 Controllers 遵循標準的 RESTful MVC 模式：

| 動作 | 路由模式 | HTTP 方法 | 說明 |
|------|----------|-----------|------|
| Index | /{controller} | GET | 清單頁面 |
| Details | /{controller}/Details/{id} | GET | 詳細資料 |
| Create | /{controller}/Create | GET | 新增表單 |
| Create | /{controller}/Create | POST | 提交新增 |
| Edit | /{controller}/Edit/{id} | GET | 編輯表單 |
| Edit | /{controller}/Edit/{id} | POST | 提交編輯 |
| Delete | /{controller}/Delete/{id} | GET | 刪除確認 |
| Delete | /{controller}/Delete/{id} | POST | 確認刪除 |

### 5.2 表單驗證

**客戶端驗證**
- jQuery Validation
- Unobtrusive Validation
- 即時欄位驗證

**伺服器端驗證**
- Data Annotations
- ModelState 驗證
- 自訂驗證邏輯

---

## 6. 非功能性需求 (Non-Functional Requirements)

### 6.1 效能需求

| 需求 | 目標 |
|------|------|
| 頁面載入時間 | < 2 秒（含 1000 筆資料） |
| 資料庫查詢回應 | < 500 毫秒 |
| 檔案上傳速度 | 5MB 檔案 < 10 秒 |
| 通知延遲 | < 10 秒（輪詢間隔 5 秒） |
| 並行使用者 | 支援 100 個並行使用者 |

### 6.2 可用性需求

- **介面語言**: 英文（欄位標籤）
- **響應式設計**: 支援桌面瀏覽器（Bootstrap 5）
- **瀏覽器支援**: Chrome, Firefox, Edge (最新版本)
- **錯誤訊息**: 明確、友善的錯誤提示
- **表單驗證**: 即時反饋，紅色標示錯誤欄位

### 6.3 可靠性需求

- **資料一致性**: 使用交易確保資料完整性
- **並行控制**: 部門管理使用樂觀鎖定
- **錯誤處理**: 所有 Controller Actions 包含 try-catch
- **日誌記錄**: Debug.WriteLine 記錄關鍵操作
- **備份策略**: 資料庫定期備份（部署建議）

### 6.4 安全性需求

- **身份驗證**: Windows Authentication
- **授權**: 基於角色的存取控制（Admin, Teacher）
- **CSRF 防護**: ValidateAntiForgeryToken 在所有 POST 操作
- **SQL 注入防護**: 使用參數化查詢和預存程序
- **檔案上傳安全**: 檔案類型和大小驗證
- **XSS 防護**: Razor 自動編碼輸出

### 6.5 可維護性需求

- **程式碼結構**: 遵循 MVC 架構模式
- **命名規範**: 遵循 C# 命名慣例
- **註解**: 關鍵業務邏輯包含註解
- **版本控制**: Git
- **文件**: README 檔案說明各功能

### 6.6 可擴展性需求

- **模組化設計**: 每個功能獨立的 Controller
- **Service Layer**: NotificationService 可替換
- **資料存取**: 支援 EF Core 和 ADO.NET 並存
- **通知系統**: 可擴展支援 SignalR 或其他推送技術

---

## 7. 約束與假設 (Constraints & Assumptions)

### 7.1 技術約束

**平台限制**
- Windows 作業系統（.NET Framework 4.8 限制）
- IIS Express 或 IIS（ASP.NET MVC 5 hosting）
- SQL Server LocalDB 或 SQL Server（資料庫）
- MSMQ 需要 Windows Pro/Enterprise 版本

**技術組合說明**
- 此專案使用 Entity Framework Core 3.1.32 於 .NET Framework 4.8.2，這是一個不常見但可行的組合
- EF Core 3.1 支援 .NET Standard 2.0，因此可在 .NET Framework 4.8+ 上運行
- 此組合可能是為了在遷移至 .NET Core 前先享受 EF Core 的優勢

**開發工具**
- Visual Studio 2019 或更新版本
- .NET Framework 4.8 Developer Pack
- SQL Server Management Studio（選用）

**瀏覽器**
- 現代瀏覽器（Chrome, Firefox, Edge）
- 不支援 IE 11 以下版本

### 7.2 資料約束

- **日期範圍**: 1753-01-01 至 9999-12-31（SQL Server datetime2）
- **字串長度**: 依 Data Annotations 定義
- **檔案大小**: 教材圖片最大 5MB
- **並行使用者**: 設計支援 100 個並行使用者
- **資料庫大小**: 無硬性限制，建議定期維護

### 7.3 業務假設

- **使用者角色**: 系統預設使用 Windows Authentication
- **管理員**: 需手動設定使用者角色
- **資料初始化**: DbInitializer 提供範例資料
- **MSMQ 可用**: 開發和生產環境都需安裝 MSMQ
- **檔案系統**: 上傳目錄有適當的寫入權限

### 7.4 未來考量

**可能的改進方向**
- 遷移至 .NET 8 以支援跨平台
- 前後端分離架構（React + ASP.NET Core Web API）
- SignalR 取代 MSMQ 實現即時通知
- Azure 部署支援
- Docker 容器化
- CI/CD 自動化部署

---

## 8. 測試規格 (Testing Specifications)

### 8.1 單元測試

**測試範圍**
- Models 驗證邏輯
- NotificationService 方法
- Controller Actions (Mocking)

### 8.2 整合測試

**測試場景**
- 資料庫 CRUD 操作
- 檔案上傳和刪除
- MSMQ 訊息發送接收
- 並行衝突處理

### 8.3 使用者驗收測試 (UAT)

**測試案例**

| 測試 ID | 功能 | 測試步驟 | 預期結果 |
|---------|------|----------|----------|
| UAT-001 | 學生管理 | 新增學生 → 編輯 → 刪除 | 所有操作成功，發送通知 |
| UAT-002 | 教師管理 | 新增教師 → 分配課程 → 檢視多層清單 | 正確顯示教師、課程、學生 |
| UAT-003 | 課程管理 | 新增課程 → 上傳教材圖片 → 編輯 → 刪除 | 圖片正確儲存和刪除 |
| UAT-004 | 部門管理 | 兩個使用者同時編輯同一部門 | 第二個使用者看到並行衝突提示 |
| UAT-005 | 通知系統 | 執行任一 CUD 操作 | 通知出現在右上角，自動消失 |
| UAT-006 | 待辦事項 | 新增待辦 → 標記完成 → 刪除 | 使用預存程序，操作成功 |

---

## 9. 部署需求 (Deployment Requirements)

### 9.1 開發環境

**必要元件**
- Windows 10/11 Pro 或 Enterprise
- Visual Studio 2019+
- .NET Framework 4.8
- SQL Server LocalDB
- MSMQ Server Core

**設定步驟**
1. 啟用 MSMQ 功能
2. 開啟解決方案並還原 NuGet 套件
3. 建置專案
4. 執行應用程式（F5）
5. 資料庫自動初始化

### 9.2 生產環境

**伺服器需求**
- Windows Server 2019 或更新版本
- IIS 10 或更新版本
- .NET Framework 4.8 Runtime
- SQL Server 2017 或更新版本
- MSMQ Server Core

**設定需求**
- 設定 IIS 應用程式池（.NET Framework v4.8）
- 設定資料庫連線字串
- 建立上傳目錄並設定權限
- 安裝並設定 MSMQ
- 設定 MSMQ 佇列權限

**安全性考量**
- 使用 HTTPS
- 限制檔案上傳大小
- 設定適當的資料庫權限
- 監控 MSMQ 佇列長度
- 定期備份資料庫和上傳檔案

---

## 10. 附錄 (Appendices)

### 10.1 資料庫連線字串

**開發環境**
```xml
<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=ContosoUniversityNoAuthEFCore;Integrated Security=True;MultipleActiveResultSets=True" />
</connectionStrings>
```

**生產環境範例**
```xml
<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Data Source=PROD_SERVER;Initial Catalog=ContosoUniversity;User Id=appuser;Password=***;MultipleActiveResultSets=True" />
</connectionStrings>
```

### 10.2 組態設定

**Web.config 關鍵設定**

```xml
<appSettings>
    <!-- MSMQ 佇列路徑 -->
    <add key="NotificationQueuePath" value=".\Private$\ContosoUniversityNotifications"/>
</appSettings>

<system.web>
    <!-- 檔案上傳限制: 10MB -->
    <httpRuntime maxRequestLength="10240" executionTimeout="3600" />
</system.web>

<system.webServer>
    <security>
        <requestFiltering>
            <!-- 檔案上傳限制: 10MB -->
            <requestLimits maxAllowedContentLength="10485760" />
        </requestFiltering>
    </security>
</system.webServer>
```

### 10.3 已知問題與限制

**MSMQ 相關**
- MSMQ 不支援 Windows Home 版本
- 需要手動啟用 Windows 功能
- 私有佇列不需要 Active Directory

**檔案上傳**
- 上傳目錄不在版本控制中
- 需要手動設定檔案系統權限
- 大檔案上傳可能超時

**並行處理**
- 僅部門管理實作並行控制
- 其他實體使用後寫入者獲勝策略

**效能**
- 通知使用輪詢而非推送（5秒間隔）
- 大量資料時分頁效能可能下降

### 10.4 參考文件

**官方文件**
- [ASP.NET MVC 5 Documentation](https://docs.microsoft.com/en-us/aspnet/mvc/overview/getting-started/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [MSMQ Documentation](https://docs.microsoft.com/en-us/previous-versions/windows/desktop/msmq/)

**內部文件**
- BUILD_REQUIREMENTS.md
- NOTIFICATION_SYSTEM_README.md
- TEACHING_MATERIAL_UPLOAD.md
- TODO_FEATURE_README.md
- SETUP_TESTING_GUIDE.md

---

## 11. 變更記錄 (Change Log)

| 版本 | 日期 | 變更內容 | 作者 |
|------|------|----------|------|
| 1.0 | 2024-01-19 | 初始版本 - 從 ContosoUniversity 程式碼提取需求 | Documentation Team |

