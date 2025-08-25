// File: Portal.Services/Configuration/WasteRulesServiceRegistration.cs
using Microsoft.Extensions.DependencyInjection;
using Portal.Services.Services.Waste;

namespace Portal.Services.Configuration
{
    /// <summary>
    /// ส่วนขยายสำหรับลงทะเบียนบริการของฟีเจอร์ Waste Rules
    /// ใช้งานใน Program.cs ของโปรเจกต์ Portal.Services ด้วย:
    ///    builder.Services.AddWasteRulesFeature();
    /// </summary>
    public static class WasteRulesServiceRegistration
    {
        /// <summary>
        /// ลงทะเบียน DI สำหรับบริการประเมินสูตร/สคีมา Waste
        /// </summary>
        public static IServiceCollection AddWasteRulesFeature(this IServiceCollection services)
        {
            services.AddScoped<WasteRuleEvaluatorService>();
            return services;
        }
    }
}
