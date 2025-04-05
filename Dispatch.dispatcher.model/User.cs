namespace Dispatch.dispatcher.model;

public class User(string username, string passwordHash, string aesKey)
{
    public string Username { get; set; } = username;
    public string PasswordHash { get; set; } = passwordHash;
    public string AesKey { get; set; } = aesKey;
}