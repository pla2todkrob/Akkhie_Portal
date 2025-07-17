using Portal.Interfaces;
using Portal.Shared.Models.Entities;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Portal.Models
{
    // คลาสนี้ทำหน้าที่ 2 อย่าง:
    // 1. เป็น Model ที่ใช้ใน View (โดยการสืบทอดคุณสมบัติจาก Role)
    // 2. เป็น Service ที่ใช้เรียก API (โดยการ implement IRoleRequest)
    public class RoleRequest : Role, IRoleRequest
    {
        private readonly HttpClient _httpClient;
        private const string BasePath = "api/Role";

        // Constructor สำหรับ Dependency Injection
        public RoleRequest(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Constructor สำหรับการสร้าง instance ของ Model ใน View
        public RoleRequest() { }


        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            try
            {
                var roles = await _httpClient.GetFromJsonAsync<List<Role>>(BasePath);
                return roles ?? new List<Role>();
            }
            catch (HttpRequestException ex)
            {
                // ควรมี Logger เพื่อบันทึก Exception
                return new List<Role>();
            }
        }

        public async Task<Role> GetByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Role>($"{BasePath}/{id}");
            }
            catch (HttpRequestException ex)
            {
                // ควรมี Logger เพื่อบันทึก Exception
                return null;
            }
        }

        public async Task<Role> CreateAsync(RoleRequest role)
        {
            var response = await _httpClient.PostAsJsonAsync(BasePath, role);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Role>();
            }
            return null;
        }

        public async Task<bool> UpdateAsync(int id, RoleRequest role)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BasePath}/{id}", role);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BasePath}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
