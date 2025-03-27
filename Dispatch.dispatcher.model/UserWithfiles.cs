namespace Dispatch.dispatcher.model;

public class UserWithFiles
{
    public User User { get; set; }
    public List<StoredFile> Files { get; set; } = new List<StoredFile>();
}