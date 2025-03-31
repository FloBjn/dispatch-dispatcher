using System.Text;

namespace Dispatch.dispatcher.model;

/// <summary>
/// Analyseur de messages clients reçus par le Dispatcher.
/// Il identifie les commandes `LOGIN`, `REGISTER`, `UPLOAD`, etc. et effectue les actions appropriées.
/// </summary>
public class ClientMessageAnalyser
{
    private readonly IUserManager _userManager;
    private readonly IFileManager _fileManager;
    private readonly IFileCatalogManager _fileCatalogManager;

    public ClientMessageAnalyser(IUserManager userManager, IFileManager fileManager, IFileCatalogManager fileCatalogManager)
    {
        _userManager = userManager;
        _fileManager = fileManager;
        _fileCatalogManager = fileCatalogManager;
    }

    /// <summary>
    /// Analyse un message et retourne un TaskItem si applicable.
    /// </summary>
    public TaskItem? AnalyseMessage(byte[] buffer, int bytesRead, Client activeClient)
{
    // Étape 1 : Réception du contenu binaire (UPLOAD)
    if (activeClient is { CurrentFile: not null, AwaitingUploadContent: true })
    {
        Console.WriteLine("Contenu de fichier reçu, traitement en cours...");

        activeClient.CurrentFile.AppendChunk(buffer, bytesRead);

        if (activeClient.CurrentFile.IsComplete())
        {
            var fileHasher = new FileHasher();
            var (hashFileName, hashFileContent) = fileHasher.HashFile(activeClient.CurrentFile);

            activeClient.CurrentFile.HashFileName = hashFileName;
            activeClient.CurrentFile.HashFileContent = hashFileContent;

            _fileManager.Encrypte(activeClient.CurrentFile, activeClient.CurrentFile.Content);

            string commandHeader = DispatcherProtocol.BuildPost(
                hashFileName,
                activeClient.CurrentFile.EncryptedContent.Length,
                hashFileContent
            );


            var task = new TaskItem(
                TaskItem.TaskType.Send,
                activeClient.CurrentFile.FileName,
                commandHeader,
                activeClient.Username,
                new List<string> { "C1" },
                new List<StoredFile>()
            ){
                BinaryPayload = activeClient.CurrentFile.EncryptedContent
            };

            activeClient.AwaitingUploadContent = false;
            activeClient.CurrentFile = null;

            return task;
        }

        return null;
    }

    // Étape 2 : Si ce n’est pas du contenu de fichier, traiter comme texte
    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

    if (DispatcherProtocol.ParseUpload(message) is (string fileName, int fileSize))
    {
        var fullUser = _userManager.GetUserByUsername(activeClient.Username);
        if (fullUser == null)
            throw new Exception("Utilisateur inconnu");

        activeClient.CurrentFile = new OurFile
        {
            FileName = fileName,
            FileSize = fileSize,
            User = fullUser
        };

        activeClient.AwaitingUploadContent = true;

        Console.WriteLine($"📝 Requête d'UPLOAD reçue pour {fileName} ({fileSize} octets). En attente de contenu...");
        return null;
    }

    if (DispatcherProtocol.ParseDownload(message) is string downloadFile)
    {
        return new TaskItem(TaskItem.TaskType.Receive, downloadFile, "GET", activeClient.Username, new List<string>(), new List<StoredFile>());
    }

    if (DispatcherProtocol.ParsePurge(message) is string purgeFile)
    {
        return new TaskItem(TaskItem.TaskType.Delete, purgeFile, "DELETE", activeClient.Username, new List<string>(), new List<StoredFile>());
    }

    if (DispatcherProtocol.ParseCatalogue(message) != null)
    {
        var storedFiles = _fileCatalogManager.LoadCatalog(activeClient.Username);
        return new TaskItem(TaskItem.TaskType.Catalog, "", "INVENTORY", activeClient.Username, new List<string>(), storedFiles);
    }

    throw new NotImplementedException("Commande non reconnue ou non implémentée : " + message);
}

}