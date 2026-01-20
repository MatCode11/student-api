//for GET
namespace FirstAPI.DTOs.Teachers;

public class TeacherResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Subject { get; set; } = null!;
}
