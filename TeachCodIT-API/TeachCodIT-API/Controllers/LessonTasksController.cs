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
    public class LessonTasksController : ControllerBase
    {
        private readonly TeachCodItContext _context;

        public LessonTasksController(TeachCodItContext context)
        {
            _context = context;
        }

        // GET: api/LessonTasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LessonTask>>> GetLessonTasks()
        {
            return await _context.LessonTasks.ToListAsync();
        }

        // GET: api/LessonTasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LessonTask>> GetLessonTask(int id)
        {
            var lessonTask = await _context.LessonTasks.FindAsync(id);

            if (lessonTask == null)
            {
                return NotFound();
            }

            return lessonTask;
        }

        // GET: api/LessonTasks/lesson/4
        [HttpGet("lesson/{lessonId}")]
        public async Task<ActionResult<IEnumerable<LessonTask>>> GetTasksByLesson(int lessonId)
        {
            return await _context.LessonTasks
                .Where(t => t.LessonId == lessonId)
                .Include(t => t.TaskOptions) 
                .ToListAsync();
        }

        // PUT: api/LessonTasks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLessonTask(int id, LessonTask lessonTask)
        {
            if (id != lessonTask.IdLessonTask)
            {
                return BadRequest();
            }

            _context.Entry(lessonTask).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LessonTaskExists(id))
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

        // POST: api/LessonTasks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LessonTask>> PostLessonTask(LessonTask lessonTask)
        {
            _context.LessonTasks.Add(lessonTask);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLessonTask", new { id = lessonTask.IdLessonTask }, lessonTask);
        }

        // DELETE: api/LessonTasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLessonTask(int id)
        {
            var lessonTask = await _context.LessonTasks.FindAsync(id);
            if (lessonTask == null)
            {
                return NotFound();
            }

            _context.LessonTasks.Remove(lessonTask);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LessonTaskExists(int id)
        {
            return _context.LessonTasks.Any(e => e.IdLessonTask == id);
        }
    }
}
