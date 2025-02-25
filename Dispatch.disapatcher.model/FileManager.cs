namespace Dispatch.disapatcher.model;

public class FileManager
{
    private FileEncryptor encryptor;

    public FileManager(FileEncryptor encryptor)
    {
        this.encryptor = encryptor;
    }
    
    public void SendFile(){}
}