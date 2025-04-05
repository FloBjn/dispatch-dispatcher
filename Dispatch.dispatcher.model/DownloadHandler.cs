using System.Security.Cryptography;

namespace Dispatch.dispatcher.model;

public class DownloadHandler(IFileManager fileManager)
{
    private readonly Protocol _protocol = Protocol.GetInstance();

    public (string, byte[]) HandleDownload(TaskItem taskItem)
    {
        string header;
        // TODO : récupéré les outils de décryptage
        var userWithFiles = fileManager.GetFileByName(taskItem.UserName, taskItem.FileName);

        var file = userWithFiles.Files.Find(f => f.FileName == taskItem.FileName);
        
        var iv = file?.Iv;
        var aad = file?.Aad;
        var tag = file?.Tag;

        // TODO : décrypter le fichier
        var resultLines = taskItem.Result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        if (resultLines.Length < 2)
        {
            return (null, null)!;
        }
        
        var metadata = resultLines[0];
        var encryptedContentHex = resultLines[1].Trim();
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