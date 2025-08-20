using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));

builder.Services.AddTransient<AuthenticationHeaderHandler>();

builder.Services.AddHttpClient<ICompanyRequest, CompanyRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IDepartmentRequest, DepartmentRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IDivisionRequest, DivisionRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IEmployeeRequest, EmployeeRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IRoleRequest, RoleRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<ISectionRequest, SectionRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<ISupportTicketRequest, SupportTicketRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IITInventoryRequest, ITInventoryRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<ISupportCategoryRequest, SupportCategoryRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IPermissionRequest, PermissionRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();

builder.Services.AddScoped<IViewPermissionService, ViewPermissionService>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(3);
        options.SlidingExpiration = true;
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanAccessManagement", policy =>
        policy.Requirements.Add(new PermissionRequirement("Permissions.Management.Access")));
});

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
