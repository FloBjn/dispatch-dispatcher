namespace Dispatch.disapatcher.model;

public interface IFileManager
{
    public void SaveFile(OurFile ourFile);
    public void Encrypte(OurFile ourFile, string content);
    public void SendFile();
}