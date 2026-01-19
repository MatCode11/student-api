//for GET
namespace FirstAPI.DTOs.Students;

public class StudentResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int Age { get; set; }
}
