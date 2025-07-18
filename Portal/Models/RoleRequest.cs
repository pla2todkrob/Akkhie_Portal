using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.Entities;

namespace Portal.Models
{
    public class RoleRequest : BaseRequest, IRoleRequest
    {
        public RoleRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
            : base(httpClient, apiSettings) { }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(_apiSettings.RoleAll);
            var apiResponse = await HandleResponse<IEnumerable<Role>>(response);
            return apiResponse.Data ?? Enumerable.Empty<Role>();
        }
    }
}