using Portal.Shared.Models.DTOs.Auth;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;

namespace Portal.Interfaces
{
    public interface IEmployeeRequest
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);

        Task<ApiResponse<RegisterResponse>> RegisterAsync(RegisterRequest request);

        Task<ApiResponse<EmployeeViewModel>> SearchAsync(Guid employeeId);

        Task<ApiResponse<EmployeeViewModel>> SearchByUsernameAsync(string username);

        Task<ApiResponse<EmployeeViewModel>> SearchByEmailAsync(string email);

        Task<ApiResponse<IEnumerable<EmployeeViewModel>>> AllAsync();
    }
}
