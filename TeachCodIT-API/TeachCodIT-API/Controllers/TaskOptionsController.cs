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
    public class TaskOptionsController : ControllerBase
    {
        private readonly TeachCodItContext _context;

        public TaskOptionsController(TeachCodItContext context)
        {
            _context = context;
        }

        // GET: api/TaskOptions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskOption>>> GetTaskOptions()
        {
            return await _context.TaskOptions.ToListAsync();
        }

        // GET: api/TaskOptions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskOption>> GetTaskOption(int id)
        {
            var taskOption = await _context.TaskOptions.FindAsync(id);

            if (taskOption == null)
            {
                return NotFound();
            }

            return taskOption;
        }

        // GET: api/TaskOptions/task/4
        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<IEnumerable<TaskOption>>> GetOptionsByTask(int taskId)
        {
            var options = await _context.TaskOptions
                .Where(o => o.LessonTaskId == taskId)
                .ToListAsync();

            return options;
        }

        // PUT: api/TaskOptions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTaskOption(int id, TaskOption taskOption)
        {
            if (id != taskOption.IdOption)
            {
                return BadRequest();
            }

            _context.Entry(taskOption).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskOptionExists(id))
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

        // POST: api/TaskOptions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TaskOption>> PostTaskOption(TaskOption taskOption)
        {
            _context.TaskOptions.Add(taskOption);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTaskOption", new { id = taskOption.IdOption }, taskOption);
        }

        // DELETE: api/TaskOptions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskOption(int id)
        {
            var taskOption = await _context.TaskOptions.FindAsync(id);
            if (taskOption == null)
            {
                return NotFound();
            }

            _context.TaskOptions.Remove(taskOption);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaskOptionExists(int id)
        {
            return _context.TaskOptions.Any(e => e.IdOption == id);
        }
    }
}
