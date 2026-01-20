//for POST
namespace FirstAPI.DTOs.Teachers;

public class CreateTeacherDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Subject { get; set; } = null!;
}
