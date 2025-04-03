namespace Dispatch.dispatcher.model;

public class StoredFile
{
    public string FileName { get; set; } 
    
    public int Size { get; set; }
    
    public List<string> Containers { get; set; } = new List<string>();
    public byte[] Iv { get; set; }
    public byte[] Aad { get; set; }
    public byte[] Tag { get; set; }
}