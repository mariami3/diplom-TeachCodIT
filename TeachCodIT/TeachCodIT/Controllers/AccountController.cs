using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Net.Http.Json;
using TeachCodIT.Models;
using TeachCodIT.Services;
using TeachCodIT.Models.DTOs;

namespace TeachCodIT.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly EmailService _emailService;

        public AccountController(IHttpClientFactory httpClientFactory, EmailService emailService)
        {
            _httpClient = httpClientFactory.CreateClient("TeachCodIT-API");
            _emailService = emailService;
        }


        [HttpGet]
        public IActionResult Authorization()
        {
            //if (User.Identity != null && User.Identity.IsAuthenticated)
           // {
            //    return RedirectToAction("Index", "Home");
            //}

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Authorization(string loginUser, string passwordUser)
        {
            if (string.IsNullOrWhiteSpace(loginUser) || string.IsNullOrWhiteSpace(passwordUser))
            {
                ModelState.AddModelError("", "Введите логин и пароль");
                return View();
            }

            var response = await _httpClient.PostAsJsonAsync(
                "api/Users/authenticate",
                new { Username = loginUser, Password = passwordUser });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Неверный логин или пароль");
                return View();
            }

            var user = await response.Content.ReadFromJsonAsync<User>();

            if (user == null)
            {
                ModelState.AddModelError("", "Ошибка получения пользователя");
                return View();
            }

            // Получаем роль
            var roleResponse = await _httpClient.GetAsync($"api/Roles/{user.RoleId}");

            string roleName = "Студент";

            if (roleResponse.IsSuccessStatusCode)
            {
                var role = await roleResponse.Content.ReadFromJsonAsync<Role>();
                if (role != null)
                    roleName = role.NameRole;
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.LoginUser),
        new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
        new Claim(ClaimTypes.Role, roleName)
    };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            if (user.RoleId == 3)
                return RedirectToAction("Home", "Student");

            if (user.RoleId == 1)
                return RedirectToAction("Index", "Admin");

            if (user.RoleId == 2)
                return RedirectToAction("Dashboard", "Teacher");

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user, string confirmPassword)
        {
            if (user.PasswordUser != confirmPassword)
                ModelState.AddModelError("", "Пароли не совпадают");

            if (string.IsNullOrWhiteSpace(user.PasswordUser) ||
                user.PasswordUser.Length < 6 ||
                !user.PasswordUser.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                ModelState.AddModelError("", "Пароль должен быть минимум 6 символов и содержать спецсимвол");
            }

            if (string.IsNullOrWhiteSpace(user.Email) || !IsEmailValid(user.Email))
                ModelState.AddModelError("", "Некорректный email");

            if (!ModelState.IsValid)
                return View(user);

            var response = await _httpClient.PostAsJsonAsync("api/Users/register", user);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Authorization", "Account");

            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]   
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Authorization", "Account");
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Введите email");
                return View();
            }

            // вызываем API — он создаёт ResetToken и сохраняет в БД
            var response = await _httpClient.PostAsJsonAsync(
                "api/Users/forgot-password",
                new { Email = email });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                return View();
            }

            var apiResult = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
            string token = apiResult.Token;

            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { token = token },
                Request.Scheme);

            string subject = "Восстановление пароля TeachCodIT";

            string body = $@"
Здравствуйте!

Вы запросили восстановление пароля.

Перейдите по ссылке:
{resetLink}

Если это были не вы — проигнорируйте письмо.
";

            await _emailService.SendEmailAsync(email, subject, body);

            TempData["SuccessMessage"] = "Ссылка отправлена на почту";
            return RedirectToAction("Authorization");
        }


        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest("Неверный токен");

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string passwordUser, string confirmPassword)
        {
            if (passwordUser != confirmPassword)
            {
                ModelState.AddModelError("", "Пароли не совпадают");
                ViewBag.Token = token;
                return View();
            }

            var response = await _httpClient.PostAsJsonAsync(
                "api/Users/reset-password",
                new { Token = token, NewPassword = passwordUser });

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Пароль изменён";
                return RedirectToAction("Authorization");
            }

            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            ViewBag.Token = token;
            return View();
        }


        private bool IsEmailValid(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                var entry = Dns.GetHostEntry(addr.Host);

                return entry.AddressList.Any(ip =>
                    ip.AddressFamily == AddressFamily.InterNetwork ||
                    ip.AddressFamily == AddressFamily.InterNetworkV6);
            }
            catch
            {
                return false;
            }
        }
    }
}