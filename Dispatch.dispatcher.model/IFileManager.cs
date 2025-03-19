namespace Dispatch.dispatcher.model;

public interface IFileManager
{
    public void SaveFile(OurFile ourFile, string username);
    public void Encrypte(OurFile ourFile, string content);
    public void SendFile(OurFile ourFile);
}