namespace Portal.Shared.Models.Entities
{
    public class EmployeePermission
    {
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int PermissionId { get; set; }
        public Permission Permission { get; set; }
    }
}
