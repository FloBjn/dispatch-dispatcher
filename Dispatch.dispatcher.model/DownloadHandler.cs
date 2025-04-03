using System.Security.Cryptography;

namespace Dispatch.dispatcher.model;

public class DownloadHandler(IFileManager fileManager, IUserManager userManager)
{
    private readonly Protocol _protocol = Protocol.GetInstance();
    private readonly IFileManager _fileManager = fileManager;
    private readonly IUserManager _userManager = userManager;

    public string HandleDownload(TaskItem taskItem)
    {
        string response;
        // TODO : récupéré les outils de décryptage
        var userWithFiles = _fileManager.GetFileByName(taskItem.UserName, taskItem.FileName);
        var iv = userWithFiles.Files.Find(f => f.FileName == taskItem.FileName)?.Iv;
        var aad = userWithFiles.Files.Find(f => f.FileName == taskItem.FileName)?.Aad;
        var tag = userWithFiles.Files.Find(f => f.FileName == taskItem.FileName)?.Tag;

        // TODO : décrypter le fichier
        var resultLines = taskItem.Result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        if (resultLines.Length < 2)
        {
            return _protocol.BuildDownloadError();
        }
        
        var metadata = resultLines[0];
        var encryptedContentHex = resultLines[1];

        // Convertir le contenu chiffré de l'hexadécimal au binaire
        byte[] encryptedContent;
        try
        {
            encryptedContent = Convert.FromHexString(encryptedContentHex);
        }
        catch (FormatException)
        {
            return _protocol.BuildDownloadError();
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
            return _protocol.BuildDownloadError();
        }       

        // TODO : construire le download_ok
        response = _protocol.BuildDownloadOk(taskItem.FileName, taskItem.FileSize, decryptedContent);
        return response;
    }
}