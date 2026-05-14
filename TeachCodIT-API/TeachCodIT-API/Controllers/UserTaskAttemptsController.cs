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
    public class UserTaskAttemptsController : ControllerBase
    {
        private readonly TeachCodItContext _context;

        public UserTaskAttemptsController(TeachCodItContext context)
        {
            _context = context;
        }

        // GET: api/UserTaskAttempts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserTaskAttempt>>> GetUserTaskAttempts()
        {
            return await _context.UserTaskAttempts.ToListAsync();
        }

        // GET: api/UserTaskAttempts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserTaskAttempt>> GetUserTaskAttempt(int id)
        {
            var userTaskAttempt = await _context.UserTaskAttempts.FindAsync(id);

            if (userTaskAttempt == null)
            {
                return NotFound();
            }

            return userTaskAttempt;
        }

        // GET: api/UserTaskAttempts/lesson/5
        [HttpGet("lesson/{lessonId}")]
        public async Task<ActionResult<IEnumerable<UserTaskAttempt>>> GetAttemptsByLesson(int lessonId)
        {
            var attempts = await _context.UserTaskAttempts
                .Where(a => a.LessonTask.LessonId == lessonId)
                .Include(a => a.LessonTask)
                .Include(a => a.User)
                .ToListAsync();

            return attempts;
        }

        // PUT: api/UserTaskAttempts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserTaskAttempt(int id, UserTaskAttempt userTaskAttempt)
        {
            if (id != userTaskAttempt.IdAttempt)
            {
                return BadRequest();
            }

            _context.Entry(userTaskAttempt).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserTaskAttemptExists(id))
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

        // POST: api/UserTaskAttempts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserTaskAttempt>> PostUserTaskAttempt(UserTaskAttempt userTaskAttempt)
        {
            _context.UserTaskAttempts.Add(userTaskAttempt);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserTaskAttempt", new { id = userTaskAttempt.IdAttempt }, userTaskAttempt);
        }

        // DELETE: api/UserTaskAttempts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserTaskAttempt(int id)
        {
            var userTaskAttempt = await _context.UserTaskAttempts.FindAsync(id);
            if (userTaskAttempt == null)
            {
                return NotFound();
            }

            _context.UserTaskAttempts.Remove(userTaskAttempt);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserTaskAttemptExists(int id)
        {
            return _context.UserTaskAttempts.Any(e => e.IdAttempt == id);
        }
    }
}
