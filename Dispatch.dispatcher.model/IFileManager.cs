namespace Dispatch.dispatcher.model;

public interface IFileManager
{
    public void SaveFile(TaskItem task);
    public void Encrypte(OurFile ourFile, byte[] content); 
}