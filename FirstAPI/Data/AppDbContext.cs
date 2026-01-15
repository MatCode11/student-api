using Microsoft.EntityFrameworkCore;
using FirstAPI.Models;

namespace FirstAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Teacher> Teachers { get; set; } = null!;
    }
}

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseMySql(connection, new MySqlServerVersion(new Version(8, 0, 33)))
);
