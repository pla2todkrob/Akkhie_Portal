using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.Entities;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Portal.Models
{
    public class RoleRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings) : BaseRequest(httpClient, apiSettings), IRoleRequest
    {

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            try
            {
                var roles = await _httpClient.GetFromJsonAsync<List<Role>>(_apiSettings.RoleAll);
                return roles ?? [];
            }
            catch (HttpRequestException)
            {
                // ควรมี Logger เพื่อบันทึก Exception
                return [];
            }
        }
    }
}
