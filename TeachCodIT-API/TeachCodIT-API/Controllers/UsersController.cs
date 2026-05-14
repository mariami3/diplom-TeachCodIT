using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachCodIT_API.Models;
using System.Security.Cryptography;
using System.Text;


namespace TeachCodIT_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TeachCodItContext _context;

        public UsersController(TeachCodItContext context)
        {
            _context = context;
        }

        // GET: api/Users
        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserProfiles)
                .Include(u => u.UserProgress)
                .ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserProfiles)
                .Include(u => u.UserProgress)
                .FirstOrDefaultAsync(u => u.IdUser == id);

            if (user == null)
                return NotFound();

            return user;
        }

        // GET: api/Users/students
        [HttpGet("students")]
        public async Task<ActionResult<IEnumerable<User>>> GetStudents()
        {
            var students = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserProfiles)
                .Include(u => u.UserProgress)
                .Where(u => u.Role.NameRole == "Студент") 
                .ToListAsync();

            return Ok(students);
        }


        // POST: api/Users/register
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(User user)
        {
            if (await _context.Users.AnyAsync(u => u.LoginUser == user.LoginUser))
                return Conflict("Пользователь уже существует");

            if (user.RoleId == null)
            {
                user.RoleId = 3;
            } 

            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(user.PasswordUser));
            user.PasswordUser = Convert.ToBase64String(hash);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // POST: api/Users/authenticate
        [HttpPost("authenticate")]
        public async Task<ActionResult<User>> Authenticate(LoginModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.LoginUser == model.Username);

            if (user == null)
                return Unauthorized();

            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(model.Password));
            var hashedInput = Convert.ToBase64String(hash);

            if (user.PasswordUser != hashedInput)
                return Unauthorized();

            return Ok(user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            if (id != updatedUser.IdUser)
                return BadRequest();

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Обновляем поля
            user.LoginUser = updatedUser.LoginUser;
            user.Email = updatedUser.Email;
            user.RoleId = updatedUser.RoleId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
                return BadRequest("Пользователь с таким email не найден");

            // генерируем токен
            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            return Ok(new { token = user.ResetToken });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ResetToken == model.Token);

            if (user == null)
                return BadRequest("Неверный токен");

            if (user.ResetTokenExpiry < DateTime.UtcNow)
                return BadRequest("Срок действия токена истёк");

            // хешируем новый пароль
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(model.NewPassword));
            user.PasswordUser = Convert.ToBase64String(hash);

            // очищаем токен
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return Ok("Пароль успешно изменён");
        }

        public class ResetPasswordModel
        {
            public string Token { get; set; }
            public string NewPassword { get; set; }
        }

        public class ForgotPasswordModel
        {
            public string Email { get; set; }
        }

        public class LoginModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
