# 資料庫規格

## 概述

本文件詳細描述 Contoso University 系統的資料庫架構，包括資料表結構、存儲程序、索引策略和初始化流程。

---

## 1. 資料庫概覽

### 1.1 基本資訊

**資料庫名稱**: ContosoUniversityNoAuthEFCore

**資料庫引擎**: SQL Server

**字元集**: UTF-8

**排序規則**: SQL_Latin1_General_CP1_CI_AS

**資料庫版本相容性**: SQL Server 2016 或更高版本

### 1.2 資料庫架構方法

**開發方法**: Code-First (Entity Framework Core)

**優點**:
- 程式碼即為真實來源
- 自動化遷移
- 強型別查詢
- LINQ 支援

**遷移管理**: Entity Framework Core Migrations

---

## 2. 資料表結構

### 2.1 People (人員基礎表)

**說明**: 使用 Table Per Hierarchy (TPH) 繼承策略存儲學生和講師

**資料表名稱**: `People`

| 欄位名稱 | 資料類型 | 長度 | 允許 NULL | 預設值 | 說明 |
|---------|----------|------|-----------|--------|------|
| ID | int | - | 否 | IDENTITY(1,1) | 主鍵 |
| LastName | nvarchar | 50 | 否 | - | 姓氏 |
| FirstMidName | nvarchar | 50 | 否 | - | 名字（含中間名） |
| EnrollmentDate | datetime2 | - | 是 | - | 學生註冊日期 |
| HireDate | datetime2 | - | 是 | - | 講師聘用日期 |
| Discriminator | nvarchar | - | 否 | - | 區分類型 (Student/Instructor) |

**主鍵**: `PK_People` ON `ID`

**索引**:
- `IX_People_LastName` ON `LastName` (非叢集)
- `IX_People_FirstMidName` ON `FirstMidName` (非叢集)

**檢查約束**:
```sql
-- 學生必須有 EnrollmentDate
ALTER TABLE People ADD CONSTRAINT CK_People_Student 
CHECK (Discriminator != 'Student' OR EnrollmentDate IS NOT NULL)

-- 講師必須有 HireDate
ALTER TABLE People ADD CONSTRAINT CK_People_Instructor 
CHECK (Discriminator != 'Instructor' OR HireDate IS NOT NULL)
```

**建立指令碼**:
```sql
CREATE TABLE [dbo].[People] (
    [ID] INT IDENTITY(1,1) NOT NULL,
    [LastName] NVARCHAR(50) NOT NULL,
    [FirstMidName] NVARCHAR(50) NOT NULL,
    [EnrollmentDate] DATETIME2 NULL,
    [HireDate] DATETIME2 NULL,
    [Discriminator] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT [PK_People] PRIMARY KEY CLUSTERED ([ID])
)
```

### 2.2 Departments (部門表)

**說明**: 存儲學術部門資訊

**資料表名稱**: `Departments`

| 欄位名稱 | 資料類型 | 長度 | 允許 NULL | 預設值 | 說明 |
|---------|----------|------|-----------|--------|------|
| DepartmentID | int | - | 否 | IDENTITY(1,1) | 主鍵 |
| Name | nvarchar | 50 | 否 | - | 部門名稱 |
| Budget | money | - | 否 | - | 部門預算 |
| StartDate | datetime2 | - | 否 | - | 部門成立日期 |
| InstructorID | int | - | 是 | - | 部門主任 ID (外鍵) |
| RowVersion | rowversion | - | 否 | - | 並發控制 |

**主鍵**: `PK_Departments` ON `DepartmentID`

**外鍵**:
- `FK_Departments_People_InstructorID` 
  - 參照: `People(ID)`
  - ON DELETE: SET NULL

**索引**:
- `IX_Departments_InstructorID` ON `InstructorID` (非叢集)
- `IX_Departments_Name` ON `Name` (非叢集, 唯一)

**建立指令碼**:
```sql
CREATE TABLE [dbo].[Departments] (
    [DepartmentID] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    [Budget] MONEY NOT NULL,
    [StartDate] DATETIME2 NOT NULL,
    [InstructorID] INT NULL,
    [RowVersion] ROWVERSION NOT NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY CLUSTERED ([DepartmentID]),
    CONSTRAINT [FK_Departments_People_InstructorID] 
        FOREIGN KEY ([InstructorID]) REFERENCES [People]([ID]) ON DELETE SET NULL
)

CREATE NONCLUSTERED INDEX [IX_Departments_InstructorID] 
    ON [Departments]([InstructorID])
```

### 2.3 Courses (課程表)

**說明**: 存儲課程資訊

**資料表名稱**: `Courses`

| 欄位名稱 | 資料類型 | 長度 | 允許 NULL | 預設值 | 說明 |
|---------|----------|------|-----------|--------|------|
| CourseID | int | - | 否 | - | 主鍵（不自動遞增） |
| Title | nvarchar | 50 | 否 | - | 課程名稱 |
| Credits | int | - | 否 | - | 學分數 (0-5) |
| DepartmentID | int | - | 否 | - | 所屬部門 ID (外鍵) |
| TeachingMaterialImagePath | nvarchar | 255 | 是 | - | 教材圖片路徑 |

**主鍵**: `PK_Courses` ON `CourseID`

**外鍵**:
- `FK_Courses_Departments_DepartmentID` 
  - 參照: `Departments(DepartmentID)`
  - ON DELETE: CASCADE

**索引**:
- `IX_Courses_DepartmentID` ON `DepartmentID` (非叢集)
- `IX_Courses_Title` ON `Title` (非叢集)

**檢查約束**:
```sql
ALTER TABLE Courses ADD CONSTRAINT CK_Courses_Credits 
CHECK (Credits >= 0 AND Credits <= 5)
```

**建立指令碼**:
```sql
CREATE TABLE [dbo].[Courses] (
    [CourseID] INT NOT NULL,
    [Title] NVARCHAR(50) NOT NULL,
    [Credits] INT NOT NULL,
    [DepartmentID] INT NOT NULL,
    [TeachingMaterialImagePath] NVARCHAR(255) NULL,
    CONSTRAINT [PK_Courses] PRIMARY KEY CLUSTERED ([CourseID]),
    CONSTRAINT [FK_Courses_Departments_DepartmentID] 
        FOREIGN KEY ([DepartmentID]) REFERENCES [Departments]([DepartmentID]) ON DELETE CASCADE,
    CONSTRAINT [CK_Courses_Credits] CHECK ([Credits] >= 0 AND [Credits] <= 5)
)

CREATE NONCLUSTERED INDEX [IX_Courses_DepartmentID] 
    ON [Courses]([DepartmentID])
```

### 2.4 Enrollments (課程註冊表)

**說明**: 學生課程註冊關聯表，包含成績資訊

**資料表名稱**: `Enrollments`

| 欄位名稱 | 資料類型 | 長度 | 允許 NULL | 預設值 | 說明 |
|---------|----------|------|-----------|--------|------|
| EnrollmentID | int | - | 否 | IDENTITY(1,1) | 主鍵 |
| CourseID | int | - | 否 | - | 課程 ID (外鍵) |
| StudentID | int | - | 否 | - | 學生 ID (外鍵) |
| Grade | int | - | 是 | - | 成績枚舉 (0=A, 1=B, 2=C, 3=D, 4=F) |

**主鍵**: `PK_Enrollments` ON `EnrollmentID`

**外鍵**:
- `FK_Enrollments_Courses_CourseID` 
  - 參照: `Courses(CourseID)`
  - ON DELETE: CASCADE
- `FK_Enrollments_People_StudentID` 
  - 參照: `People(ID)`
  - ON DELETE: CASCADE

**索引**:
- `IX_Enrollments_CourseID` ON `CourseID` (非叢集)
- `IX_Enrollments_StudentID` ON `StudentID` (非叢集)

**建立指令碼**:
```sql
CREATE TABLE [dbo].[Enrollments] (
    [EnrollmentID] INT IDENTITY(1,1) NOT NULL,
    [CourseID] INT NOT NULL,
    [StudentID] INT NOT NULL,
    [Grade] INT NULL,
    CONSTRAINT [PK_Enrollments] PRIMARY KEY CLUSTERED ([EnrollmentID]),
    CONSTRAINT [FK_Enrollments_Courses_CourseID] 
        FOREIGN KEY ([CourseID]) REFERENCES [Courses]([CourseID]) ON DELETE CASCADE,
    CONSTRAINT [FK_Enrollments_People_StudentID] 
        FOREIGN KEY ([StudentID]) REFERENCES [People]([ID]) ON DELETE CASCADE
)

CREATE NONCLUSTERED INDEX [IX_Enrollments_CourseID] 
    ON [Enrollments]([CourseID])
CREATE NONCLUSTERED INDEX [IX_Enrollments_StudentID] 
    ON [Enrollments]([StudentID])
```

### 2.5 CourseAssignments (課程分配表)

**說明**: 講師課程分配多對多關聯表

**資料表名稱**: `CourseAssignments`

| 欄位名稱 | 資料類型 | 長度 | 允許 NULL | 預設值 | 說明 |
|---------|----------|------|-----------|--------|------|
| InstructorID | int | - | 否 | - | 講師 ID (複合主鍵, 外鍵) |
| CourseID | int | - | 否 | - | 課程 ID (複合主鍵, 外鍵) |

**主鍵**: `PK_CourseAssignments` ON `(InstructorID, CourseID)`

**外鍵**:
- `FK_CourseAssignments_People_InstructorID` 
  - 參照: `People(ID)`
  - ON DELETE: CASCADE
- `FK_CourseAssignments_Courses_CourseID` 
  - 參照: `Courses(CourseID)`
  - ON DELETE: CASCADE

**索引**:
- `IX_CourseAssignments_CourseID` ON `CourseID` (非叢集)

**建立指令碼**:
```sql
CREATE TABLE [dbo].[CourseAssignments] (
    [InstructorID] INT NOT NULL,
    [CourseID] INT NOT NULL,
    CONSTRAINT [PK_CourseAssignments] PRIMARY KEY CLUSTERED ([InstructorID], [CourseID]),
    CONSTRAINT [FK_CourseAssignments_People_InstructorID] 
        FOREIGN KEY ([InstructorID]) REFERENCES [People]([ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_CourseAssignments_Courses_CourseID] 
        FOREIGN KEY ([CourseID]) REFERENCES [Courses]([CourseID]) ON DELETE CASCADE
)

CREATE NONCLUSTERED INDEX [IX_CourseAssignments_CourseID] 
    ON [CourseAssignments]([CourseID])
```

### 2.6 OfficeAssignments (辦公室分配表)

**說明**: 講師辦公室分配一對一關聯表

**資料表名稱**: `OfficeAssignments`

| 欄位名稱 | 資料類型 | 長度 | 允許 NULL | 預設值 | 說明 |
|---------|----------|------|-----------|--------|------|
| InstructorID | int | - | 否 | - | 講師 ID (主鍵, 外鍵) |
| Location | nvarchar | 50 | 否 | - | 辦公室位置 |

**主鍵**: `PK_OfficeAssignments` ON `InstructorID`

**外鍵**:
- `FK_OfficeAssignments_People_InstructorID` 
  - 參照: `People(ID)`
  - ON DELETE: CASCADE

**建立指令碼**:
```sql
CREATE TABLE [dbo].[OfficeAssignments] (
    [InstructorID] INT NOT NULL,
    [Location] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_OfficeAssignments] PRIMARY KEY CLUSTERED ([InstructorID]),
    CONSTRAINT [FK_OfficeAssignments_People_InstructorID] 
        FOREIGN KEY ([InstructorID]) REFERENCES [People]([ID]) ON DELETE CASCADE
)
```

### 2.7 Notifications (通知表)

**說明**: 存儲系統通知記錄

**資料表名稱**: `Notifications`

| 欄位名稱 | 資料類型 | 長度 | 允許 NULL | 預設值 | 說明 |
|---------|----------|------|-----------|--------|------|
| Id | int | - | 否 | IDENTITY(1,1) | 主鍵 |
| EntityType | nvarchar | 50 | 否 | - | 實體類型 |
| EntityId | int | - | 否 | - | 實體 ID |
| Operation | nvarchar | 20 | 否 | - | 操作類型 |
| Message | nvarchar | 500 | 否 | - | 通知訊息 |
| CreatedAt | datetime2 | - | 否 | GETDATE() | 創建時間 |

**主鍵**: `PK_Notifications` ON `Id`

**索引**:
- `IX_Notifications_CreatedAt` ON `CreatedAt` (非叢集, 降序)
- `IX_Notifications_EntityType` ON `EntityType` (非叢集)

**建立指令碼**:
```sql
CREATE TABLE [dbo].[Notifications] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [EntityType] NVARCHAR(50) NOT NULL,
    [EntityId] INT NOT NULL,
    [Operation] NVARCHAR(20) NOT NULL,
    [Message] NVARCHAR(500) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED ([Id])
)

CREATE NONCLUSTERED INDEX [IX_Notifications_CreatedAt] 
    ON [Notifications]([CreatedAt] DESC)
CREATE NONCLUSTERED INDEX [IX_Notifications_EntityType] 
    ON [Notifications]([EntityType])
```

### 2.8 ToDos (待辦事項表)

**說明**: 存儲待辦事項

**資料表名稱**: `ToDos`

| 欄位名稱 | 資料類型 | 長度 | 允許 NULL | 預設值 | 說明 |
|---------|----------|------|-----------|--------|------|
| Id | int | - | 否 | IDENTITY(1,1) | 主鍵 |
| Title | nvarchar | 255 | 否 | - | 標題 |
| Description | nvarchar | MAX | 是 | - | 描述 |
| IsCompleted | bit | - | 否 | 0 | 完成狀態 |
| CreatedDate | datetime2 | - | 否 | GETDATE() | 創建日期 |
| CompletedDate | datetime2 | - | 是 | - | 完成日期 |

**主鍵**: `PK_ToDos` ON `Id`

**索引**:
- `IX_ToDos_IsCompleted_CreatedDate` ON `(IsCompleted, CreatedDate DESC)` (非叢集, 複合)

**建立指令碼**:
```sql
CREATE TABLE [dbo].[ToDos] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Title] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [IsCompleted] BIT NOT NULL DEFAULT 0,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [CompletedDate] DATETIME2 NULL,
    CONSTRAINT [PK_ToDos] PRIMARY KEY CLUSTERED ([Id])
)

CREATE NONCLUSTERED INDEX [IX_ToDos_IsCompleted_CreatedDate] 
    ON [ToDos]([IsCompleted], [CreatedDate] DESC)
```

---

## 3. 存儲程序

### 3.1 sp_GetAllToDos

**用途**: 取得所有待辦事項

**參數**: 無

**返回**: ToDos 結果集

**定義**:
```sql
CREATE PROCEDURE [dbo].[sp_GetAllToDos]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [Title],
        [Description],
        [IsCompleted],
        [CreatedDate],
        [CompletedDate]
    FROM [dbo].[ToDos]
    ORDER BY [IsCompleted] ASC, [CreatedDate] DESC
END
```

### 3.2 sp_GetToDoById

**用途**: 根據 ID 取得特定待辦事項

**參數**:
- `@Id INT` - 待辦事項 ID

**返回**: 單一 ToDo 記錄

**定義**:
```sql
CREATE PROCEDURE [dbo].[sp_GetToDoById]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [Title],
        [Description],
        [IsCompleted],
        [CreatedDate],
        [CompletedDate]
    FROM [dbo].[ToDos]
    WHERE [Id] = @Id
END
```

### 3.3 sp_CreateToDo

**用途**: 建立新的待辦事項

**參數**:
- `@Title NVARCHAR(255)` - 標題（必填）
- `@Description NVARCHAR(MAX)` - 描述（選填）
- `@IsCompleted BIT` - 完成狀態（預設 0）

**返回**: 新建立的 ID

**定義**:
```sql
CREATE PROCEDURE [dbo].[sp_CreateToDo]
    @Title NVARCHAR(255),
    @Description NVARCHAR(MAX) = NULL,
    @IsCompleted BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [dbo].[ToDos] 
        ([Title], [Description], [IsCompleted], [CreatedDate])
    VALUES 
        (@Title, @Description, @IsCompleted, GETDATE())
    
    SELECT SCOPE_IDENTITY() AS [Id]
END
```

### 3.4 sp_UpdateToDo

**用途**: 更新待辦事項

**參數**:
- `@Id INT` - 待辦事項 ID
- `@Title NVARCHAR(255)` - 標題
- `@Description NVARCHAR(MAX)` - 描述
- `@IsCompleted BIT` - 完成狀態

**返回**: 無

**定義**:
```sql
CREATE PROCEDURE [dbo].[sp_UpdateToDo]
    @Id INT,
    @Title NVARCHAR(255),
    @Description NVARCHAR(MAX) = NULL,
    @IsCompleted BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[ToDos]
    SET 
        [Title] = @Title,
        [Description] = @Description,
        [IsCompleted] = @IsCompleted,
        [CompletedDate] = CASE 
            WHEN @IsCompleted = 1 AND [IsCompleted] = 0 THEN GETDATE()
            WHEN @IsCompleted = 0 THEN NULL
            ELSE [CompletedDate]
        END
    WHERE [Id] = @Id
END
```

**邏輯說明**:
- 從未完成變為完成: 設定 CompletedDate 為當前時間
- 從完成變為未完成: 清除 CompletedDate
- 狀態未變: 保持原 CompletedDate

### 3.5 sp_DeleteToDo

**用途**: 刪除待辦事項

**參數**:
- `@Id INT` - 待辦事項 ID

**返回**: 無

**定義**:
```sql
CREATE PROCEDURE [dbo].[sp_DeleteToDo]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM [dbo].[ToDos]
    WHERE [Id] = @Id
END
```

---

## 4. 視圖 (Views)

### 4.1 vw_EnrollmentStatistics

**用途**: 提供註冊統計視圖

**定義**:
```sql
CREATE VIEW [dbo].[vw_EnrollmentStatistics]
AS
SELECT 
    CAST([EnrollmentDate] AS DATE) AS [EnrollmentDate],
    COUNT(*) AS [StudentCount]
FROM [dbo].[People]
WHERE [Discriminator] = 'Student'
    AND [EnrollmentDate] IS NOT NULL
GROUP BY CAST([EnrollmentDate] AS DATE)
```

---

## 5. 索引策略

### 5.1 叢集索引

**所有主鍵**: 自動建立叢集索引

**優點**:
- 資料實體排序
- 範圍查詢效能佳

### 5.2 非叢集索引

**搜尋欄位**:
- `People.LastName`, `People.FirstMidName` - 學生/講師搜尋
- `Courses.Title` - 課程搜尋
- `Departments.Name` - 部門搜尋

**外鍵欄位**:
- 所有外鍵都建立索引以加速 JOIN 查詢

**複合索引**:
- `ToDos(IsCompleted, CreatedDate)` - 優化列表排序查詢

### 5.3 索引維護

**定期維護任務**:
- 索引重建（碎片率 > 30%）
- 索引重新組織（碎片率 10-30%）
- 更新統計資訊

**維護腳本**:
```sql
-- 檢查索引碎片
SELECT 
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 10
ORDER BY ips.avg_fragmentation_in_percent DESC

-- 重建索引
ALTER INDEX ALL ON [TableName] REBUILD

-- 更新統計
UPDATE STATISTICS [TableName]
```

---

## 6. 資料庫初始化

### 6.1 DbInitializer 類別

**位置**: `Data/DbInitializer.cs`

**功能**:
- 檢查資料庫是否已初始化
- 如果為空，插入種子資料

**主要方法**: `Initialize(SchoolContext context)`

### 6.2 初始化流程

```csharp
public static void Initialize(SchoolContext context)
{
    context.Database.EnsureCreated();

    // 檢查是否已有資料
    if (context.Students.Any())
    {
        return; // 資料庫已初始化
    }

    // 插入種子資料
    var students = new Student[] { /* ... */ };
    context.Students.AddRange(students);
    context.SaveChanges();

    var instructors = new Instructor[] { /* ... */ };
    context.Instructors.AddRange(instructors);
    context.SaveChanges();

    var departments = new Department[] { /* ... */ };
    context.Departments.AddRange(departments);
    context.SaveChanges();

    var courses = new Course[] { /* ... */ };
    context.Courses.AddRange(courses);
    context.SaveChanges();

    var enrollments = new Enrollment[] { /* ... */ };
    context.Enrollments.AddRange(enrollments);
    context.SaveChanges();

    var courseAssignments = new CourseAssignment[] { /* ... */ };
    context.CourseAssignments.AddRange(courseAssignments);
    context.SaveChanges();
}
```

### 6.3 種子資料

#### 學生資料 (8 筆)

- Carson Alexander
- Meredith Alonso
- Arturo Anand
- Gytis Barzdukas
- Yan Li
- Peggy Justice
- Laura Norman
- Nino Olivetto

#### 講師資料 (5 筆)

- Kim Abercrombie
- Fadi Fakhouri
- Roger Harui
- Candace Kapoor
- Roger Zheng

#### 部門資料 (4 筆)

- English
- Mathematics
- Engineering
- Economics

#### 課程資料 (10 筆)

- 1050: Chemistry
- 4022: Microeconomics
- 4041: Macroeconomics
- 1045: Calculus
- 3141: Trigonometry
- 2021: Composition
- 2042: Literature
- 4061: Poetry
- 2115: Database Systems
- 2345: Web Development

### 6.4 觸發時機

**應用程式啟動**: `Global.asax.cs`

```csharp
protected void Application_Start()
{
    AreaRegistration.RegisterAllAreas();
    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
    RouteConfig.RegisterRoutes(RouteTable.Routes);
    BundleConfig.RegisterBundles(BundleTable.Bundles);

    using (var context = new SchoolContext())
    {
        DbInitializer.Initialize(context);
    }
}
```

---

## 7. 備份與還原

### 7.1 備份策略

**完整備份**: 每日

**差異備份**: 每 6 小時

**交易記錄備份**: 每 15 分鐘

**備份腳本**:
```sql
-- 完整備份
BACKUP DATABASE [ContosoUniversityNoAuthEFCore]
TO DISK = 'C:\Backups\ContosoUniversity_Full.bak'
WITH FORMAT, INIT, NAME = 'Full Database Backup'

-- 差異備份
BACKUP DATABASE [ContosoUniversityNoAuthEFCore]
TO DISK = 'C:\Backups\ContosoUniversity_Diff.bak'
WITH DIFFERENTIAL, NAME = 'Differential Database Backup'

-- 交易記錄備份
BACKUP LOG [ContosoUniversityNoAuthEFCore]
TO DISK = 'C:\Backups\ContosoUniversity_Log.trn'
WITH NAME = 'Transaction Log Backup'
```

### 7.2 還原策略

**還原腳本**:
```sql
-- 還原完整備份
RESTORE DATABASE [ContosoUniversityNoAuthEFCore]
FROM DISK = 'C:\Backups\ContosoUniversity_Full.bak'
WITH NORECOVERY

-- 還原差異備份
RESTORE DATABASE [ContosoUniversityNoAuthEFCore]
FROM DISK = 'C:\Backups\ContosoUniversity_Diff.bak'
WITH NORECOVERY

-- 還原交易記錄
RESTORE LOG [ContosoUniversityNoAuthEFCore]
FROM DISK = 'C:\Backups\ContosoUniversity_Log.trn'
WITH RECOVERY
```

---

## 8. 效能優化

### 8.1 查詢優化

**常見查詢優化**:

1. **使用適當的 JOIN 類型**
2. **避免 SELECT ***（明確指定欄位）
3. **使用 WHERE 過濾而非 HAVING**
4. **適當使用索引提示**

### 8.2 連接池配置

**連接字串設定**:
```
Server=(localdb)\mssqllocaldb;
Database=ContosoUniversityNoAuthEFCore;
Trusted_Connection=True;
MultipleActiveResultSets=true;
Min Pool Size=5;
Max Pool Size=100;
```

### 8.3 執行計畫分析

**啟用執行計畫**:
```sql
SET STATISTICS IO ON
SET STATISTICS TIME ON

-- 執行查詢

SET STATISTICS IO OFF
SET STATISTICS TIME OFF
```

---

## 9. 安全性

### 9.1 存取控制

**資料庫角色**:
- `db_owner` - 管理員
- `db_datareader` - 唯讀使用者
- `db_datawriter` - 讀寫使用者

**應用程式使用者**: 最小權限原則

### 9.2 資料加密

**透明資料加密 (TDE)**: 
- 生產環境建議啟用
- 保護靜態資料

**連接加密**:
- 使用 SSL/TLS 加密連接

### 9.3 稽核

**啟用稽核**:
```sql
CREATE SERVER AUDIT [ContosoUniversity_Audit]
TO FILE (FILEPATH = 'C:\Audits\', MAXSIZE = 100 MB)

CREATE DATABASE AUDIT SPECIFICATION [ContosoUniversity_DB_Audit]
FOR SERVER AUDIT [ContosoUniversity_Audit]
ADD (DELETE, INSERT, UPDATE ON DATABASE::[ContosoUniversityNoAuthEFCore] BY [public])
```

---

## 10. 資料庫遷移

### 10.1 Entity Framework Migrations

**新增遷移**:
```bash
Add-Migration InitialCreate
```

**更新資料庫**:
```bash
Update-Database
```

**回滾遷移**:
```bash
Update-Database -TargetMigration:PreviousMigrationName
```

### 10.2 遷移腳本

**生成 SQL 腳本**:
```bash
Script-Migration
```

---

## 11. 監控與維護

### 11.1 效能監控

**關鍵指標**:
- 查詢執行時間
- 連接數
- 鎖定和死鎖
- 索引使用率
- 資料庫大小增長

### 11.2 維護計畫

**定期任務**:
- 資料庫完整性檢查 (DBCC CHECKDB)
- 索引維護
- 統計更新
- 備份驗證

---

## 參考文件

- [資料模型規格](./02-data-model-specification.md)
- [系統架構概覽](./01-system-architecture-overview.md)
- [技術架構規格](./05-technical-architecture-specification.md)

## 外部參考

- [SQL Server 文件](https://docs.microsoft.com/sql/sql-server/)
- [Entity Framework Core 文件](https://docs.microsoft.com/ef/core/)
- [SQL Server 效能最佳實務](https://docs.microsoft.com/sql/relational-databases/performance/)
