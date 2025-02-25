namespace Dispatch.disapatcher.model;

public class User
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string AesKey { get; set; }
}