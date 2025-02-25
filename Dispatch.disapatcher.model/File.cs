namespace Dispatch.disapatcher.model;

public class File
{
    public string FileName { get; set; }
    public int FileSize { get; set; }
    public User User { get; set; }
    public string Content { get; set; }
    public byte[] EncryptedContent { get; set; }
    public byte[] Iv { get; set; }
    public byte[] Aad { get; set; }
    public byte[] Tag { get; set; }
}