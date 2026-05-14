using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using TeachCodIT.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace TeachCodIT.Controllers
{
    public class BaseController : Controller
    {
        protected readonly TeachCodItContext _context;

        public BaseController(TeachCodItContext context)
        {
            _context = context;
        }

        protected async Task SetUserSettingsAsync()
        {
            if (!User.Identity.IsAuthenticated) return;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return;

            int userId = int.Parse(userIdClaim.Value);

            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            //  Theme
            ViewBag.UserTheme = settings?.Theme ?? "light";

            // Language
            var language = settings?.Language ?? "ru";
            ViewBag.UserLanguage = language;

            var culture = new CultureInfo(language);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            // TimeZone
            var timeZoneId = settings?.TimeZone ?? "UTC";
            ViewBag.UserTimeZone = timeZoneId;

            try
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                ViewBag.UserLocalTime =
                    TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            }
            catch
            {
                ViewBag.UserLocalTime = DateTime.UtcNow;
            }
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            await SetUserSettingsAsync();
            await next();
        }
    }
}