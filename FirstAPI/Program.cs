using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using FirstAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add controllers to the service container
builder.Services.AddControllers();

// Configure DbContext for MySQL
var connection = builder.Configuration.GetConnectionString("DefaultConnection") ?? "server=localhost;port=3306;database=firstapi_db;user=root;password=example";
// Use an explicit server version to avoid requiring a running DB at design time
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(connection, new MySqlServerVersion(new Version(8, 0, 33))));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

