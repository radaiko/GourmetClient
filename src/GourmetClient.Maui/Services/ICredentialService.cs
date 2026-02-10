namespace GourmetClient.Maui.Services;

public interface ICredentialService
{
    Task SaveCredentialsAsync(string key, string username, string password);
    Task<(string username, string password)?> GetCredentialsAsync(string key);
    Task DeleteCredentialsAsync(string key);
}
