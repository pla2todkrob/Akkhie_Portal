using Microsoft.AspNetCore.Mvc.Rendering;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;

namespace Portal.Interfaces
{
    public interface IDivisionRequest
    {
        Task<IEnumerable<DivisionViewModel>> GetAll();
        Task<DivisionViewModel> GetById(int id);
        Task<ApiResponse> Create(DivisionViewModel viewModel);
        Task<ApiResponse> Update(int id, DivisionViewModel viewModel);
        Task<ApiResponse> Delete(int id);

        // เพิ่มเมธอดนี้เข้าไปสำหรับเรียกข้อมูล Dropdown โดยเฉพาะ
        Task<IEnumerable<SelectListItem>> GetLookupByCompany(int companyId);
    }
}
