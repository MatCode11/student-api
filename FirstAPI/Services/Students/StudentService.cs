using FirstAPI.Data;
using FirstAPI.DTOs.Students;
using FirstAPI.Models;
using Microsoft.EntityFrameworkCore;

public class StudentService : IStudentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<StudentService> _logger;

    public StudentService(AppDbContext context, ILogger<StudentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<StudentResponseDto> CreateAsync(CreateStudentDto dto)
    {
        var student = new Student
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Age = dto.Age
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return MapToResponse(student);
    }

    public async Task<StudentResponseDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching student with ID {StudentId}", id);

        var student = await _context.Students.FindAsync(id);
        if (student == null)
        {
            _logger.LogWarning("Student with ID {StudentId} not found", id);
            return null;
        }

        return MapToResponse(student);
    }

    public async Task<IEnumerable<StudentResponseDto>> GetAllAsync()
    {
        return await _context.Students
            .Select(s => MapToResponse(s))
            .ToListAsync();
    }

    public async Task<bool> UpdateAsync(int id, UpdateStudentDto dto)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null) return false;

        student.FirstName = dto.FirstName;
        student.LastName = dto.LastName;
        student.Email = dto.Email;
        student.Age = dto.Age;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null) return false;

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        return true;
    }

    private static StudentResponseDto MapToResponse(Student student)
    {
        return new StudentResponseDto
        {
            Id = student.Id,
            FullName = $"{student.FirstName} {student.LastName}",
            Email = student.Email,
            Age = student.Age
        };
    }
}
