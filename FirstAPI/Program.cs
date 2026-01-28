using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using FirstAPI.Data;
using Serilog;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Security.Claims;
using DotNetEnv;


// Load .env file if it exists
if (File.Exists(".env"))
{
    Env.Load();
}

// Configure Serilog BEFORE creating the builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/log-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging
builder.Host.UseSerilog();

// Add services to the container.
// Add controllers to the service container
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FirstAPI",
        Version = "v1",
        Description = "FirstAPI ASP.NET Core Web API with JWT authentication"
    });

    // XML comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // Group endpoints by controller name (tag)
    options.TagActionsBy(api =>
    {
        var controllerName = api.GroupName ?? api.ActionDescriptor.RouteValues["controller"];
        return new[] { controllerName ?? "Default" };
    });
    options.DocInclusionPredicate((_, _) => true);

    // JWT Bearer auth definition for Swagger
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token only (without the 'Bearer ' prefix).",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    };

    options.AddSecurityRequirement(securityRequirement);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure DbContext for MySQL
// Try to get from configuration first, then from environment variables
var connection = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING");

if (string.IsNullOrEmpty(connection))
{
    throw new Exception("Database connection string 'DefaultConnection' not found. Please set it in appsettings.json or CONNECTION_STRING environment variable.");
}

// Get JWT settings from configuration or environment variables
var jwtKey = builder.Configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "FirstAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "FirstAPIUsers";
var jwtExpiresMinutes = builder.Configuration["Jwt:ExpiresMinutes"] ?? Environment.GetEnvironmentVariable("JWT_EXPIRES_MINUTES") ?? "60";

if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("JWT Key not found. Please set it in appsettings.json (Jwt:Key) or JWT_KEY environment variable.");
}

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
                Encoding.UTF8.GetBytes(jwtKey)),
            // Map role claims from JWT token
            RoleClaimType = ClaimTypes.Role
        };
    });

// Configure role-based authorization policies
builder.Services.AddAuthorization(options =>
{
    // Admin policy - full access
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    
    // Teacher policy - teachers and admins
    options.AddPolicy("TeacherOrAdmin", policy => policy.RequireRole("Teacher", "Admin"));
    
    // User policy - all authenticated users (User, Teacher, Admin)
    options.AddPolicy("Authenticated", policy => policy.RequireRole("User", "Teacher", "Admin"));
});

// Use an explicit server version to avoid requiring a running DB at design time
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(connection, new MySqlServerVersion(new Version(8, 0, 33))));

// Register services
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateStudentDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// HTTPS Enforcement - Must be before other middleware
if (!app.Environment.IsDevelopment())
{
    // HSTS (HTTP Strict Transport Security) - Only in production
    app.UseHsts();
}

// Swagger only in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FirstAPI v1");
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

app.Run();

