using System.Net.Http.Headers;
using TeachCodIT.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using TeachCodIT.Services;
using OfficeOpenXml;


var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddDbContext<TeachCodItContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient("TeachCodIT-API", client =>
{
    client.BaseAddress = new Uri("http://teachcodit-api:8080");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Authorization";
        options.AccessDeniedPath = "/Error/AccessDenied";
        options.LogoutPath = "/Account/Logout";

        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.HttpOnly = true;
    });
// Настройка авторизации с политиками ролей
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CustomerPolicy", policy => policy.RequireRole("Пользователь"));
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Админ"));
});


// PasswordHasher
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

// API Services
builder.Services.AddTransient<UserApiService>();
builder.Services.AddTransient<RoleApiService>();
builder.Services.AddTransient<UserProfileApiService>();
builder.Services.AddTransient<UserProgressApiService>();
builder.Services.AddTransient<CourseApiService>();
builder.Services.AddTransient<ModuleApiService>();
builder.Services.AddTransient<LessonApiService>();
builder.Services.AddTransient<LessonTaskApiService>();
builder.Services.AddTransient<TaskOptionApiService>();
builder.Services.AddTransient<UserTaskAttemptApiService>();
builder.Services.AddTransient<StudentCourseApiService>();
builder.Services.AddTransient<AchievementApiService>();
builder.Services.AddTransient<UserAchievementApiService>();
builder.Services.AddTransient<DailyQuestApiService>();
builder.Services.AddTransient<UserDailyQuestApiService>();

builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<GamificationService>();
builder.Services.AddScoped<CodeExecutionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
