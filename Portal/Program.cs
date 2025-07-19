using Microsoft.AspNetCore.Authentication.Cookies;
using Portal.Interfaces;
using Portal.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ===== 1. Configure Services (���ŧ����¹ Service ��ҧ�) =====

// ��駤�� Serilog ����Ѻ��úѹ�֡ Log
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

// ���� Service ����������Ѻ MVC
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// ��駤�ҡ����ҹ ApiSettings �ҡ appsettings.json
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));

// ŧ����¹ Authentication Header Handler
// �������ء Request ����ԧ��� API �� Token �Դ仴������ѵ��ѵ���ѧ�ҡ Login
builder.Services.AddTransient<AuthenticationHeaderHandler>();

// ŧ����¹ HttpClient ��� Service Requests ������
// �������駼١ Authentication Header Handler ����
builder.Services.AddHttpClient<ICompanyRequest, CompanyRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IDepartmentRequest, DepartmentRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IDivisionRequest, DivisionRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IEmployeeRequest, EmployeeRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IRoleRequest, RoleRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<ISectionRequest, SectionRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<ISupportTicketRequest, SupportTicketRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<IITInventoryRequest, ITInventoryRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient<ISupportCategoryRequest, SupportCategoryRequest>().AddHttpMessageHandler<AuthenticationHeaderHandler>();

// ��駤�� Authentication ���� Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // ��駤�� Cookie �������� 60 �ҷ�
        options.SlidingExpiration = true; // ������� Cookie �ѵ��ѵԶ���ա����ҹ
    });

builder.Services.AddAuthorization();


// ===== 2. Configure HTTP Request Pipeline (��õ�駤���ӴѺ��÷ӧҹ) =====

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
