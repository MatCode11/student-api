using Microsoft.AspNetCore.Mvc;
using FirstAPI.Models;
using FirstAPI.Data;
using FirstAPI.DTOs.Students;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


namespace FirstAPI.Controllers
{
    [ApiController]
    [Route("api/students")]
    [Authorize(Policy = "Authenticated")]
    public class StudentsController : ControllerBase
    {
    private readonly IStudentService _service;

        public StudentsController(IStudentService service)
        {
            _service = service;
        }

        /// <summary>
        /// Creates a new student.
        /// </summary>
        /// <remarks>
        /// This endpoint is restricted to users with the Teacher or Admin role.
        /// A valid JWT access token must be provided in the Authorization header.
        /// </remarks>
        /// <param name="dto">The student data to create.</param>
        /// <response code="201">The student was created successfully.</response>
        /// <response code="400">The request data is invalid.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user is authenticated but not in the required role.</response>
        [HttpPost]
        [Authorize(Policy = "TeacherOrAdmin")]
        [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(CreateStudentDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Gets a specific student by identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the student.</param>
        /// <response code="200">Returns the student data.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="404">The student was not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _service.GetByIdAsync(id);
            if (student == null) return NotFound();

            return Ok(student);
        }

        /// <summary>
        /// Gets all students.
        /// </summary>
        /// <response code="200">Returns the list of students.</response>
        /// <response code="401">The user is not authenticated.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<StudentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "TeacherOrAdmin")]
        public async Task<IActionResult> Update(int id, UpdateStudentDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);
            if (!success) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "TeacherOrAdmin")]
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

