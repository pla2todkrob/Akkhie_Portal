namespace Portal.Interfaces
{
    public interface IViewPermissionService
    {
        Task<bool> HasPermissionAsync(string permissionKey);
    }
}