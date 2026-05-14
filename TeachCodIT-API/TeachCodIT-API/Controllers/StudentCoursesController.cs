using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachCodIT_API.Models;

namespace TeachCodIT_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentCoursesController : ControllerBase
    {
        private readonly TeachCodItContext _context;

        public StudentCoursesController(TeachCodItContext context)
        {
            _context = context;
        }

        // GET: api/StudentCourses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentCourse>>> GetStudentCourses()
        {
            return await _context.StudentCourses.ToListAsync();
        }

        // GET: api/StudentCourses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StudentCourse>> GetStudentCourse(int id)
        {
            var studentCourse = await _context.StudentCourses.FindAsync(id);

            if (studentCourse == null)
            {
                return NotFound();
            }

            return studentCourse;
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<StudentCourse>>> GetStudentsByCourse(int courseId)
        {
            var students = await _context.StudentCourses
                .Include(sc => sc.Student)
                .Where(sc => sc.CourseId == courseId)
                .ToListAsync();

            return students;
        }

        // PUT: api/StudentCourses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudentCourse(int id, StudentCourse studentCourse)
        {
            if (id != studentCourse.IdStudentCourse)
            {
                return BadRequest();
            }

            _context.Entry(studentCourse).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentCourseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/StudentCourses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<StudentCourse>> PostStudentCourse(StudentCourse studentCourse)
        {
            _context.StudentCourses.Add(studentCourse);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudentCourse", new { id = studentCourse.IdStudentCourse }, studentCourse);
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollDto dto)
        {
            if (dto.StudentId <= 0 || dto.CourseId <= 0)
                return BadRequest("Некорректные идентификаторы");

            // проверка, что студент существует
            var studentExists = await _context.Users.AnyAsync(u => u.IdUser == dto.StudentId && u.Role.NameRole == "Студент");
            if (!studentExists)
                return BadRequest("Студент не найден или это не студент");

            // проверка, что курс существует
            var courseExists = await _context.Courses.AnyAsync(c => c.IdCourse == dto.CourseId);
            if (!courseExists)
                return BadRequest("Курс не найден");

            // уже записан?
            var exists = await _context.StudentCourses
                .AnyAsync(sc => sc.StudentId == dto.StudentId && sc.CourseId == dto.CourseId);

            if (exists)
                return Conflict("Студент уже записан на этот курс");

            var enrollment = new StudentCourse
            {
                StudentId = dto.StudentId,
                CourseId = dto.CourseId,
                EnrolledAt = DateTime.UtcNow,
                ProgressPercent = 0
            };

            _context.StudentCourses.Add(enrollment);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class EnrollDto
        {
            public int StudentId { get; set; }
            public int CourseId { get; set; }
        }

        // DELETE: api/StudentCourses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudentCourse(int id)
        {
            var studentCourse = await _context.StudentCourses.FindAsync(id);
            if (studentCourse == null)
            {
                return NotFound();
            }

            _context.StudentCourses.Remove(studentCourse);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StudentCourseExists(int id)
        {
            return _context.StudentCourses.Any(e => e.IdStudentCourse == id);
        }
    }
}
