namespace Portal.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(string permissionKey);
        Task<HashSet<string>> GetUserPermissionsAsync(Guid userId);
    }
}