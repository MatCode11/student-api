using Microsoft.AspNetCore.Mvc;
using FirstAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace FirstAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private static List<Student> students = new List<Student>
        {
            new Student { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" },
            new Student { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com" }
            
        };

        // GET: api/student
        [HttpGet]
        public ActionResult<IEnumerable<Student>> GetAllStudents()
        {
            return Ok(students);
        }

        // [HttpGet]
        // public ActionResult<List<Student>> GetStudents()
        // {
        //     return Ok(students);
        // }

        // GET: api/student/5
        // [HttpGet("{id}")]
        // public ActionResult<Student> GetStudentById(int id)
        // {
        //     var student = students.FirstOrDefault(s => s.Id == id);
        //     if (student == null)
        //     {
        //         return NotFound(new { message = "Student not found" });
        //     }
        //     return Ok(student);
        // }
    }
}
