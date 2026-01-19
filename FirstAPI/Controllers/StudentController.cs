using Microsoft.AspNetCore.Mvc;
using FirstAPI.Models;
using FirstAPI.Data;
using FirstAPI.DTOs.Students;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace FirstAPI.Controllers
{
    [ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;

    public StudentsController(IStudentService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStudentDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var student = await _service.GetByIdAsync(id);
        if (student == null) return NotFound();

        return Ok(student);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateStudentDto dto)
    {
        var success = await _service.UpdateAsync(id, dto);
        if (!success) return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound();

        return NoContent();
    }
}

}


// namespace FirstAPI.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class StudentController : ControllerBase
//     {
//         private readonly AppDbContext _db;

//         public StudentController(AppDbContext db)
//         {
//             _db = db;
//         }

//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetStudentById(int id)
//         {
//             var student = await _db.Students.FindAsync(id);

//             if (student == null)
//                 return NotFound();

//             var response = new StudentResponseDto
//             {
//                 Id = student.Id,
//                 FullName = $"{student.FirstName} {student.LastName}",
//                 Email = student.Email,
//                 Age = student.Age
//             };

//             return Ok(response);
//         }


//         [HttpPost]
//         public async Task<IActionResult> Create(CreateStudentDto dto)
//         {
//             var student = new Student
//             {
//                 FirstName = dto.FirstName,
//                 LastName = dto.LastName,
//                 Email = dto.Email,
//                 Age = dto.Age
//             };

//             _db.Students.Add(student);
//             await _db.SaveChangesAsync();

//             var response = new StudentResponseDto
//             {
//                 Id = student.Id,
//                 FullName = $"{student.FirstName} {student.LastName}",
//                 Email = student.Email,
//                 Age = student.Age
//             };

//             return CreatedAtAction(nameof(GetStudentById), new { id = student.Id }, response);
//         }

//         [HttpPut("{id}")]
//         public async Task<IActionResult> Update(int id, UpdateStudentDto dto)
//         {
//             var student = await _db.Students.FindAsync(id);

//             if (student == null)
//                 return NotFound();

//             student.FirstName = dto.FirstName;
//             student.LastName = dto.LastName;
//             student.Email = dto.Email;
//             student.Age = dto.Age;

//             await _db.SaveChangesAsync();

//             return NoContent(); // 204
//         }

//         [HttpDelete("{id}")]
//         public async Task<ActionResult> DeleteStudent(int id)
//         {
//             var student = await _db.Students.FindAsync(id);
//             if (student == null)
//             {
//                 return NotFound(new { message = "Student not found" });
//             }

//             _db.Students.Remove(student);
//             await _db.SaveChangesAsync();
//             return Ok(new { message = "Student deleted successfully" });
//         }

//     }
// }


// namespace FirstAPI.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class StudentController : ControllerBase
//     {
//         private readonly AppDbContext _db;

//         public StudentController(AppDbContext db)
//         {
//             _db = db;
//         }

//         // GET: api/student
//         //User Request all student
//         [HttpGet]
//         public async Task<ActionResult<IEnumerable<Student>>> GetAllStudents()
//         {
//             return Ok(await _db.Students.ToListAsync());
//         }

//         // GET: api/student/5
//         //User Request one student
//         [HttpGet("{id}")]
//         public async Task<ActionResult<Student>> GetStudentById(int id)
//         {
//             var student = await _db.Students.FindAsync(id);
//             if (student == null)
//             {
//                 return NotFound(new { message = "Student not found" });
//             }
//             return Ok(student);
//         }

//         // POST: api/student
//         // User creates a new student
//         [HttpPost]
//         public async Task<ActionResult<Student>> CreateStudent([FromBody] Student student)
//         {
//             if (student == null)
//             {
//                 return BadRequest(new { message = "Student data is required" });
//             }

//             _db.Students.Add(student);
//             await _db.SaveChangesAsync();

//             return CreatedAtAction(nameof(GetStudentById), new { id = student.Id }, student);
//         }

//         // PUT: api/student/5
//         // User updates an existing student
//         [HttpPut("{id}")]
//         public async Task<ActionResult<Student>> UpdateStudent(int id, [FromBody] Student student)
//         {
//             if (student == null)
//             {
//                 return BadRequest(new { message = "Student data is required" });
//             }

//             var existingStudent = await _db.Students.FindAsync(id);
//             if (existingStudent == null)
//             {
//                 return NotFound(new { message = "Student not found" });
//             }

//             existingStudent.FirstName = student.FirstName;
//             existingStudent.LastName = student.LastName;
//             existingStudent.Email = student.Email;
//             existingStudent.Age = student.Age;

//             await _db.SaveChangesAsync();

//             return Ok(existingStudent);
//         }

//         // DELETE: api/student/5
//         // User deletes an existing student
//         [HttpDelete("{id}")]
//         public async Task<ActionResult> DeleteStudent(int id)
//         {
//             var student = await _db.Students.FindAsync(id);
//             if (student == null)
//             {
//                 return NotFound(new { message = "Student not found" });
//             }

//             _db.Students.Remove(student);
//             await _db.SaveChangesAsync();
//             return Ok(new { message = "Student deleted successfully" });
//         }

//     }
// }

