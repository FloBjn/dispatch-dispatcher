namespace Dispatch.dispatcher.model;

public class UserWithFiles
{
    public User User { get; set; }
    public List<string> Files { get; set; } = new List<string>();
}