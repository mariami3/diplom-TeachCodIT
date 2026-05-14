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
    public class UserDailyQuestsController : ControllerBase
    {
        private readonly TeachCodItContext _context;

        public UserDailyQuestsController(TeachCodItContext context)
        {
            _context = context;
        }

        // GET: api/UserDailyQuests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDailyQuest>>> GetUserDailyQuests()
        {
            return await _context.UserDailyQuests.ToListAsync();
        }

        // GET: api/UserDailyQuests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDailyQuest>> GetUserDailyQuest(int id)
        {
            var userDailyQuest = await _context.UserDailyQuests.FindAsync(id);

            if (userDailyQuest == null)
            {
                return NotFound();
            }

            return userDailyQuest;
        }

        // PUT: api/UserDailyQuests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserDailyQuest(int id, UserDailyQuest userDailyQuest)
        {
            if (id != userDailyQuest.IdUserDailyQuest)
            {
                return BadRequest();
            }

            _context.Entry(userDailyQuest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserDailyQuestExists(id))
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

        // POST: api/UserDailyQuests
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserDailyQuest>> PostUserDailyQuest(UserDailyQuest userDailyQuest)
        {
            _context.UserDailyQuests.Add(userDailyQuest);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserDailyQuest", new { id = userDailyQuest.IdUserDailyQuest }, userDailyQuest);
        }

        // DELETE: api/UserDailyQuests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserDailyQuest(int id)
        {
            var userDailyQuest = await _context.UserDailyQuests.FindAsync(id);
            if (userDailyQuest == null)
            {
                return NotFound();
            }

            _context.UserDailyQuests.Remove(userDailyQuest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserDailyQuestExists(int id)
        {
            return _context.UserDailyQuests.Any(e => e.IdUserDailyQuest == id);
        }
    }
}
