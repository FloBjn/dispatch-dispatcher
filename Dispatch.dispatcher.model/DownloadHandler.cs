using System.Security.Cryptography;

namespace Dispatch.dispatcher.model;

public class DownloadHandler(IFileManager fileManager)
{
    private readonly Protocol _protocol = Protocol.GetInstance();
    private readonly IFileManager _fileManager = fileManager;

    public (string, byte[]) HandleDownload(TaskItem taskItem)
    {
        Console.WriteLine("DownloadHandler.HandleDownload");
        byte[] response;
        string header;
        // TODO : récupéré les outils de décryptage
        var userWithFiles = _fileManager.GetFileByName(taskItem.UserName, taskItem.FileName);
        var iv = userWithFiles.Files.Find(f => f.FileName == taskItem.FileName)?.Iv;
        var aad = userWithFiles.Files.Find(f => f.FileName == taskItem.FileName)?.Aad;
        var tag = userWithFiles.Files.Find(f => f.FileName == taskItem.FileName)?.Tag;

        // TODO : décrypter le fichier
        var resultLines = taskItem.Result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        if (resultLines.Length < 2)
        {
            return (null, null)!;
        }
        
        var metadata = resultLines[0];
        var encryptedContentHex = resultLines[1].Trim();
        Console.WriteLine(encryptedContentHex);
        // Convertir le contenu chiffré de l'hexadécimal au binaire
        byte[] encryptedContent;
        try
        {
            encryptedContent = Convert.FromHexString(encryptedContentHex);
        }
        catch (FormatException)
        {
            return (null, null)!;
        }
        
        var fileEncryptor = new FileEncryptor();
        byte[] aesKey = Convert.FromBase64String(userWithFiles.User.AesKey);
        byte[] decryptedContent;
        try
        {
            decryptedContent = fileEncryptor.DecryptAesgcm(encryptedContent, tag, aesKey, iv, aad);
        }
        catch (CryptographicException)
        {
            return (null, null)!;
        }       

        // TODO : construire le download_ok
        header = _protocol.BuildDownloadOk(taskItem.FileName, taskItem.FileSize, decryptedContent);
        return (header, decryptedContent);
    }
}