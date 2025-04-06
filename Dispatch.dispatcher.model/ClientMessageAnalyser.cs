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
    private readonly ContainerManager _containerManager;

    private readonly Protocol _protocol = Protocol.GetInstance();

    public ClientMessageAnalyser(IUserManager userManager, IFileManager fileManager, ContainerManager containerManager)
    {
        _userManager = userManager;
        _fileManager = fileManager;
        _containerManager = containerManager;
    }

    /// <summary>
    /// Analyse un message et retourne un TaskItem si applicable.
    /// </summary>
    public TaskItem? AnalyseMessage(byte[] buffer, int bytesRead, Client activeClient)
    {
        // Étape 1 : Réception du contenu binaire (UPLOAD)
        if (activeClient is { CurrentFile: not null, AwaitingUploadContent: true })
        {
            activeClient.CurrentFile.AppendChunk(buffer, bytesRead);

            if (activeClient.CurrentFile.IsComplete())
            {
                _fileManager.Encrypte(activeClient.CurrentFile, activeClient.CurrentFile.Content);
                var fileHasher = new FileHasher();
                var (hashFileName, hashFileContent) = fileHasher.HashFile(activeClient.CurrentFile);

                activeClient.CurrentFile.HashFileName = hashFileName;
                activeClient.CurrentFile.HashFileContent = hashFileContent;

                string commandHeader = _protocol.BuildPost(
                    hashFileName,
                    activeClient.CurrentFile.EncryptedContent.Length,
                    hashFileContent
                );

                var containers = _containerManager.GetActiveContainers();
                var containerNames = containers.Select(c => c.Name).ToList();

                var task = new TaskItem(
                    TaskItem.TaskType.Send,
                    TaskItem.TaskStatus.InProgress,
                    activeClient.CurrentFile.FileName,
                    commandHeader,
                    activeClient.Username,
                    containerNames
                )
                {
                    BinaryPayload = activeClient.CurrentFile.EncryptedContent,
                    FileSize = activeClient.CurrentFile.EncryptedContent.Length,
                    OurFile = activeClient.CurrentFile
                };

                activeClient.AwaitingUploadContent = false;
                activeClient.CurrentFile = null;

                return task;
            }

            return null;
        }

        // Étape 2 : Si ce n’est pas du contenu de fichier, traiter comme texte
        var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        if (_protocol.ParseUpload(message) is (string fileName, int fileSize))
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

            return null;
        }

        if (_protocol.ParseDownload(message) is string downloadFile)
        {
            var userWithFiles = _fileManager.GetFileByName(activeClient.Username, downloadFile);
            if (userWithFiles == null)
            {
                Console.WriteLine("[ERREUR] Aucun fichier trouvé pour l'utilisateur.");
                throw new Exception("Fichier introuvable");
            }

            var file = userWithFiles.Files.Find(f => f.FileName == downloadFile);
            if (file == null)
            {
                Console.WriteLine("[ERREUR] Fichier demandé non trouvé dans la liste.");
                throw new Exception("Fichier non trouvé");
            }

            if (file == null)
            {
                throw new Exception("File non trouvé");
            }

            var hasher = new FileHasher();
            string hashFileName = hasher.HashFileName(activeClient.Username + "_" + file.FileName);
            
            List<string> containersId = file.Containers;

            string getCommand = _protocol.BuildGet(hashFileName);

            var task = new TaskItem(TaskItem.TaskType.Receive, TaskItem.TaskStatus.InProgress, downloadFile, getCommand,
                activeClient.Username,
                containersId)
            {
                FileSize = file.Size,
            };
            

            return task;
        }

        if (_protocol.ParsePurge(message) is string purgeFile)
        {
            var userWithFiles = _fileManager.GetFileByName(activeClient.Username, purgeFile);
            var file = userWithFiles.Files.Find(f => f.FileName == purgeFile);

            if (file == null)
            {
                throw new Exception("Fichier introuvable ou non autorisé.");
            }

            var hasher = new FileHasher();
            string hashFileName = hasher.HashFileName(activeClient.Username + "_" + purgeFile);

            string deleteCommand = _protocol.BuildDelete(hashFileName);

            return new TaskItem(
                TaskItem.TaskType.Delete,
                TaskItem.TaskStatus.InProgress,
                purgeFile,
                deleteCommand,
                activeClient.Username,
                file.Containers
            );
        }

        throw new NotImplementedException("Commande non reconnue ou non implémentée : " + message);
    }
}