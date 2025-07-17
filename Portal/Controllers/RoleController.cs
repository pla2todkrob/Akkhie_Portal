using Microsoft.AspNetCore.Mvc;
using Portal.Interfaces;
using Portal.Models;
using System.Threading.Tasks;

namespace Portal.Controllers
{
    public class RoleController(IRoleRequest roleRequest) : Controller
    {

        // แสดงหน้าหลักของ Role Management
        public IActionResult Index()
        {
            return View();
        }

        // Action สำหรับดึงข้อมูลไปแสดงใน DataTable (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await roleRequest.GetAllAsync();
            return Json(new { data = roles });
        }

        // Action สำหรับการสร้างและแก้ไข (ส่งข้อมูลผ่าน AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrUpdate(RoleRequest role)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "ข้อมูลไม่ถูกต้อง" });
            }

            bool success;
            if (role.Id == 0)
            {
                // Create
                var result = await roleRequest.CreateAsync(role);
                success = result != null;
            }
            else
            {
                // Update
                success = await roleRequest.UpdateAsync(role.Id, role);
            }

            return Json(new { success, message = success ? "บันทึกข้อมูลสำเร็จ" : "เกิดข้อผิดพลาดในการบันทึก" });
        }

        // Action สำหรับการลบ (ส่งข้อมูลผ่าน AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await roleRequest.DeleteAsync(id);
            return Json(new { success, message = success ? "ลบข้อมูลสำเร็จ" : "เกิดข้อผิดพลาดในการลบ" });
        }
    }
}
