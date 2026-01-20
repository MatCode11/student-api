using FirstAPI.DTOs.Teachers;
using FirstAPI.Models;

public interface ITeacherService
{
    Task<TeacherResponseDto> CreateAsync(CreateTeacherDto dto);
    Task<TeacherResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<TeacherResponseDto>> GetAllAsync();
    Task<bool> UpdateAsync(int id, UpdateTeacherDto dto);
    Task<bool> DeleteAsync(int id);
}
