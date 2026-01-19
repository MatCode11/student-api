//for POST
namespace FirstAPI.DTOs.Students;

public class CreateStudentDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int Age { get; set; }
}
