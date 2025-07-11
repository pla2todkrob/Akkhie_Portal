using Portal.Shared.Models.ViewModel;

namespace Portal.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<EmployeeViewModel?> SearchAsync(Guid employeeId);
        Task<EmployeeViewModel?> SearchByUsernameAsync(string username);
        Task<EmployeeViewModel?> SearchByEmailAsync(string email);
        Task<List<EmployeeViewModel>> GetAsync(int? companyId = null, int? divisionId = null, int? departmentId = null, int? sectionId = null);
    }
}
