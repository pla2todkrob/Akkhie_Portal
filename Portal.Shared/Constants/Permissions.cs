namespace Portal.Shared.Constants
{
    public static class Permissions
    {
        public static class Management
        {
            public const string Access = "Permissions.Management.Access";
        }

        public static class PermissionsManagement // สิทธิ์สำหรับจัดการหน้าสิทธิ์เอง
        {
            public const string View = "Permissions.Permissions.View";
            public const string Create = "Permissions.Permissions.Create";
            public const string Edit = "Permissions.Permissions.Edit";
            public const string Delete = "Permissions.Permissions.Delete";
        }

        // เพิ่มกลุ่มสิทธิ์อื่นๆ ตามเมนู เช่น
        // public static class Company { ... }
        // public static class Employee { ... }
    }
}