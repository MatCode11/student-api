# FirstAPI - Architecture Documentation

## Table of Contents
1. [High-Level Architecture Overview](#high-level-architecture-overview)
2. [Layered Architecture and Responsibilities](#layered-architecture-and-responsibilities)
3. [Dependency Injection](#dependency-injection)
4. [Request Lifecycle](#request-lifecycle)
5. [Middleware Pipeline](#middleware-pipeline)
6. [DTOs and Domain Protection](#dtos-and-domain-protection)
7. [Services and Business Logic](#services-and-business-logic)
8. [Why NOT to Put Logic in Controllers](#why-not-to-put-logic-in-controllers)
9. [Entity Framework Core Integration](#entity-framework-core-integration)
10. [Cross-Cutting Concerns](#cross-cutting-concerns)
11. [Scaling for Large Projects](#scaling-for-large-projects)
12. [Common Anti-Patterns to Avoid](#common-anti-patterns-to-avoid)

---

## High-Level Architecture Overview

FirstAPI follows a **layered architecture pattern**, which is one of the most common and effective patterns for building maintainable web APIs. Think of it like building a house with distinct floors, each with a specific purpose.

```
┌─────────────────────────────────────────────────────────┐
│                    HTTP Request                          │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│              API Layer (Controllers)                     │
│  • Receives HTTP requests                               │
│  • Validates input (via DTOs)                           │
│  • Delegates to services                                │
│  • Returns HTTP responses                               │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│         Application Layer (Services)                    │
│  • Contains business logic                              │
│  • Orchestrates data operations                         │
│  • Validates business rules                             │
│  • Coordinates between layers                           │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│         Infrastructure Layer (Data Access)               │
│  • Entity Framework Core (DbContext)                    │
│  • Database connections                                 │
│  • Migrations                                           │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│              Database (MySQL)                           │
└─────────────────────────────────────────────────────────┘
```

**Key Principles:**
- **Separation of Concerns**: Each layer has one clear responsibility
- **Dependency Direction**: Layers depend on layers below them, never above
- **Abstraction**: Upper layers depend on interfaces, not concrete implementations
- **Testability**: Each layer can be tested independently

---

## Layered Architecture and Responsibilities

### 1. API Layer (Controllers)

**Location**: `FirstAPI/Controllers/`

**Purpose**: The API layer is the **entry point** for all HTTP requests. It's the "front door" of your application.

**Responsibilities:**
- ✅ Receive HTTP requests (GET, POST, PUT, DELETE)
- ✅ Parse request data into DTOs
- ✅ Call appropriate service methods
- ✅ Map service results to HTTP responses (200 OK, 404 Not Found, etc.)
- ✅ Handle HTTP-specific concerns (status codes, headers, content negotiation)

**What it should NOT do:**
- ❌ Direct database access
- ❌ Business logic
- ❌ Complex data transformations
- ❌ Validation logic (beyond basic HTTP validation)

**Example from FirstAPI:**

```csharp
[ApiController]
[Route("api/students")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;

    public StudentsController(IStudentService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStudentDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

**Why this is good:**
- Controller is **thin** - it just delegates to the service
- Uses **dependency injection** to get the service
- Returns appropriate **HTTP status codes**
- Uses **DTOs** instead of exposing domain models directly

---

### 2. Application Layer (Services)

**Location**: `FirstAPI/Services/`

**Purpose**: The application layer contains your **business logic** - the "what" and "why" of your application, not just the "how".

**Responsibilities:**
- ✅ Implement business rules and logic
- ✅ Coordinate between multiple data operations
- ✅ Transform between DTOs and domain models
- ✅ Handle business-level validation
- ✅ Orchestrate complex workflows

**Example from FirstAPI:**

```csharp
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly PasswordHasher<User> _hasher = new();

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Business rule: Email must be unique
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        // Business logic: Create user and hash password
        var user = new User
        {
            Email = dto.Email,
            Role = "User"
        };
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return GenerateToken(user);
    }
}
```

**Key Points:**
- Service implements an **interface** (`IAuthService`) for testability
- Contains **business rules** (email uniqueness check)
- Handles **security concerns** (password hashing)
- Returns **DTOs**, not domain models

---

### 3. DTO Layer

**Location**: `FirstAPI/DTOs/`

**Purpose**: DTOs (Data Transfer Objects) are **contracts** between your API and the outside world. They define what data can be sent in and what data comes out.

**Types of DTOs in FirstAPI:**
- **Request DTOs**: `RegisterDto`, `LoginDto`, `CreateStudentDto`, `UpdateStudentDto`
- **Response DTOs**: `AuthResponseDto`, `StudentResponseDto`

**Example:**

```csharp
public class RegisterDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
```

**Why DTOs exist:**
1. **API Contract**: DTOs define your API's public interface
2. **Versioning**: You can change domain models without breaking API contracts
3. **Security**: Hide internal structure (e.g., don't expose `PasswordHash`)
4. **Validation**: DTOs can have different validation rules than domain models
5. **Performance**: Return only needed data (e.g., `StudentResponseDto` with `FullName` instead of separate `FirstName`/`LastName`)

**DTO vs Domain Model:**
- **DTO**: What the API accepts/returns (public contract)
- **Domain Model**: What your application uses internally (business entities)

---

### 4. Domain Layer (Models / Entities)

**Location**: `FirstAPI/Models/` and `FirstAPI/Data/User.cs`

**Purpose**: Domain models represent your **business entities** - the core concepts your application works with.

**Example:**

```csharp
public class Student
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Range(0, 150)]
    public int Age { get; set; }
}
```

**Characteristics:**
- **Rich models**: Can contain validation attributes, computed properties, business methods
- **Persistence-agnostic**: Models don't know about databases
- **Business-focused**: Represent real-world concepts (Student, Teacher, User)

**Note**: In FirstAPI, domain models are kept simple. In larger applications, you might have:
- Value objects (e.g., `Email`, `Money`)
- Domain events
- Aggregate roots
- Rich domain models with behavior

---

### 5. Infrastructure Layer (Data, DbContext, Migrations)

**Location**: `FirstAPI/Data/` and `FirstAPI/Migrations/`

**Purpose**: The infrastructure layer handles **technical concerns** - how data is stored, retrieved, and persisted.

#### DbContext

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Teacher> Teachers { get; set; } = null!;
}
```

**Responsibilities:**
- Define database schema (via `DbSet<T>`)
- Configure entity relationships
- Handle database connections
- Track entity changes
- Execute queries and commands

#### Migrations

**Location**: `FirstAPI/Migrations/`

Migrations are **version control for your database schema**. They allow you to:
- Track schema changes over time
- Apply changes to different environments (dev, staging, production)
- Roll back changes if needed

**How migrations work:**
1. You change your models or DbContext
2. Run `dotnet ef migrations add MigrationName`
3. EF Core generates migration code
4. Run `dotnet ef database update` to apply to database

---

## Dependency Injection

**What is Dependency Injection (DI)?**

Dependency Injection is a design pattern where objects receive their dependencies from an external source rather than creating them internally.

### Why Use DI?

**Without DI (Bad):**
```csharp
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _db;
    
    public StudentsController()
    {
        // Controller creates its own dependencies - BAD!
        _db = new AppDbContext(...); // How do we configure this?
    }
}
```

**Problems:**
- Hard to test (can't mock `AppDbContext`)
- Tight coupling (controller depends on concrete class)
- Hard to configure (where do connection strings come from?)
- Can't swap implementations (e.g., use in-memory DB for tests)

**With DI (Good):**
```csharp
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;
    
    public StudentsController(IStudentService service)
    {
        _service = service; // Dependency is injected - GOOD!
    }
}
```

**Benefits:**
- ✅ **Testable**: Can inject mock services in tests
- ✅ **Flexible**: Can swap implementations easily
- ✅ **Loose coupling**: Depends on interface, not concrete class
- ✅ **Centralized configuration**: All dependencies configured in `Program.cs`

### DI in FirstAPI

**Registration in `Program.cs`:**
```csharp
// Register services with their lifetimes
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseMySql(connection, new MySqlServerVersion(new Version(8, 0, 33))));
```

**Service Lifetimes:**
- **Scoped**: One instance per HTTP request (most common for services)
- **Singleton**: One instance for the entire application lifetime
- **Transient**: New instance every time it's requested

**Why Scoped for Services?**
- Each HTTP request gets its own service instance
- Perfect for database operations (one `DbContext` per request)
- Thread-safe within a single request

---

## Request Lifecycle

Understanding the request lifecycle helps you debug issues and understand where things happen.

### Complete Request Flow

```
1. HTTP Request arrives
   │
   ├─► HTTPS Redirection (if needed)
   │
   ├─► CORS Middleware
   │   └─► Checks if origin is allowed
   │
   ├─► Authentication Middleware
   │   └─► Validates JWT token (if [Authorize] attribute present)
   │
   ├─► Authorization Middleware
   │   └─► Checks if user has required permissions
   │
   ├─► Serilog Request Logging
   │   └─► Logs request details
   │
   ├─► Exception Middleware (wraps everything)
   │   │
   │   ├─► Route Matching
   │   │   └─► Finds matching controller/action
   │   │
   │   ├─► Model Binding
   │   │   └─► Converts JSON/query params to DTOs
   │   │
   │   ├─► FluentValidation
   │   │   └─► Validates DTOs using validators
   │   │
   │   ├─► Controller Action
   │   │   ├─► Calls Service
   │   │   │   ├─► Service executes business logic
   │   │   │   ├─► Service uses DbContext
   │   │   │   └─► DbContext queries database
   │   │   └─► Returns result
   │   │
   │   └─► Response Serialization
   │       └─► Converts result to JSON
   │
   └─► HTTP Response sent back
```

### Step-by-Step Example: `POST /api/students`

1. **Request arrives**: `POST /api/students` with JSON body
2. **HTTPS Redirection**: Redirects to HTTPS if needed
3. **CORS**: Checks if origin is allowed
4. **Authentication**: Validates JWT token (because of `[Authorize]`)
5. **Route Matching**: Finds `StudentsController.Create()`
6. **Model Binding**: Converts JSON to `CreateStudentDto`
7. **Validation**: `CreateStudentDtoValidator` validates the DTO
8. **Controller Action**: `Create()` method executes
9. **Service Call**: `_service.CreateAsync(dto)` is called
10. **Business Logic**: Service checks business rules, creates `Student` entity
11. **Database**: `DbContext` saves to MySQL
12. **Response**: Controller returns `201 Created` with location header
13. **Serialization**: Response is converted to JSON
14. **Response sent**: HTTP response sent back to client

---

## Middleware Pipeline

Middleware are components that process HTTP requests and responses. They form a **pipeline** where each middleware can:
- Process the request before it reaches the next middleware
- Process the response after it comes back
- Short-circuit the pipeline (e.g., return 401 Unauthorized)

### Middleware Order in FirstAPI

**Order matters!** Middleware executes in the order it's registered.

```csharp
// Program.cs - Middleware pipeline order

app.UseHttpsRedirection();        // 1. Force HTTPS
app.UseCors("AllowAll");          // 2. Handle CORS
app.UseAuthentication();          // 3. Authenticate user (JWT)
app.UseAuthorization();           // 4. Check permissions
app.UseSerilogRequestLogging();   // 5. Log requests
app.UseMiddleware<ExceptionMiddleware>(); // 6. Catch exceptions
app.MapControllers();             // 7. Route to controllers
```

### Exception Middleware

**Location**: `FirstAPI/Middleware/ExceptionMiddleware.cs`

```csharp
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // Continue to next middleware
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

**Why it's important:**
- **Centralized error handling**: All exceptions caught in one place
- **Consistent error responses**: All errors return same format
- **Logging**: All errors are logged automatically
- **Security**: Prevents stack traces from leaking to clients

**Why it's placed near the end:**
- It wraps the entire pipeline
- Catches exceptions from controllers, services, and database operations

### Authentication Middleware

**Purpose**: Validates JWT tokens and populates `HttpContext.User`

**Configuration in `Program.cs`:**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });
```

**How it works:**
1. Checks for `Authorization: Bearer <token>` header
2. Validates token signature, expiration, issuer, audience
3. If valid, populates `HttpContext.User` with claims
4. If invalid, returns `401 Unauthorized`

---

## DTOs and Domain Protection

### Why DTOs Exist

**Problem without DTOs:**
```csharp
// BAD: Exposing domain model directly
[HttpPost]
public async Task<IActionResult> Create(Student student)
{
    // What if Student has properties we don't want to expose?
    // What if we need different validation rules?
    // What if we want to change the model without breaking the API?
}
```

**Solution with DTOs:**
```csharp
// GOOD: Using DTOs
[HttpPost]
public async Task<IActionResult> Create(CreateStudentDto dto)
{
    // DTO defines exactly what the API accepts
    // Domain model can change without affecting API
    // Different validation rules can be applied
}
```

### How DTOs Protect the Domain

1. **Hide Internal Structure**
   ```csharp
   // Domain model might have:
   public class User
   {
       public string PasswordHash { get; set; } // NEVER expose this!
       public DateTime CreatedAt { get; set; }
       public string InternalNotes { get; set; }
   }
   
   // DTO only exposes what's needed:
   public class RegisterDto
   {
       public string Email { get; set; }
       public string Password { get; set; } // Plain text, not hash
   }
   ```

2. **Different Validation Rules**
   ```csharp
   // Domain model validation (data integrity):
   public class Student
   {
       [MaxLength(100)]
       public string FirstName { get; set; }
   }
   
   // DTO validation (API contract):
   public class CreateStudentDto
   {
       [Required]
       [MinLength(2)]
       [MaxLength(50)] // Different max length for API
       public string FirstName { get; set; }
   }
   ```

3. **API Versioning**
   ```csharp
   // v1 DTO
   public class CreateStudentDtoV1
   {
       public string Name { get; set; } // Combined name
   }
   
   // v2 DTO
   public class CreateStudentDtoV2
   {
       public string FirstName { get; set; } // Split name
       public string LastName { get; set; }
   }
   
   // Domain model stays the same!
   ```

4. **Performance Optimization**
   ```csharp
   // Return only what's needed
   public class StudentResponseDto
   {
       public int Id { get; set; }
       public string FullName { get; set; } // Computed, not stored
       // Don't return PasswordHash, InternalNotes, etc.
   }
   ```

---

## Services and Business Logic

### What is Business Logic?

Business logic is the **rules and processes** that define how your application works. It's the "why" behind your code.

**Examples of Business Logic:**
- "A user cannot register with an email that already exists"
- "Passwords must be hashed before storing"
- "Students under 18 need parental consent"
- "Orders over $100 get free shipping"
- "Users can only edit their own posts"

### Why Services Exist

**Services encapsulate business logic** and keep it out of controllers and data access code.

**Without Services (Bad):**
```csharp
[HttpPost("register")]
public async Task<IActionResult> Register(RegisterDto dto)
{
    // Business logic in controller - BAD!
    var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
    if (existingUser != null)
        return Conflict("Email exists");
    
    var user = new User { Email = dto.Email };
    user.PasswordHash = HashPassword(dto.Password); // Business logic!
    
    _db.Users.Add(user);
    await _db.SaveChangesAsync();
    
    return Ok();
}
```

**Problems:**
- ❌ Can't reuse logic (what if you need to register from another endpoint?)
- ❌ Hard to test (controller tests are complex)
- ❌ Violates Single Responsibility Principle
- ❌ Business logic scattered across controllers

**With Services (Good):**
```csharp
// Controller is thin
[HttpPost("register")]
public async Task<IActionResult> Register(RegisterDto dto)
{
    var result = await _authService.RegisterAsync(dto);
    return Ok(result);
}

// Service contains business logic
public class AuthService : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Business rule: Email must be unique
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email is already registered.");
        
        // Business logic: Create and hash password
        var user = new User { Email = dto.Email };
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return GenerateToken(user);
    }
}
```

**Benefits:**
- ✅ **Reusable**: Can call `RegisterAsync` from anywhere
- ✅ **Testable**: Can test business logic without HTTP concerns
- ✅ **Maintainable**: Business rules in one place
- ✅ **Single Responsibility**: Controller handles HTTP, service handles business

### Service Responsibilities

**Services should:**
- ✅ Implement business rules
- ✅ Coordinate multiple operations
- ✅ Transform between DTOs and domain models
- ✅ Handle business-level validation
- ✅ Orchestrate workflows

**Services should NOT:**
- ❌ Directly handle HTTP concerns (that's controllers)
- ❌ Know about routing, status codes, headers
- ❌ Contain presentation logic
- ❌ Be too large (split into smaller services if needed)

---

## Why NOT to Put Logic in Controllers

### The Controller's Job

Controllers are **coordinators**, not workers. Their job is to:
1. Receive HTTP requests
2. Call the right service
3. Return HTTP responses

That's it!

### What Happens When You Put Logic in Controllers

**Example of Bad Controller:**
```csharp
[HttpPost("register")]
public async Task<IActionResult> Register(RegisterDto dto)
{
    // ❌ Business logic in controller
    if (string.IsNullOrEmpty(dto.Email))
        return BadRequest("Email required");
    
    // ❌ Data access in controller
    var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
    if (exists)
        return Conflict("Email exists");
    
    // ❌ Business logic (password hashing)
    var hash = BCrypt.HashPassword(dto.Password);
    
    // ❌ Data access
    var user = new User { Email = dto.Email, PasswordHash = hash };
    _db.Users.Add(user);
    await _db.SaveChangesAsync();
    
    // ❌ More business logic (token generation)
    var token = GenerateJwtToken(user);
    
    return Ok(new { token });
}
```

**Problems:**
1. **Can't Reuse**: Logic is trapped in the controller
2. **Hard to Test**: Need to mock HTTP context, database, etc.
3. **Violates SRP**: Controller does too many things
4. **Tight Coupling**: Controller depends on `DbContext` directly
5. **Hard to Maintain**: Business rules scattered across controllers

### The Right Way

```csharp
// Controller: Thin and focused
[HttpPost("register")]
public async Task<IActionResult> Register(RegisterDto dto)
{
    try
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Conflict(new { message = ex.Message });
    }
}

// Service: Contains all the logic
public class AuthService : IAuthService
{
    // All business logic here - reusable, testable, maintainable
}
```

### Rule of Thumb

**If your controller method is more than 10-15 lines, you're probably doing too much.**

Controllers should be **thin** - they're just the "glue" between HTTP and your business logic.

---

## Entity Framework Core Integration

### What is EF Core?

Entity Framework Core is an **ORM (Object-Relational Mapper)** that:
- Maps C# classes to database tables
- Converts LINQ queries to SQL
- Tracks entity changes
- Manages database connections

### How EF Core Fits In

```
┌─────────────────┐
│   Service       │
│  (Business      │
│   Logic)        │
└────────┬────────┘
         │ Uses
         ▼
┌─────────────────┐
│   DbContext     │
│  (EF Core)      │
└────────┬────────┘
         │ Converts to
         ▼
┌─────────────────┐
│   SQL Queries   │
└────────┬────────┘
         │ Executes
         ▼
┌─────────────────┐
│   MySQL DB      │
└─────────────────┘
```

### DbContext in FirstAPI

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Teacher> Teachers { get; set; } = null!;
}
```

**What DbContext does:**
- **DbSet<T>**: Represents a table in the database
- **Change Tracking**: Tracks which entities have changed
- **Query Translation**: Converts LINQ to SQL
- **Connection Management**: Handles database connections

### Using EF Core in Services

```csharp
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    
    public async Task<User?> GetUserAsync(string email)
    {
        // LINQ query - EF Core converts to SQL
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task CreateUserAsync(User user)
    {
        _context.Users.Add(user); // Tracked by EF Core
        await _context.SaveChangesAsync(); // Generates INSERT SQL
    }
}
```

### Migrations

Migrations are **version control for your database schema**.

**Workflow:**
1. Change your model or DbContext
2. Create migration: `dotnet ef migrations add AddEmailToUser`
3. Review generated migration code
4. Apply to database: `dotnet ef database update`

**Migration File Example:**
```csharp
public partial class AddUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", ...),
                Email = table.Column<string>(type: "varchar(255)", nullable: false),
                // ...
            });
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Users");
    }
}
```

**Why Migrations Matter:**
- ✅ **Version Control**: Track schema changes over time
- ✅ **Reproducible**: Same schema in dev, staging, production
- ✅ **Rollback**: Can undo changes if needed
- ✅ **Team Collaboration**: Everyone has the same schema

---

## Cross-Cutting Concerns

Cross-cutting concerns are features that affect **multiple layers** of your application. They "cut across" the architecture.

### 1. Logging

**Location**: Configured in `Program.cs`, used throughout application

**Implementation**: Serilog

```csharp
// Configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

**Usage:**
```csharp
// In middleware
_logger.LogError(ex, ex.Message);

// In services (via dependency injection)
public class AuthService
{
    private readonly ILogger<AuthService> _logger;
    
    public AuthService(ILogger<AuthService> logger)
    {
        _logger = logger;
    }
    
    public async Task RegisterAsync(RegisterDto dto)
    {
        _logger.LogInformation("Registering user with email: {Email}", dto.Email);
        // ...
    }
}
```

**Why centralized logging:**
- ✅ Consistent log format across application
- ✅ Can change logging provider without changing code
- ✅ Request logging middleware logs all requests automatically

### 2. Validation

**Location**: `FirstAPI/Validators/`

**Implementation**: FluentValidation

```csharp
// Validator
public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"[A-Z]")
            .Matches(@"[a-z]")
            .Matches(@"[0-9]");
    }
}
```

**Registration:**
```csharp
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
```

**How it works:**
1. DTO is received by controller
2. FluentValidation automatically validates it
3. If invalid, returns `400 Bad Request` with validation errors
4. If valid, continues to controller action

**Why FluentValidation:**
- ✅ Separation: Validation logic separate from models
- ✅ Testable: Can test validators independently
- ✅ Reusable: Same validator for different scenarios
- ✅ Rich: Complex validation rules (regex, custom validators)

### 3. Authentication & Authorization

**Location**: Configured in `Program.cs`, used via `[Authorize]` attribute

**Implementation**: JWT Bearer Authentication

```csharp
// Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* ... */ });

builder.Services.AddAuthorization();

// Usage
[Authorize]
public class StudentsController : ControllerBase
{
    // All actions require authentication
}

[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
    // Only admins can delete
}
```

**How it works:**
1. User logs in, receives JWT token
2. Client sends token in `Authorization: Bearer <token>` header
3. Authentication middleware validates token
4. If valid, `HttpContext.User` is populated with claims
5. Authorization checks if user has required permissions

**Why centralized auth:**
- ✅ Single source of truth for authentication
- ✅ Applied via attributes, not code
- ✅ Can protect entire controllers or individual actions
- ✅ Works with middleware pipeline

### 4. Exception Handling

**Location**: `FirstAPI/Middleware/ExceptionMiddleware.cs`

**Purpose**: Catch all exceptions and return consistent error responses

```csharp
public class ExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

**Benefits:**
- ✅ No need for try-catch in every controller
- ✅ Consistent error format
- ✅ Automatic logging
- ✅ Security (no stack traces in production)

---

## Scaling for Large Projects

The architecture in FirstAPI is designed to **scale** as your project grows. Here's how:

### 1. Modular Structure

**Current Structure:**
```
FirstAPI/
├── Controllers/
│   ├── AuthController.cs
│   ├── StudentController.cs
│   └── TeacherController.cs
├── Services/
│   ├── Auth/
│   ├── Students/
│   └── Teachers/
└── DTOs/
    ├── Auth/
    ├── Students/
    └── Teachers/
```

**As you grow, you can:**
- Add new features following the same pattern
- Each feature is self-contained (controller, service, DTOs)
- Easy to find code (by feature, not by layer)

### 2. Interface-Based Design

**Current:**
```csharp
public interface IAuthService { }
public interface IStudentService { }
```

**Benefits:**
- Easy to swap implementations
- Easy to mock for testing
- Can create decorators (e.g., `CachingAuthService` wraps `AuthService`)

### 3. Separation of Concerns

Each layer has a clear responsibility:
- **Controllers**: HTTP concerns
- **Services**: Business logic
- **Data**: Persistence

**This allows:**
- Different teams to work on different layers
- Easy to understand and maintain
- Changes in one layer don't affect others

### 4. Adding New Features

**To add a new feature (e.g., "Courses"):**

1. **Create Domain Model**: `Models/Course.cs`
2. **Add to DbContext**: `AppDbContext.Courses`
3. **Create DTOs**: `DTOs/Courses/CreateCourseDto.cs`, etc.
4. **Create Validators**: `Validators/Courses/CreateCourseDtoValidator.cs`
5. **Create Service Interface**: `Services/Courses/ICourseService.cs`
6. **Create Service**: `Services/Courses/CourseService.cs`
7. **Create Controller**: `Controllers/CourseController.cs`
8. **Register Service**: `Program.cs` → `AddScoped<ICourseService, CourseService>()`

**Follow the pattern, and it scales!**

### 5. Advanced Patterns for Large Projects

**When FirstAPI grows, consider:**

1. **Repository Pattern**: Abstract data access further
   ```csharp
   public interface IStudentRepository
   {
       Task<Student?> GetByIdAsync(int id);
       Task<List<Student>> GetAllAsync();
   }
   ```

2. **Unit of Work**: Coordinate multiple repositories
   ```csharp
   public interface IUnitOfWork
   {
       IStudentRepository Students { get; }
       ITeacherRepository Teachers { get; }
       Task SaveChangesAsync();
   }
   ```

3. **CQRS**: Separate read and write operations
   ```csharp
   // Commands (write)
   public interface ICommandHandler<TCommand> { }
   
   // Queries (read)
   public interface IQueryHandler<TQuery, TResult> { }
   ```

4. **Mediator Pattern**: Decouple controllers from services
   ```csharp
   // Instead of: _service.CreateAsync(dto)
   // Use: _mediator.Send(new CreateStudentCommand(dto))
   ```

5. **Domain Events**: React to domain changes
   ```csharp
   public class StudentCreatedEvent
   {
       public int StudentId { get; set; }
   }
   ```

**But start simple!** FirstAPI's current architecture is perfect for learning and small-to-medium projects.

---

## Common Anti-Patterns to Avoid

### 1. Anemic Domain Model

**Anti-Pattern:**
```csharp
// Model is just data, no behavior
public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

// Logic is elsewhere
public string GetFullName(Student student)
{
    return $"{student.FirstName} {student.LastName}";
}
```

**Better:**
```csharp
public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    // Behavior in the model
    public string FullName => $"{FirstName} {LastName}";
    
    public bool CanEnrollInCourse(Course course)
    {
        // Business rule in the model
        return Age >= course.MinimumAge;
    }
}
```

### 2. God Controller

**Anti-Pattern:**
```csharp
// Controller does everything
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly IPdfGenerator _pdf;
    // ... 10 more dependencies
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateStudentDto dto)
    {
        // 100 lines of business logic
    }
}
```

**Better:** Thin controllers that delegate to services.

### 3. Service Layer Anemia

**Anti-Pattern:**
```csharp
// Service is just a pass-through
public class StudentService : IStudentService
{
    public async Task<Student> GetByIdAsync(int id)
    {
        return await _db.Students.FindAsync(id);
    }
}
```

**Better:** Services should contain business logic, not just wrap data access.

### 4. Exposing Domain Models Directly

**Anti-Pattern:**
```csharp
[HttpGet]
public async Task<ActionResult<Student>> GetStudent(int id)
{
    return await _db.Students.FindAsync(id);
    // Exposes internal structure, password hashes, etc.
}
```

**Better:** Always use DTOs for API contracts.

### 5. Business Logic in Controllers

**Anti-Pattern:**
```csharp
[HttpPost("register")]
public async Task<IActionResult> Register(RegisterDto dto)
{
    // Business logic here - BAD!
    if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
        return Conflict();
    
    var hash = HashPassword(dto.Password);
    // ...
}
```

**Better:** Move all business logic to services.

### 6. Tight Coupling

**Anti-Pattern:**
```csharp
// Controller depends on concrete class
public class StudentsController
{
    private readonly StudentService _service; // BAD!
}
```

**Better:**
```csharp
// Controller depends on interface
public class StudentsController
{
    private readonly IStudentService _service; // GOOD!
}
```

### 7. Ignoring Dependency Injection

**Anti-Pattern:**
```csharp
public class StudentsController
{
    private readonly AppDbContext _db = new AppDbContext(); // BAD!
}
```

**Better:** Always use dependency injection.

### 8. Database Logic in Services

**Anti-Pattern:**
```csharp
public class StudentService
{
    public async Task<List<Student>> GetStudents()
    {
        // Raw SQL in service - BAD!
        return await _db.Database
            .SqlQueryRaw<Student>("SELECT * FROM Students WHERE ...")
            .ToListAsync();
    }
}
```

**Better:** Use DbContext and LINQ. If you need raw SQL, use a repository.

### 9. No Validation

**Anti-Pattern:**
```csharp
[HttpPost]
public async Task<IActionResult> Create(RegisterDto dto)
{
    // No validation - accepts anything!
    await _service.RegisterAsync(dto);
}
```

**Better:** Always validate input using FluentValidation or data annotations.

### 10. Exception Swallowing

**Anti-Pattern:**
```csharp
public async Task<IActionResult> Create(CreateStudentDto dto)
{
    try
    {
        await _service.CreateAsync(dto);
    }
    catch (Exception ex)
    {
        // Swallowing exception - BAD!
        return Ok(); // User thinks it worked!
    }
}
```

**Better:** Let exceptions bubble up to exception middleware, or handle them explicitly and return appropriate status codes.

---

## Summary

FirstAPI follows a **clean, layered architecture** that:

✅ **Separates concerns** - Each layer has one responsibility  
✅ **Uses dependency injection** - Loose coupling, easy to test  
✅ **Protects the domain** - DTOs shield internal structure  
✅ **Encapsulates business logic** - Services contain rules and processes  
✅ **Handles cross-cutting concerns** - Logging, validation, auth centralized  
✅ **Scales well** - Easy to add new features following the pattern  

**Key Takeaways:**
- Controllers are thin coordinators
- Services contain business logic
- DTOs define API contracts
- Domain models represent business entities
- Infrastructure handles technical concerns
- Middleware handles cross-cutting concerns

This architecture is **professional, maintainable, and scalable**. As you learn and grow, you can add more advanced patterns (repositories, CQRS, etc.), but the foundation is solid.

---

**Remember**: Good architecture is about making code **easy to understand, test, and change**. The patterns in FirstAPI achieve exactly that.
