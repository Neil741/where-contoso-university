-- Create ToDo table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ToDo')
BEGIN
    CREATE TABLE [dbo].[ToDo] (
        [ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [IsCompleted] BIT NOT NULL DEFAULT 0,
        [CreatedDate] DATETIME2 NOT NULL,
        [CompletedDate] DATETIME2 NULL
    );
END
GO

-- Stored Procedure: Get All ToDos
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetAllToDos')
    DROP PROCEDURE sp_GetAllToDos;
GO

CREATE PROCEDURE sp_GetAllToDos
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT ID, Title, Description, IsCompleted, CreatedDate, CompletedDate
    FROM ToDo
    ORDER BY 
        CASE WHEN IsCompleted = 0 THEN 0 ELSE 1 END,
        CreatedDate DESC;
END
GO

-- Stored Procedure: Get ToDo By ID
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetToDoById')
    DROP PROCEDURE sp_GetToDoById;
GO

CREATE PROCEDURE sp_GetToDoById
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT ID, Title, Description, IsCompleted, CreatedDate, CompletedDate
    FROM ToDo
    WHERE ID = @ID;
END
GO

-- Stored Procedure: Create ToDo
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CreateToDo')
    DROP PROCEDURE sp_CreateToDo;
GO

CREATE PROCEDURE sp_CreateToDo
    @Title NVARCHAR(200),
    @Description NVARCHAR(1000),
    @IsCompleted BIT,
    @CreatedDate DATETIME2,
    @CompletedDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO ToDo (Title, Description, IsCompleted, CreatedDate, CompletedDate)
    VALUES (@Title, @Description, @IsCompleted, @CreatedDate, @CompletedDate);
    
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ID;
END
GO

-- Stored Procedure: Update ToDo
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_UpdateToDo')
    DROP PROCEDURE sp_UpdateToDo;
GO

CREATE PROCEDURE sp_UpdateToDo
    @ID INT,
    @Title NVARCHAR(200),
    @Description NVARCHAR(1000),
    @IsCompleted BIT,
    @CreatedDate DATETIME2,
    @CompletedDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE ToDo
    SET Title = @Title,
        Description = @Description,
        IsCompleted = @IsCompleted,
        CreatedDate = @CreatedDate,
        CompletedDate = @CompletedDate
    WHERE ID = @ID;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- Stored Procedure: Delete ToDo
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_DeleteToDo')
    DROP PROCEDURE sp_DeleteToDo;
GO

CREATE PROCEDURE sp_DeleteToDo
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM ToDo
    WHERE ID = @ID;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
