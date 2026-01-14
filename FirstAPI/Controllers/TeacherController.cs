using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FirstAPI.Data;
using FirstAPI.Models;

namespace FirstAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TeacherController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.Teachers.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var t = await _db.Teachers.FindAsync(id);
            if (t == null) return NotFound();
            return Ok(t);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Teacher teacher)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _db.Teachers.Add(teacher);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = teacher.Id }, teacher);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Teacher teacher)
        {
            if (id != teacher.Id) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _db.Entry(teacher).State = EntityState.Modified;
            try { await _db.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Teachers.AnyAsync(x => x.Id == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _db.Teachers.FindAsync(id);
            if (t == null) return NotFound();
            _db.Teachers.Remove(t);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}