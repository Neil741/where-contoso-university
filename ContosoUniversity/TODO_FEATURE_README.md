# ToDo List Feature - Implementation Guide

## Overview
This document describes the ToDo List feature that has been added to Contoso University. The feature uses **stored procedures** for all database operations (CRUD).

## Architecture

### Model
- **Location**: `Models/ToDo.cs`
- **Properties**:
  - `ID` (int): Primary key
  - `Title` (string, required, max 200 chars): Title of the todo item
  - `Description` (string, max 1000 chars): Optional description
  - `IsCompleted` (bool, required): Completion status
  - `CreatedDate` (DateTime, required): When the todo was created
  - `CompletedDate` (DateTime?, optional): When the todo was completed

### Database Schema
The ToDo table is created automatically when the database is initialized. The table structure:
```sql
CREATE TABLE [dbo].[ToDo] (
    [ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(1000) NULL,
    [IsCompleted] BIT NOT NULL DEFAULT 0,
    [CreatedDate] DATETIME2 NOT NULL,
    [CompletedDate] DATETIME2 NULL
);
```

### Stored Procedures
All CRUD operations use stored procedures:

1. **sp_GetAllToDos**: Retrieves all todos, sorted by completion status (incomplete first) and creation date
2. **sp_GetToDoById**: Retrieves a single todo by ID
3. **sp_CreateToDo**: Creates a new todo and returns the new ID
4. **sp_UpdateToDo**: Updates an existing todo
5. **sp_DeleteToDo**: Deletes a todo by ID

The stored procedures are created automatically during database initialization in `Data/DbInitializer.cs`.

### Controller
- **Location**: `Controllers/ToDosController.cs`
- **Base Class**: `BaseController`
- **Actions**:
  - `Index()`: Lists all todos
  - `Details(id)`: Shows details of a specific todo
  - `Create()`: Shows create form (GET) / Creates new todo (POST)
  - `Edit(id)`: Shows edit form (GET) / Updates todo (POST)
  - `Delete(id)`: Shows delete confirmation (GET) / Deletes todo (POST)

All actions use ADO.NET to call the stored procedures directly via `DbConnection.CreateCommand()`.

### Views
- **Location**: `Views/ToDos/`
- **Files**:
  - `Index.cshtml`: List view with status indicators
  - `Details.cshtml`: Detail view
  - `Create.cshtml`: Create form
  - `Edit.cshtml`: Edit form
  - `Delete.cshtml`: Delete confirmation

### Navigation
The ToDo List is accessible from the main navigation menu between "Departments" and "Notifications".

## Testing Instructions

### Prerequisites
- Visual Studio 2019 or later
- SQL Server LocalDB
- .NET Framework 4.8

### Setup and Run
1. Open the solution in Visual Studio
2. Build the solution (Ctrl+Shift+B)
3. Run the application (F5)
4. Navigate to "ToDo List" in the menu

### Test Cases

#### 1. Create ToDo
1. Click "Create New" button
2. Fill in:
   - Title: "Test Todo Item"
   - Description: "This is a test"
   - IsCompleted: Unchecked
   - Created Date: Today's date
3. Click "Create"
4. Verify: Redirected to Index page with new item displayed

#### 2. View All ToDos
1. Navigate to ToDo List
2. Verify: All todos are listed
3. Verify: Incomplete todos appear before completed ones
4. Verify: Truncated descriptions for long text

#### 3. View Details
1. Click "Details" on any todo
2. Verify: All fields are displayed correctly
3. Verify: Completion status is shown with appropriate label

#### 4. Edit ToDo
1. Click "Edit" on any todo
2. Modify the title or description
3. Check/uncheck "IsCompleted"
4. Click "Save"
5. Verify: Changes are saved and visible in the list

#### 5. Delete ToDo
1. Click "Delete" on any todo
2. Verify: Confirmation page shows correct todo details
3. Click "Delete"
4. Verify: Todo is removed from the list

#### 6. Complete a ToDo
1. Click "Edit" on an incomplete todo
2. Check "IsCompleted"
3. Set "Completed Date" to today
4. Click "Save"
5. Verify: Todo moves to the completed section (with success highlighting)

### Verify Stored Procedures
To verify stored procedures are being used:

1. Open SQL Server Management Studio or SQL Server Object Explorer
2. Connect to `(LocalDb)\MSSQLLocalDB`
3. Navigate to database: `ContosoUniversityNoAuthEFCore`
4. Expand "Programmability" > "Stored Procedures"
5. Verify presence of:
   - `sp_GetAllToDos`
   - `sp_GetToDoById`
   - `sp_CreateToDo`
   - `sp_UpdateToDo`
   - `sp_DeleteToDo`

### Build Verification
To verify the build succeeds:
```
dotnet build -c Release
```

**Note**: This project requires Windows and Visual Studio build tools as it targets .NET Framework 4.8.

## Implementation Notes

### Key Features
1. **Stored Procedures Only**: All database operations use stored procedures, not Entity Framework queries
2. **ADO.NET**: Direct database access using `DbConnection` and `SqlParameter`
3. **Error Handling**: Comprehensive try-catch blocks with trace logging
4. **Validation**: Client and server-side validation for required fields
5. **UI Consistency**: Follows the same pattern as other controllers (Students, Courses, etc.)

### Non-Breaking Changes
- No modifications to existing code (Students, Courses, Instructors, Departments)
- No package updates
- No security changes
- Additive only - new files and minimal updates to shared files

## Files Modified/Added

### New Files
- `Models/ToDo.cs`
- `Controllers/ToDosController.cs`
- `Data/ToDoStoredProcedures.sql`
- `Views/ToDos/Index.cshtml`
- `Views/ToDos/Details.cshtml`
- `Views/ToDos/Create.cshtml`
- `Views/ToDos/Edit.cshtml`
- `Views/ToDos/Delete.cshtml`

### Modified Files
- `Data/SchoolContext.cs` - Added `DbSet<ToDo>` and table mapping
- `Data/DbInitializer.cs` - Added stored procedure creation logic
- `Views/Shared/_Layout.cshtml` - Added navigation menu item
- `ContosoUniversity.csproj` - Added new files to compilation

## SQL Script Location
A standalone SQL script is available at `Data/ToDoStoredProcedures.sql` for manual database setup if needed.
