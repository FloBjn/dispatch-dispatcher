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

    public ClientMessageAnalyser(IUserManager userManager, IFileManager fileManager)
    {
        _userManager = userManager;
        _fileManager = fileManager;
    }

    /// <summary>
    /// Analyse un message et retourne un TaskItem si applicable.
    /// </summary>
    public TaskItem? AnalyseMessage(string message, Client activeClient)
    {
        // Étape 1 : Si le client attend un contenu de fichier, on traite ce contenu
        if (activeClient is { CurrentFile: not null, AwaitingUploadContent: true } )
        {            
            Console.WriteLine("📥 Contenu de fichier reçu, traitement en cours...");
            
            int numberContentMessages = activeClient.CurrentFile.FileSize / 1024;
            _fileManager.Encrypte(activeClient.CurrentFile, message);

            var fileHasher = new FileHasher();
            var (hashFileName, hashFileContent) = fileHasher.HashFile(activeClient.CurrentFile);
            activeClient.CurrentFile.HashFileName = hashFileName;
            activeClient.CurrentFile.HashFileContent = hashFileContent;

            string command = DispatcherProtocol.BuildPost(
                activeClient.CurrentFile.HashFileName,
                activeClient.CurrentFile.HashFileContent.Length,
                activeClient.CurrentFile.HashFileContent
            ) + "\r\n" + activeClient.CurrentFile.Content;

            var task = new TaskItem(
                TaskItem.TaskType.Send,
                activeClient.CurrentFile.FileName,
                command,
                activeClient.Username,
                new List<string> { "C1" } // TODO: Ajouter les containers approprié
            );

            if (activeClient.CurrentFile.CurrentContentNumber == numberContentMessages)
            {
                activeClient.AwaitingUploadContent = false;
                activeClient.CurrentFile = null;
                return task;
            }

            return null;
        }

        // Étape 2 : Détection d'une requête d'upload (en 2 temps)
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

        // Étape 3 : Download
        if (DispatcherProtocol.ParseDownload(message) is string downloadFile)
        {
            return new TaskItem(TaskItem.TaskType.Receive, downloadFile, "GET", activeClient.Username, new List<string>());
        }

        // Étape 4 : Purge
        if (DispatcherProtocol.ParsePurge(message) is string purgeFile)
        {
            return new TaskItem(TaskItem.TaskType.Delete, purgeFile, "DELETE", activeClient.Username, new List<string>());
        }

        throw new NotImplementedException("Commande non reconnue ou non implémentée : " + message);
    }
}