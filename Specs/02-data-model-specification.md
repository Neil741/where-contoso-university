# 資料模型規格

## 概述

Contoso University 系統的資料模型基於高等教育機構的核心實體設計，採用物件導向的繼承結構和關聯關係來表示學生、講師、課程、部門等實體之間的複雜關係。

## 實體關係圖 (ERD)

```
┌─────────────┐
│   Person    │ (抽象基類)
│─────────────│
│ ID          │
│ LastName    │
│ FirstMidName│
└──────┬──────┘
       │
       ├───────────────────────┐
       │                       │
┌──────▼──────┐         ┌──────▼──────┐
│   Student   │         │ Instructor  │
│─────────────│         │─────────────│
│ EnrollmentDt│         │ HireDate    │
└──────┬──────┘         └──────┬──────┘
       │                       │
       │                       │ 1
       │ 1                     │
       │                       │
       │ *                     │ *
┌──────▼──────┐         ┌──────▼──────────┐
│ Enrollment  │         │CourseAssignment │
│─────────────│         │─────────────────│
│ EnrollmentID│         │ InstructorID    │
│ CourseID    │         │ CourseID        │
│ StudentID   │         └──────┬──────────┘
│ Grade?      │                │
└──────┬──────┘                │
       │                       │
       │ *                     │ *
       │                       │
       │         ┌─────────────▼──────┐
       └─────────►    Course          │
                 │────────────────────│
                 │ CourseID           │
                 │ Title              │
                 │ Credits            │
                 │ DepartmentID       │
                 │ TeachingMaterial...│
                 └─────────┬──────────┘
                           │ *
                           │
                           │ 1
                  ┌────────▼────────┐
                  │   Department    │
                  │─────────────────│
                  │ DepartmentID    │
                  │ Name            │
                  │ Budget          │
                  │ StartDate       │
                  │ InstructorID?   │───┐
                  │ (Administrator) │   │
                  └─────────────────┘   │
                           ▲            │
                           └────────────┘

┌──────────────────┐         ┌──────────────────┐
│ OfficeAssignment │         │   Notification   │
│──────────────────│         │──────────────────│
│ InstructorID (PK)│         │ Id               │
│ Location         │         │ EntityType       │
└──────────────────┘         │ EntityId         │
                             │ Operation        │
                             │ Message          │
                             │ CreatedAt        │
                             └──────────────────┘

┌──────────────────┐
│      ToDo        │
│──────────────────│
│ Id               │
│ Title            │
│ Description      │
│ IsCompleted      │
│ CreatedDate      │
│ CompletedDate?   │
└──────────────────┘
```

## 核心實體定義

### 1. Person (抽象基類)

**目的**: 為 Student 和 Instructor 提供共同屬性的抽象基類

**屬性**:

| 屬性名稱 | 類型 | 必填 | 說明 |
|----------|------|------|------|
| ID | int | 是 | 主鍵，自動遞增 |
| LastName | string | 是 | 姓氏，最大長度 50 |
| FirstMidName | string | 是 | 名字（可包含中間名），最大長度 50，顯示標籤為 "First Name" |

**設計考量**:
- 採用 TPH (Table Per Hierarchy) 繼承策略
- 使用 Discriminator 欄位區分 Student 和 Instructor

### 2. Student (學生)

**繼承**: Person

**目的**: 表示在大學註冊的學生

**屬性**:

| 屬性名稱 | 類型 | 必填 | 說明 |
|----------|------|------|------|
| EnrollmentDate | DateTime | 是 | 學生註冊日期，顯示標籤為 "Enrollment Date" |
| Enrollments | ICollection\<Enrollment\> | - | 學生的課程註冊集合（導覽屬性） |

**業務規則**:
- 一個學生可以註冊多門課程
- EnrollmentDate 用於統計分析

**資料驗證**:
- EnrollmentDate: 必須是有效日期

### 3. Instructor (講師)

**繼承**: Person

**目的**: 表示大學的講師或教授

**屬性**:

| 屬性名稱 | 類型 | 必填 | 說明 |
|----------|------|------|------|
| HireDate | DateTime | 是 | 講師聘用日期，顯示標籤為 "Hire Date" |
| CourseAssignments | ICollection\<CourseAssignment\> | - | 講師授課的課程集合（導覽屬性） |
| OfficeAssignment | OfficeAssignment | - | 講師的辦公室分配（一對一關係） |

**業務規則**:
- 一個講師可以教授多門課程
- 一個講師最多有一個辦公室
- 講師可以擔任部門主任（Administrator）

**資料驗證**:
- HireDate: 必須是有效日期

### 4. Course (課程)

**目的**: 表示大學提供的課程

**屬性**:

| 屬性名稱 | 類型 | 必填 | 說明 |
|----------|------|------|------|
| CourseID | int | 是 | 主鍵，不自動遞增（手動指定） |
| Title | string | 是 | 課程名稱，最大長度 50 |
| Credits | int | 是 | 學分數，範圍 0-5 |
| DepartmentID | int | 是 | 所屬部門 ID（外鍵） |
| TeachingMaterialImagePath | string | - | 教材圖片路徑，最大長度 255 |
| Department | Department | - | 所屬部門（導覽屬性） |
| Enrollments | ICollection\<Enrollment\> | - | 課程註冊集合（導覽屬性） |
| CourseAssignments | ICollection\<CourseAssignment\> | - | 授課講師集合（導覽屬性） |

**業務規則**:
- 課程必須屬於一個部門
- 課程可以被多個學生註冊
- 課程可以由多個講師授課
- 課程可以有教材圖片

**資料驗證**:
- Title: 必填，最大長度 50
- Credits: 0-5 之間
- TeachingMaterialImagePath: 最大長度 255
- 支援的教材格式: JPG, JPEG, PNG, GIF, BMP
- 教材檔案大小限制: 5MB

### 5. Department (部門)

**目的**: 表示大學的學術部門

**屬性**:

| 屬性名稱 | 類型 | 必填 | 說明 |
|----------|------|------|------|
| DepartmentID | int | 是 | 主鍵，自動遞增 |
| Name | string | 是 | 部門名稱，最大長度 50 |
| Budget | decimal | 是 | 部門預算，貨幣格式 |
| StartDate | DateTime | 是 | 部門成立日期 |
| InstructorID | int | - | 部門主任 ID（外鍵，可為 null） |
| RowVersion | byte[] | - | 並發控制用的時間戳記 |
| Administrator | Instructor | - | 部門主任（導覽屬性） |
| Courses | ICollection\<Course\> | - | 部門的課程集合（導覽屬性） |

**業務規則**:
- 部門可以有一個主任（講師）
- 部門可以沒有主任（InstructorID 可為 null）
- 部門可以有多門課程
- 使用樂觀並發控制（RowVersion）

**資料驗證**:
- Name: 必填，最大長度 50
- Budget: 必填，貨幣格式
- StartDate: 必填，有效日期

### 6. Enrollment (課程註冊)

**目的**: 表示學生註冊課程的關聯

**屬性**:

| 屬性名稱 | 類型 | 必填 | 說明 |
|----------|------|------|------|
| EnrollmentID | int | 是 | 主鍵，自動遞增 |
| CourseID | int | 是 | 課程 ID（外鍵） |
| StudentID | int | 是 | 學生 ID（外鍵） |
| Grade | Grade? | - | 成績（枚舉類型，可為 null） |
| Course | Course | - | 課程（導覽屬性） |
| Student | Student | - | 學生（導覽屬性） |

**成績枚舉 (Grade)**:
- A
- B
- C
- D
- F

**業務規則**:
- 一個學生可以註冊多門課程
- 一門課程可以被多個學生註冊
- 成績可以為空（尚未評分）

**資料驗證**:
- CourseID: 必填，必須是有效的課程
- StudentID: 必填，必須是有效的學生
- Grade: 可為 null 或有效的枚舉值

### 7. CourseAssignment (課程分配)

**目的**: 表示講師授課的多對多關聯

**屬性**:

| 屬性名稱 | 類型 | 必填 | 說明 |
|----------|------|------|------|
| InstructorID | int | 是 | 講師 ID（複合主鍵之一） |
| CourseID | int | 是 | 課程 ID（複合主鍵之一） |
| Instructor | Instructor | - | 講師（導覽屬性） |
| Course | Course | - | 課程（導覽屬性） |

**業務規則**:
- 一個講師可以教授多門課程
- 一門課程可以由多個講師授課
- 使用複合主鍵 (InstructorID, CourseID)

**資料驗證**:
- InstructorID: 必填，必須是有效的講師
- CourseID: 必填，必須是有效的課程

### 8. OfficeAssignment (辦公室分配)

**目的**: 表示講師的辦公室位置

**屬性**:

| 屬性名稱 | 類型 | 必填 | 說明 |
|----------|------|------|------|
| InstructorID | int | 是 | 講師 ID（主鍵、外鍵） |
| Location | string | 是 | 辦公室位置，最大長度 50 |
| Instructor | Instructor | - | 講師（導覽屬性） |

**業務規則**:
- 一個講師最多有一個辦公室
- 辦公室與講師是一對一關係
- InstructorID 同時作為主鍵和外鍵

**資料驗證**:
- Location: 必填，最大長度 50

## 特殊功能實體

### 9. Notification (通知)

**目的**: 記錄系統中的資料變更通知

**屬性**:

| 屬性名稱 | 類型 | 必填 | 說明 |
|----------|------|------|------|
| Id | int | 是 | 主鍵，自動遞增 |
| EntityType | string | 是 | 實體類型（Student, Course, Instructor, Department） |
| EntityId | int | 是 | 實體 ID |
| Operation | string | 是 | 操作類型（CREATE, UPDATE, DELETE） |
| Message | string | 是 | 通知訊息 |
| CreatedAt | DateTime | 是 | 通知創建時間 |

**業務規則**:
- 通知透過 MSMQ 產生
- 僅管理員可見
- 通知在 UI 上顯示 1 分鐘後自動關閉
- 前端每 5 秒輪詢一次新通知

**支援的實體類型**:
- Student (學生)
- Course (課程)
- Instructor (講師)
- Department (部門)

**支援的操作類型**:
- CREATE (新增) - 綠色標示
- UPDATE (更新) - 藍色標示
- DELETE (刪除) - 橙色標示

### 10. ToDo (待辦事項)

**目的**: 管理待辦任務

**屬性**:

| 屬性名稱 | 類型 | 必填 | 說明 |
|----------|------|------|------|
| Id | int | 是 | 主鍵，自動遞增 |
| Title | string | 是 | 待辦事項標題，最大長度 255 |
| Description | string | - | 詳細描述 |
| IsCompleted | bool | 是 | 是否完成，預設 false |
| CreatedDate | DateTime | 是 | 創建日期 |
| CompletedDate | DateTime | - | 完成日期（可為 null） |

**業務規則**:
- 使用存儲程序進行 CRUD 操作
- 完成時自動設定 CompletedDate
- 列表按完成狀態和創建日期排序

**資料驗證**:
- Title: 必填，最大長度 255
- IsCompleted: 必填，預設 false
- CreatedDate: 自動設定為當前時間

## 資料關聯總結

### 一對多關聯 (One-to-Many)

1. **Department → Course**
   - 一個部門有多門課程
   - 外鍵: Course.DepartmentID

2. **Course → Enrollment**
   - 一門課程有多個註冊記錄
   - 外鍵: Enrollment.CourseID

3. **Student → Enrollment**
   - 一個學生有多個課程註冊
   - 外鍵: Enrollment.StudentID

4. **Instructor → Department** (as Administrator)
   - 一個講師可以管理一個部門
   - 外鍵: Department.InstructorID

### 多對多關聯 (Many-to-Many)

1. **Instructor ↔ Course** (透過 CourseAssignment)
   - 一個講師可以教授多門課程
   - 一門課程可以由多個講師授課
   - 連接表: CourseAssignment

2. **Student ↔ Course** (透過 Enrollment)
   - 一個學生可以註冊多門課程
   - 一門課程可以被多個學生註冊
   - 連接表: Enrollment（包含成績資訊）

### 一對一關聯 (One-to-One)

1. **Instructor ↔ OfficeAssignment**
   - 一個講師最多有一個辦公室
   - 主鍵-外鍵: OfficeAssignment.InstructorID

## 資料完整性約束

### 主鍵約束

- 所有實體都有主鍵
- 大多數使用自動遞增整數
- Course.CourseID 手動指定
- CourseAssignment 使用複合主鍵

### 外鍵約束

- 所有外鍵都有參照完整性約束
- 級聯刪除在某些關聯中啟用
- Department.InstructorID 允許 null

### 唯一性約束

- 主鍵自動具有唯一性
- InstructorID 在 OfficeAssignment 中唯一

### 檢查約束

- Course.Credits: 0-5 範圍
- 字串長度限制

## 並發控制

### 樂觀並發控制

- Department 使用 RowVersion (byte[]) 欄位
- Entity Framework 自動處理並發衝突

### 並發策略

- 檢測最後勝出（Last-Write-Wins）預設行為
- 可在更新時檢查 RowVersion 並處理衝突

## 索引建議

### 建議的索引

1. **Student**
   - LastName, FirstMidName (搜尋優化)

2. **Course**
   - DepartmentID (外鍵查詢)
   - Title (搜尋優化)

3. **Enrollment**
   - StudentID (學生課程查詢)
   - CourseID (課程學生查詢)

4. **CourseAssignment**
   - InstructorID (講師課程查詢)
   - CourseID (課程講師查詢)

5. **Notification**
   - CreatedAt (時間排序查詢)
   - EntityType (實體類型過濾)

## 資料遷移策略

### Code-First 遷移

- 使用 Entity Framework Core Code-First 方法
- DbInitializer 用於初始化和種子資料
- 自動創建資料庫結構

### 初始化資料

- DbInitializer.Initialize() 方法
- 檢查資料庫是否已有資料
- 如果為空，則插入測試資料

## 參考文件

- [系統架構概覽](./01-system-architecture-overview.md)
- [資料庫規格](./06-database-specification.md)
- [功能模組規格](./03-functional-modules-specification.md)
