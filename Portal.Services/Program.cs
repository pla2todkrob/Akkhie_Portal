using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Portal.Services.Interfaces;
using Portal.Services.Models;
using Serilog;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog configuration
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

ConfigureJwtSettings(builder);
ConfigureActiveDirectorySettings(builder);

builder.Services.AddDbContext<PortalDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<ActiveDirectorySettings>(builder.Configuration.GetSection(ActiveDirectorySettings.SectionName));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IDivisionService, DivisionService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
builder.Services.AddScoped<IITInventoryService, ITInventoryService>();
builder.Services.AddScoped<ISupportCategoryService, SupportCategoryService>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Portal API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PortalDbContext>();
    if (db.Database.GetPendingMigrations().Any())
    {
        db.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

void ConfigureJwtSettings(WebApplicationBuilder webApplicationBuilder)
{
    var jwtKey = webApplicationBuilder.Configuration["Jwt:Key"];
    if (string.IsNullOrEmpty(jwtKey) || jwtKey.Contains("YOUR_SECRET_KEY_HERE"))
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        jwtKey = Convert.ToBase64String(randomNumber);
        webApplicationBuilder.Configuration["Jwt:Key"] = jwtKey;
        Console.WriteLine("Generated new JWT Key for this session.");
    }
    else
    {
        Console.WriteLine("Using JWT Key from configuration.");
    }

    if (string.IsNullOrEmpty(webApplicationBuilder.Configuration["Jwt:Issuer"]))
    {
        webApplicationBuilder.Configuration["Jwt:Issuer"] = "https://localhost:7024";
    }
    if (string.IsNullOrEmpty(webApplicationBuilder.Configuration["Jwt:Audience"]))
    {
        webApplicationBuilder.Configuration["Jwt:Audience"] = "https://localhost:7138";
    }
    if (string.IsNullOrEmpty(webApplicationBuilder.Configuration["Jwt:DurationMinutes"]) ||
        !int.TryParse(webApplicationBuilder.Configuration["Jwt:DurationMinutes"], out _))
    {
        webApplicationBuilder.Configuration["Jwt:DurationMinutes"] = "60";
    }
}

void ConfigureActiveDirectorySettings(WebApplicationBuilder webApplicationBuilder)
{
    if (string.IsNullOrEmpty(webApplicationBuilder.Configuration["ActiveDirectory:BindUser"]))
    {
        webApplicationBuilder.Configuration["ActiveDirectory:BindUser"] = "central_all";
    }
    if (string.IsNullOrEmpty(webApplicationBuilder.Configuration["ActiveDirectory:BindPassword"]))
    {
        webApplicationBuilder.Configuration["ActiveDirectory:BindPassword"] = "123456";
    }
}
