using Microsoft.AspNetCore.Authentication.Cookies;
using Portal.Interfaces;
using Portal.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ===== 1. Configure Services (การลงทะเบียน Service ต่างๆ) =====

// ตั้งค่า Serilog สำหรับการบันทึก Log
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

// เพิ่ม Service ที่จำเป็นสำหรับ MVC
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// ตั้งค่าการอ่าน ApiSettings จาก appsettings.json
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));

// ลงทะเบียน Authentication Header Handler
// เพื่อให้ทุก Request ที่ยิงไปหา API มี Token ติดไปด้วยโดยอัตโนมัติหลังจาก Login
builder.Services.AddTransient<AuthenticationHeaderHandler>();

// ลงทะเบียน HttpClient และ Service Requests ทั้งหมด
// พร้อมทั้งผูก Authentication Header Handler เข้าไป
builder.Services.AddHttpClient<ICompanyRequest, CompanyRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IDepartmentRequest, DepartmentRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IDivisionRequest, DivisionRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IEmployeeRequest, EmployeeRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IRoleRequest, RoleRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<ISectionRequest, SectionRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<ISupportTicketRequest, SupportTicketRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IITInventoryRequest, ITInventoryRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<ISupportCategoryRequest, SupportCategoryRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();

// ตั้งค่า Authentication ด้วย Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // ตั้งค่า Cookie หมดอายุใน 60 นาที
        options.SlidingExpiration = true; // ต่ออายุ Cookie อัตโนมัติถ้ามีการใช้งาน
    });

builder.Services.AddAuthorization();


// ===== 2. Configure HTTP Request Pipeline (การตั้งค่าลำดับการทำงาน) =====

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
