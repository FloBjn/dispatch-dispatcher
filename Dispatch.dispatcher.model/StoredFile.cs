namespace Dispatch.dispatcher.model;

public class StoredFile
{
    public string FileName { get; set; } 
    public List<string> Containers { get; set; } = new List<string>();
}