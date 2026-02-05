# DI 與服務設計規格

## 概述

本文件定義 Contoso University .NET 8 架構中的依賴注入 (Dependency Injection) 設計模式、服務層架構、Repository Pattern 實作，以及服務生命週期管理策略。

---

## 1. 依賴注入架構設計

### 1.1 DI 容器選擇

**選定容器**: ASP.NET Core 內建 DI 容器 (`Microsoft.Extensions.DependencyInjection`)

**選擇理由**:
- ✅ 框架原生支援，無需額外套件
- ✅ 效能經最佳化
- ✅ 與 ASP.NET Core 生命週期完美整合
- ✅ 支援建構子注入、屬性注入、方法注入
- ✅ 足夠應對大多數場景

**非選擇 Autofac/Ninject 的理由**:
- 內建容器功能已足夠（支援泛型、開放泛型、裝飾器模式）
- 避免引入額外依賴與學習曲線
- 效能更優（基準測試顯示內建容器最快）

### 1.2 服務註冊原則

#### 1.2.1 服務生命週期選擇指南

| 生命週期 | 何時使用 | 範例服務 | 注意事項 |
|---------|---------|---------|---------|
| **Transient** | 無狀態、輕量級服務 | DTO Mapper, Validator, Helper | 每次注入建立新實例，避免快取狀態 |
| **Scoped** | 需維持請求內一致性 | DbContext, UnitOfWork, Repository | **最常用**，一個 HTTP 請求一個實例 |
| **Singleton** | 無狀態、可安全共享 | Configuration, Logger, HttpClient Factory | 需執行緒安全，避免儲存可變狀態 |

#### 1.2.2 生命週期選擇決策樹

```
需要在不同請求間共享狀態？
  │
  ├─ Yes → 是否執行緒安全？
  │         │
  │         ├─ Yes → Singleton
  │         └─ No → 重新設計為無狀態或使用分散式快取
  │
  └─ No → 需要在單一請求內共享狀態？
            │
            ├─ Yes → Scoped
            └─ No → Transient
```

#### 1.2.3 常見陷阱與避免方法

**陷阱 1: Singleton 依賴 Scoped 服務**

```csharp
// ❌ 錯誤：Singleton 注入 Scoped DbContext
public class CachingService  // Singleton
{
    private readonly SchoolContext _context;  // Scoped - 錯誤！
    
    public CachingService(SchoolContext context)
    {
        _context = context;  // DbContext 會在第一次請求後被釋放
    }
}

// ✅ 正確：使用 IServiceProvider 或工廠模式
public class CachingService
{
    private readonly IServiceProvider _serviceProvider;
    
    public CachingService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<Student> GetStudentAsync(int id)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        return await context.Students.FindAsync(id);
    }
}
```

**陷阱 2: Scoped 服務儲存狀態到 Singleton**

```csharp
// ❌ 錯誤：將請求特定資料存到 Singleton
public class SingletonCache  // Singleton
{
    public string CurrentUserId { get; set; }  // 危險！會被所有請求共享
}

// ✅ 正確：使用 IHttpContextAccessor 或 AsyncLocal
public class UserContextService  // Scoped
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public string CurrentUserId => 
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
```

### 1.3 服務註冊方式

#### 1.3.1 集中註冊 vs. 模組化註冊

**推薦方式**: 模組化註冊（使用擴充方法）

```csharp
// Program.cs - 保持簡潔
var builder = WebApplication.CreateBuilder(args);

// 模組化註冊
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddAzureServices(builder.Configuration);

var app = builder.Build();
```

```csharp
// Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // 資料庫
        services.AddDbContext<SchoolContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("ContosoUniversity.Api")
            ));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Application Services
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IInstructorService, InstructorService>();

        return services;
    }

    public static IServiceCollection AddAzureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Azure Blob Storage
        services.AddSingleton(sp =>
        {
            var connectionString = configuration["AzureBlobStorage:ConnectionString"];
            return new BlobServiceClient(connectionString);
        });
        services.AddScoped<IBlobStorageService, BlobStorageService>();

        // Azure Service Bus
        services.AddSingleton(sp =>
        {
            var connectionString = configuration["AzureServiceBus:ConnectionString"];
            return new ServiceBusClient(connectionString);
        });
        services.AddScoped<INotificationQueueService, NotificationQueueService>();
        
        // SignalR
        services.AddSignalR();

        return services;
    }
}
```

#### 1.3.2 介面與實作命名規範

| 模式 | 介面命名 | 實作命名 | 範例 |
|-----|---------|---------|------|
| **預設** | `I{Name}Service` | `{Name}Service` | `IStudentService` → `StudentService` |
| **多實作** | `I{Name}Service` | `{Variant}{Name}Service` | `IBlobStorageService` → `AzureBlobStorageService`, `LocalBlobStorageService` |
| **Repository** | `I{Entity}Repository` | `{Entity}Repository` | `IStudentRepository` → `StudentRepository` |
| **泛型** | `IRepository<T>` | `Repository<T>` | 基礎泛型介面 |

---

## 2. 服務層架構設計

### 2.1 分層架構定義

```
┌─────────────────────────────────────────────────────────┐
│                   Presentation Layer                     │
│              (Controllers, SignalR Hubs)                 │
│  - API 端點定義                                           │
│  - 請求驗證與回應格式化                                    │
│  - 授權檢查                                               │
└───────────────────────┬─────────────────────────────────┘
                        │ 呼叫
┌───────────────────────▼─────────────────────────────────┐
│                   Application Layer                      │
│                (Application Services)                    │
│  - 業務流程編排                                           │
│  - DTO 轉換 (Mapper)                                     │
│  - 業務驗證                                               │
│  - 交易管理 (Unit of Work)                               │
└───────────────────────┬─────────────────────────────────┘
                        │ 委派
┌───────────────────────▼─────────────────────────────────┐
│                    Domain Layer                          │
│                (Domain Entities, Logic)                  │
│  - 領域實體 (Student, Course, etc.)                      │
│  - 業務規則與驗證                                         │
│  - 領域事件                                               │
└───────────────────────┬─────────────────────────────────┘
                        │ 持久化
┌───────────────────────▼─────────────────────────────────┐
│                 Infrastructure Layer                     │
│            (Repositories, EF Core, Azure SDK)            │
│  - 資料訪問實作 (Repository)                              │
│  - EF Core DbContext                                     │
│  - 外部服務整合 (Azure Blob, Service Bus)                │
└─────────────────────────────────────────────────────────┘
```

### 2.2 服務類型與職責

#### 2.2.1 Application Services（應用服務）

**職責**:
- 編排多個 Repository 或 Domain Service 完成業務流程
- DTO 與 Entity 轉換
- 事務邊界管理
- 呼叫外部服務（通知、檔案上傳）

**範例**: `StudentService`

```csharp
public interface IStudentService
{
    Task<PagedResult<StudentDto>> GetStudentsAsync(StudentFilterDto filter);
    Task<StudentDto?> GetStudentByIdAsync(int id);
    Task<StudentDto> CreateStudentAsync(CreateStudentDto dto);
    Task<StudentDto> UpdateStudentAsync(int id, UpdateStudentDto dto);
    Task<bool> DeleteStudentAsync(int id);
    Task<bool> EnrollCourseAsync(int studentId, int courseId);
}

public class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationQueueService _notificationQueue;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationQueueService notificationQueue,
        ILogger<StudentService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationQueue = notificationQueue;
        _logger = logger;
    }

    public async Task<StudentDto> CreateStudentAsync(CreateStudentDto dto)
    {
        // 1. DTO → Entity
        var student = _mapper.Map<Student>(dto);

        // 2. 業務驗證（如需要）
        // ValidateStudent(student);

        // 3. 持久化
        await _unitOfWork.Students.AddAsync(student);
        await _unitOfWork.CommitAsync();

        // 4. 發送通知（非同步，不影響主流程）
        await _notificationQueue.SendNotificationAsync(new NotificationDto
        {
            EntityType = "Student",
            EntityId = student.ID.ToString(),
            EntityDisplayName = student.FullName,
            Operation = "CREATE",
            CreatedBy = "System"  // 實際應從 HttpContext 取得
        });

        // 5. Entity → DTO
        return _mapper.Map<StudentDto>(student);
    }

    public async Task<PagedResult<StudentDto>> GetStudentsAsync(StudentFilterDto filter)
    {
        // 1. 取得分頁資料
        var query = _unitOfWork.Students.Query();

        // 2. 套用篩選
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(s =>
                s.LastName.Contains(filter.SearchTerm) ||
                s.FirstMidName.Contains(filter.SearchTerm));
        }

        // 3. 排序
        query = filter.SortBy?.ToLower() switch
        {
            "lastname" => filter.SortDescending
                ? query.OrderByDescending(s => s.LastName)
                : query.OrderBy(s => s.LastName),
            "enrollmentdate" => filter.SortDescending
                ? query.OrderByDescending(s => s.EnrollmentDate)
                : query.OrderBy(s => s.EnrollmentDate),
            _ => query.OrderBy(s => s.LastName)
        };

        // 4. 分頁
        var totalCount = await query.CountAsync();
        var students = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // 5. Entity → DTO
        var studentDtos = _mapper.Map<List<StudentDto>>(students);

        return new PagedResult<StudentDto>
        {
            Items = studentDtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }
}
```

#### 2.2.2 Domain Services（領域服務）

**職責**:
- 封裝不屬於單一實體的業務邏輯
- 跨實體的業務規則驗證
- 複雜的領域計算

**範例**: `EnrollmentDomainService`

```csharp
public interface IEnrollmentDomainService
{
    bool CanEnroll(Student student, Course course);
    decimal CalculateGPA(IEnumerable<Enrollment> enrollments);
    bool HasPrerequisites(Student student, Course course);
}

public class EnrollmentDomainService : IEnrollmentDomainService
{
    public bool CanEnroll(Student student, Course course)
    {
        // 業務規則 1: 學生不能重複註冊同一課程
        if (student.Enrollments.Any(e => e.CourseID == course.CourseID))
            return false;

        // 業務規則 2: 課程有名額限制（假設）
        // if (course.EnrollmentCount >= course.MaxCapacity)
        //     return false;

        // 業務規則 3: 檢查先修課程
        return HasPrerequisites(student, course);
    }

    public decimal CalculateGPA(IEnumerable<Enrollment> enrollments)
    {
        var gradePoints = enrollments
            .Where(e => e.Grade.HasValue)
            .Select(e => e.Grade.Value switch
            {
                Grade.A => 4.0m,
                Grade.B => 3.0m,
                Grade.C => 2.0m,
                Grade.D => 1.0m,
                Grade.F => 0.0m,
                _ => 0.0m
            });

        return gradePoints.Any() ? gradePoints.Average() : 0.0m;
    }

    public bool HasPrerequisites(Student student, Course course)
    {
        // 實作先修課程檢查邏輯
        // 簡化版本：假設無先修課程限制
        return true;
    }
}
```

#### 2.2.3 Infrastructure Services（基礎設施服務）

**職責**:
- 與外部系統整合（Azure 服務、第三方 API）
- 檔案 I/O、網路通訊
- 實作技術細節（非業務邏輯）

**範例**: `BlobStorageService`

```csharp
public interface IBlobStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream> DownloadFileAsync(string fileName);
    Task<bool> DeleteFileAsync(string fileName);
    Task<string> GenerateSasUrlAsync(string fileName, int expiryMinutes = 60);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(
        BlobServiceClient blobServiceClient,
        IConfiguration configuration,
        ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = configuration["AzureBlobStorage:ContainerName"] 
            ?? throw new InvalidOperationException("Container name not configured");
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream, 
        string fileName, 
        string contentType)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);
            
            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            };

            await blobClient.UploadAsync(fileStream, options);

            _logger.LogInformation("File uploaded successfully: {FileName}", fileName);
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> GenerateSasUrlAsync(string fileName, int expiryMinutes = 60)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = fileName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow,
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri.ToString();
    }

    // 其他方法實作...
}
```

---

## 3. Repository Pattern 實作

### 3.1 為什麼使用 Repository Pattern？

**優點**:
- ✅ 抽象化資料訪問邏輯
- ✅ 方便單元測試（可 Mock）
- ✅ 集中查詢邏輯，避免 LINQ 散落各處
- ✅ 支援切換資料來源（SQL → NoSQL）

**缺點**:
- ⚠️ 增加抽象層（但 EF Core 本身已是一層抽象）
- ⚠️ 過度設計風險（簡單 CRUD 直接用 DbContext 即可）

**結論**: 對於 Contoso University，使用 Repository + Unit of Work 模式以提升可測試性與維護性

### 3.2 Generic Repository 設計

#### 3.2.1 基礎泛型 Repository

```csharp
// 介面定義
public interface IRepository<T> where T : class
{
    // 查詢
    IQueryable<T> Query();
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    // 新增
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);

    // 更新
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);

    // 刪除
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);

    // 計數
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
}

// 實作
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly SchoolContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(SchoolContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public IQueryable<T> Query() => _dbSet.AsQueryable();

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
        => await _dbSet.AddRangeAsync(entities);

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void UpdateRange(IEnumerable<T> entities)
        => _dbSet.UpdateRange(entities);

    public void Remove(T entity)
        => _dbSet.Remove(entity);

    public void RemoveRange(IEnumerable<T> entities)
        => _dbSet.RemoveRange(entities);

    public async Task<int> CountAsync()
        => await _dbSet.CountAsync();

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.CountAsync(predicate);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AnyAsync(predicate);
}
```

#### 3.2.2 特定實體 Repository（擴充）

```csharp
// 學生特定查詢
public interface IStudentRepository : IRepository<Student>
{
    Task<Student?> GetStudentWithEnrollmentsAsync(int id);
    Task<IEnumerable<Student>> GetStudentsByEnrollmentDateAsync(DateTime date);
    Task<Dictionary<DateTime, int>> GetEnrollmentStatisticsAsync();
}

public class StudentRepository : Repository<Student>, IStudentRepository
{
    public StudentRepository(SchoolContext context) : base(context) { }

    public async Task<Student?> GetStudentWithEnrollmentsAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.ID == id);
    }

    public async Task<IEnumerable<Student>> GetStudentsByEnrollmentDateAsync(DateTime date)
    {
        return await _dbSet
            .Where(s => s.EnrollmentDate.Date == date.Date)
            .ToListAsync();
    }

    public async Task<Dictionary<DateTime, int>> GetEnrollmentStatisticsAsync()
    {
        return await _dbSet
            .GroupBy(s => s.EnrollmentDate.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);
    }
}
```

### 3.3 Unit of Work Pattern

#### 3.3.1 為什麼需要 Unit of Work？

- 管理多個 Repository 的事務
- 確保一次性提交所有變更
- 避免部分成功、部分失敗的不一致狀態

#### 3.3.2 實作

```csharp
public interface IUnitOfWork : IDisposable
{
    // Repositories
    IStudentRepository Students { get; }
    ICourseRepository Courses { get; }
    IInstructorRepository Instructors { get; }
    IDepartmentRepository Departments { get; }
    IEnrollmentRepository Enrollments { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<ToDo> ToDos { get; }

    // 事務管理
    Task<int> CommitAsync();
    Task RollbackAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly SchoolContext _context;
    private bool _disposed;

    // Lazy Repositories
    private IStudentRepository? _students;
    private ICourseRepository? _courses;
    private IInstructorRepository? _instructors;
    private IDepartmentRepository? _departments;
    private IEnrollmentRepository? _enrollments;
    private IRepository<Notification>? _notifications;
    private IRepository<ToDo>? _todos;

    public UnitOfWork(SchoolContext context)
    {
        _context = context;
    }

    // Repository Properties
    public IStudentRepository Students =>
        _students ??= new StudentRepository(_context);

    public ICourseRepository Courses =>
        _courses ??= new CourseRepository(_context);

    public IInstructorRepository Instructors =>
        _instructors ??= new InstructorRepository(_context);

    public IDepartmentRepository Departments =>
        _departments ??= new DepartmentRepository(_context);

    public IEnrollmentRepository Enrollments =>
        _enrollments ??= new EnrollmentRepository(_context);

    public IRepository<Notification> Notifications =>
        _notifications ??= new Repository<Notification>(_context);

    public IRepository<ToDo> ToDos =>
        _todos ??= new Repository<ToDo>(_context);

    // 提交所有變更
    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }

    // 回滾變更
    public async Task RollbackAsync()
    {
        await Task.Run(() =>
        {
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                }
            }
        });
    }

    // Dispose Pattern
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
```

#### 3.3.3 使用範例

```csharp
public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public async Task<bool> EnrollStudentAsync(int studentId, int courseId)
    {
        // 1. 取得實體
        var student = await _unitOfWork.Students.GetByIdAsync(studentId);
        var course = await _unitOfWork.Courses.GetByIdAsync(courseId);

        if (student == null || course == null)
            return false;

        // 2. 建立註冊記錄
        var enrollment = new Enrollment
        {
            StudentID = studentId,
            CourseID = courseId,
            Grade = null
        };

        // 3. 新增到 Repository（尚未提交到資料庫）
        await _unitOfWork.Enrollments.AddAsync(enrollment);

        // 4. 可選：更新課程統計（假設有此欄位）
        // course.EnrollmentCount++;
        // _unitOfWork.Courses.Update(course);

        // 5. 統一提交所有變更（事務性）
        await _unitOfWork.CommitAsync();

        return true;
    }
}
```

---

## 4. 服務生命週期管理

### 4.1 DbContext 生命週期

**配置**: Scoped（每個 HTTP 請求一個實例）

```csharp
builder.Services.AddDbContext<SchoolContext>(options =>
{
    options.UseSqlServer(connectionString);
    
    // 開發環境啟用詳細日誌
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
    
    // 效能最佳化
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // 預設不追蹤
}, ServiceLifetime.Scoped);
```

**最佳實踐**:
- ✅ 使用 `AsNoTracking()` 進行唯讀查詢
- ✅ 避免長時間持有 DbContext（如 Singleton 服務中）
- ✅ 使用 `SaveChangesAsync()` 而非 `SaveChanges()`

### 4.2 HttpClient 生命週期

**配置**: 使用 `IHttpClientFactory`（Singleton Factory）

```csharp
// 註冊 Typed Client
builder.Services.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// 使用 Polly 進行重試
builder.Services.AddHttpClient<IExternalApiService, ExternalApiService>()
    .AddTransientHttpErrorPolicy(policy => 
        policy.WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
```

### 4.3 Background Services

#### 4.3.1 通知處理 Worker

```csharp
public class NotificationProcessorWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationProcessorWorker> _logger;

    public NotificationProcessorWorker(
        IServiceProvider serviceProvider,
        ILogger<NotificationProcessorWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Processor Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 建立新 Scope 以取得 Scoped 服務
                using var scope = _serviceProvider.CreateScope();
                var queueService = scope.ServiceProvider
                    .GetRequiredService<INotificationQueueService>();
                var hubContext = scope.ServiceProvider
                    .GetRequiredService<IHubContext<NotificationHub>>();
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<SchoolContext>();

                // 從 Service Bus 接收訊息
                var notification = await queueService.ReceiveNotificationAsync();

                if (notification != null)
                {
                    // 儲存到資料庫
                    dbContext.Notifications.Add(notification);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    // 推送到前端
                    await hubContext.Clients.All.SendAsync(
                        "ReceiveNotification",
                        notification,
                        stoppingToken);

                    _logger.LogInformation(
                        "Processed notification: {Message}",
                        notification.Message);
                }
                else
                {
                    // 無訊息，短暫等待
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification");
                await Task.Delay(5000, stoppingToken); // 錯誤後等待較長時間
            }
        }

        _logger.LogInformation("Notification Processor Worker stopped");
    }
}

// 註冊
builder.Services.AddHostedService<NotificationProcessorWorker>();
```

---

## 5. 測試策略

### 5.1 單元測試服務層

```csharp
public class StudentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<INotificationQueueService> _mockNotificationQueue;
    private readonly Mock<ILogger<StudentService>> _mockLogger;
    private readonly StudentService _service;

    public StudentServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockNotificationQueue = new Mock<INotificationQueueService>();
        _mockLogger = new Mock<ILogger<StudentService>>();

        _service = new StudentService(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockNotificationQueue.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateStudentAsync_ShouldReturnStudentDto_WhenSuccessful()
    {
        // Arrange
        var createDto = new CreateStudentDto
        {
            FirstMidName = "John",
            LastName = "Doe",
            EnrollmentDate = DateTime.Now
        };

        var student = new Student { ID = 1, FirstMidName = "John", LastName = "Doe" };
        var studentDto = new StudentDto { Id = 1, FirstMidName = "John", LastName = "Doe" };

        _mockMapper.Setup(m => m.Map<Student>(createDto)).Returns(student);
        _mockMapper.Setup(m => m.Map<StudentDto>(student)).Returns(studentDto);

        _mockUnitOfWork.Setup(u => u.Students.AddAsync(student))
            .ReturnsAsync(student);
        _mockUnitOfWork.Setup(u => u.CommitAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.CreateStudentAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("John", result.FirstMidName);

        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        _mockNotificationQueue.Verify(
            n => n.SendNotificationAsync(It.IsAny<NotificationDto>()),
            Times.Once);
    }
}
```

### 5.2 整合測試 Repository

```csharp
public class StudentRepositoryIntegrationTests : IDisposable
{
    private readonly SchoolContext _context;
    private readonly StudentRepository _repository;

    public StudentRepositoryIntegrationTests()
    {
        // 使用 In-Memory Database
        var options = new DbContextOptionsBuilder<SchoolContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SchoolContext(options);
        _repository = new StudentRepository(_context);

        // Seed 測試資料
        SeedTestData();
    }

    private void SeedTestData()
    {
        _context.Students.AddRange(
            new Student { ID = 1, FirstMidName = "John", LastName = "Doe", EnrollmentDate = DateTime.Now },
            new Student { ID = 2, FirstMidName = "Jane", LastName = "Smith", EnrollmentDate = DateTime.Now }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetStudentWithEnrollmentsAsync_ShouldIncludeEnrollments()
    {
        // Arrange
        var enrollment = new Enrollment { StudentID = 1, CourseID = 1 };
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        // Act
        var student = await _repository.GetStudentWithEnrollmentsAsync(1);

        // Assert
        Assert.NotNull(student);
        Assert.NotEmpty(student.Enrollments);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

---

## 6. 效能考量

### 6.1 避免 N+1 查詢問題

```csharp
// ❌ 錯誤：N+1 問題
var students = await _context.Students.ToListAsync();
foreach (var student in students)
{
    // 每次迴圈都會產生一個額外查詢
    var enrollments = await _context.Enrollments
        .Where(e => e.StudentID == student.ID)
        .ToListAsync();
}

// ✅ 正確：使用 Include 預載
var students = await _context.Students
    .Include(s => s.Enrollments)
        .ThenInclude(e => e.Course)
    .ToListAsync();
```

### 6.2 查詢效能最佳化

```csharp
// 1. 使用 AsNoTracking 進行唯讀查詢
var students = await _context.Students
    .AsNoTracking()
    .ToListAsync();

// 2. 僅選取需要的欄位
var studentNames = await _context.Students
    .Select(s => new { s.ID, s.FirstMidName, s.LastName })
    .ToListAsync();

// 3. 使用 AsSplitQuery 避免 Cartesian Explosion
var instructors = await _context.Instructors
    .Include(i => i.CourseAssignments)
    .Include(i => i.OfficeAssignment)
    .AsSplitQuery()  // 分成多個查詢
    .ToListAsync();
```

---

## 7. 參考資源

- [Dependency Injection in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection)
- [Repository Pattern with EF Core](https://learn.microsoft.com/ef/core/testing/)
- [Unit of Work Pattern](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- [Clean Architecture by Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

**文件版本**: 1.0  
**最後更新**: 2026-02-05  
**作者**: Contoso University 架構團隊  
**審核者**: (待指定)
