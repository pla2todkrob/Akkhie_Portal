namespace Portal.Interfaces
{
    public interface IPermissionRequest
    {
        Task<HashSet<string>?> GetMyPermissionsAsync();
    }
}