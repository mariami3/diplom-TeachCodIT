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
    public class DailyQuestsController : ControllerBase
    {
        private readonly TeachCodItContext _context;

        public DailyQuestsController(TeachCodItContext context)
        {
            _context = context;
        }

        // GET: api/DailyQuests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DailyQuest>>> GetDailyQuests()
        {
            return await _context.DailyQuests.ToListAsync();
        }

        // GET: api/DailyQuests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DailyQuest>> GetDailyQuest(int id)
        {
            var dailyQuest = await _context.DailyQuests.FindAsync(id);

            if (dailyQuest == null)
            {
                return NotFound();
            }

            return dailyQuest;
        }

        // PUT: api/DailyQuests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDailyQuest(int id, DailyQuest dailyQuest)
        {
            if (id != dailyQuest.IdQuest)
            {
                return BadRequest();
            }

            _context.Entry(dailyQuest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DailyQuestExists(id))
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

        // POST: api/DailyQuests
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DailyQuest>> PostDailyQuest(DailyQuest dailyQuest)
        {
            _context.DailyQuests.Add(dailyQuest);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDailyQuest", new { id = dailyQuest.IdQuest }, dailyQuest);
        }

        // DELETE: api/DailyQuests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDailyQuest(int id)
        {
            var dailyQuest = await _context.DailyQuests.FindAsync(id);
            if (dailyQuest == null)
            {
                return NotFound();
            }

            _context.DailyQuests.Remove(dailyQuest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DailyQuestExists(int id)
        {
            return _context.DailyQuests.Any(e => e.IdQuest == id);
        }
    }
}
