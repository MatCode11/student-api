using FirstAPI.Data;
using FirstAPI.DTOs.Teachers;
using FirstAPI.Models;
using Microsoft.EntityFrameworkCore;

public class TeacherService : ITeacherService
{
    private readonly AppDbContext _context;

    public TeacherService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherResponseDto> CreateAsync(CreateTeacherDto dto)
    {
        var teacher = new Teacher
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Subject = dto.Subject
        };

        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        return MapToResponse(teacher);
    }

    public async Task<TeacherResponseDto?> GetByIdAsync(int id)
    {
        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher == null) return null;

        return MapToResponse(teacher);
    }

    public async Task<IEnumerable<TeacherResponseDto>> GetAllAsync()
    {
        return await _context.Teachers  
            .Select(t => MapToResponse(t))
            .ToListAsync();
    }

    public async Task<bool> UpdateAsync(int id, UpdateTeacherDto dto)
    {
        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher == null) return false;

        teacher.FirstName = dto.FirstName;
        teacher.LastName = dto.LastName;
        teacher.Email = dto.Email;
        teacher.Subject = dto.Subject;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher == null) return false;

        _context.Teachers.Remove(teacher);
        await _context.SaveChangesAsync();
        return true;
    }

    private static TeacherResponseDto MapToResponse(Teacher teacher)
    {
        return new TeacherResponseDto
        {
            Id = teacher.Id,
            FullName = $"{teacher.FirstName} {teacher.LastName}",
            Email = teacher.Email,
            Subject = teacher.Subject
        };
    }
}
