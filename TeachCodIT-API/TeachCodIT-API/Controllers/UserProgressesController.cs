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
    public class UserProgressesController : ControllerBase
    {
        private readonly TeachCodItContext _context;

        public UserProgressesController(TeachCodItContext context)
        {
            _context = context;
        }

        // GET: api/UserProgresses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserProgress>>> GetUserProgresses()
        {
            return await _context.UserProgresses.ToListAsync();
        }

        // GET: api/UserProgresses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserProgress>> GetUserProgress(int id)
        {
            var userProgress = await _context.UserProgresses.FindAsync(id);

            if (userProgress == null)
            {
                return NotFound();
            }

            return userProgress;
        }

        // PUT: api/UserProgresses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserProgress(int id, UserProgress userProgress)
        {
            if (id != userProgress.UserId)
            {
                return BadRequest();
            }

            _context.Entry(userProgress).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserProgressExists(id))
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

        // POST: api/UserProgresses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserProgress>> PostUserProgress(UserProgress userProgress)
        {
            _context.UserProgresses.Add(userProgress);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserProgressExists(userProgress.UserId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetUserProgress", new { id = userProgress.UserId }, userProgress);
        }

        // DELETE: api/UserProgresses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserProgress(int id)
        {
            var userProgress = await _context.UserProgresses.FindAsync(id);
            if (userProgress == null)
            {
                return NotFound();
            }

            _context.UserProgresses.Remove(userProgress);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserProgressExists(int id)
        {
            return _context.UserProgresses.Any(e => e.UserId == id);
        }
    }
}
