using System.Text;
using System.Text.Json;
using Dispatch.disapatcher.model;
using Dispatch.dispatcher.model;

namespace Dispatch.dispatcher.infrastructure;

public class FileManager : IFileManager
{
    private FileEncryptor _encryptor;
    private FileHasher _hasher;
    private ContainerManager _containerManager;

    public FileManager(FileEncryptor encryptor, TcpContainer tcpContainer, FileHasher hasher, ContainerManager containerManager)
    {
        _encryptor = encryptor;
        _hasher = hasher;
        _containerManager = containerManager;
    }
    
    /// <summary>
    /// Méthode ayant pour but de sauvegarder un fichier grace à un fichier JSON
    /// </summary>
    /// <param name="user"></param>
    /// <param name="ourFile"></param>
    public void SaveFile(TaskItem task)
    {
        Console.WriteLine("💾 Mise à jour du fichier de configuration avec la tâche...");

        try
        {
            var config = Config.Load("config.json");

            var userWithFiles = config.Users.Find(u => u.User.Username == task.UserName);
            if (userWithFiles == null)
            {
                throw new Exception($"Utilisateur {task.UserName} non trouvé dans la configuration !");
            }

            // Ajout du fichier avec les Containers spécifiés dans la task
            userWithFiles.Files.Add(new StoredFile
            {
                FileName = task.FileName, // Ici tu dois passer le hash, pas le nom original
                Containers = task.ContainerId.ToList()
            });

            config.Save("config.json");

            Console.WriteLine($"✅ Tâche enregistrée pour {task.UserName} dans config.json.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Erreur dans SaveFile(TaskItem) : {ex.Message}");
        }
    }



    public void Encrypte(OurFile ourFile, string content)
    {
        //TODO Cohérent avec SOLID?
        Console.WriteLine("Cryptage... ");
        _encryptor.Encrypte(ourFile, content);
    }
    /*
    public void SendFile(OurFile ourFile)
    {
        Console.WriteLine("📡 Envoi du fichier à tous les Containers actifs...");

        var activeContainers = _containerManager.GetActiveContainers();
        if (activeContainers.Count == 0)
        {
            throw new Exception("🚨 Aucun Container actif trouvé !");
        }

        var hashedFile = _hasher.HashFile(ourFile);
        var content = Encoding.UTF8.GetString(ourFile.Content);

        string message = $"POST {hashedFile.hashFileName} {ourFile.FileSize} {hashedFile.hashFileContent}\n{content}";

        _tcpContainer.SendMessageToAllAsync(message).Wait(); // Bloque jusqu'à la fin de l'exécution

        Console.WriteLine("✅ Fichier envoyé à tous les Containers !");
    }
    */


    /// <summary>
    /// Méthode permettant de formater un fichier en JSON
    /// </summary>
    /// <param name="ourFile"></param>
    /// <returns></returns>
    private string ToJson(OurFile ourFile)
    {
        return JsonSerializer.Serialize(ourFile, new JsonSerializerOptions
        {
            WriteIndented = true,
        });
    }
}