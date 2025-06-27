// Portal as ASP.NET Core Web Application
// Program.cs for the Portal project

using Microsoft.AspNetCore.Authentication.Cookies;
using Portal.Interfaces;
using Portal.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .MinimumLevel.Warning()
        .WriteTo.File(
            path: Path.Combine("Logs", "log-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7
        );
});

builder.Services.AddControllersWithViews();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));
builder.Services.AddScoped<ICompanyRequest, CompanyRequest>();
builder.Services.AddScoped<IDepartmentRequest, DepartmentRequest>();
builder.Services.AddScoped<IDivisionRequest, DivisionRequest>();
builder.Services.AddScoped<IEmployeeRequest, EmployeeRequest>();
builder.Services.AddScoped<IRoleRequest, RoleRequest>();
builder.Services.AddScoped<ISectionRequest, SectionRequest>();
builder.Services.AddHttpClient();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
