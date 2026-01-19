using FirstAPI.DTOs.Students;

public interface IStudentService
{
    Task<StudentResponseDto> CreateAsync(CreateStudentDto dto);
    Task<StudentResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<StudentResponseDto>> GetAllAsync();
    Task<bool> UpdateAsync(int id, UpdateStudentDto dto);
    Task<bool> DeleteAsync(int id);
}
