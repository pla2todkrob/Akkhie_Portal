namespace Portal.Services.Interfaces
{
    public interface IActiveDirectoryService
    {
        Task<bool> ValidateCredentials(string username, string password);

        Task<Dictionary<string, string>> GetUserProperties(string username);

        Task<Dictionary<string, string>> GetUserProperties(string username, string[] properties);
    }
}
