using System.Text.Json;
using Dispatch.dispatcher.model;

namespace Dispatch.dispatcher.infrastructure;

public class FileManager(FileEncryptor encryptor) : IFileManager
{
    private FileEncryptor _encryptor = encryptor;

    /// <summary>
    /// Méthode ayant pour but de sauvegarder un fichier grace à un fichier JSON
    /// </summary>
    /// <param name="user"></param>
    /// <param name="ourFile"></param>
    public void SaveFile(TaskItem task)
    {
        try
        {
            var config = Config.Load("config.json");

            var userWithFiles = config.Users.Find(u => u.User.Username == task.UserName);
            if (userWithFiles == null)
            {
                throw new Exception($"Utilisateur {task.UserName} non trouvé dans la configuration !");
            }

            userWithFiles.Files.Add(new StoredFile
            {
                FileName = task.FileName, 
                Size = task.FileSize,
                Iv = task.OurFile.Iv,
                Aad = task.OurFile.Aad,
                Tag = task.OurFile.Tag,
                Containers = task.ContainerId.ToList()
            });

            config.Save("config.json");
        }
        catch (Exception ex)
        {
            throw new Exception($"Erreur dans SaveFile(TaskItem) : {ex.Message}");
        }
    }



    public void Encrypte(OurFile ourFile, byte[] content)
    {
        _encryptor.Encrypte(ourFile, content);
    }

    public UserWithFiles GetFileByName(string username, string downloadFile)
    {
        var config = Config.Load("config.json");

        // Trouver l'utilisateur correspondant au nom d'utilisateur fourni
        var userWithFiles = config.Users.Find(u => u.User.Username == username);

        if (userWithFiles == null)
        {
            throw new Exception($"Utilisateur {username} non trouvé dans la configuration !");
        }

        // Rechercher le fichier parmi les fichiers de cet utilisateur
        var file = userWithFiles.Files.Find(f => f.FileName == downloadFile);

        if (file == null)
        {
            throw new Exception($"Fichier {downloadFile} non trouvé pour l'utilisateur {username}.");
        }

        return userWithFiles;
    }

    public void DeleteFile(TaskItem task)
    {
        try
        {
            var config = Config.Load("config.json");

            var userWithFiles = config.Users.Find(u => u.User.Username == task.UserName);
            if (userWithFiles == null)
            {
                throw new Exception($"Utilisateur {task.UserName} non trouvé dans la configuration !");
            }

            var fileToRemove = userWithFiles.Files.FirstOrDefault(f => f.FileName == task.FileName);
            if (fileToRemove == null)
            {
                throw new Exception($"Fichier {task.FileName} introuvable pour l'utilisateur {task.UserName}.");
            }

            userWithFiles.Files.Remove(fileToRemove);

            config.Save("config.json");
        }
        catch (Exception ex)
        {
            throw new Exception($"Erreur dans DeleteFile(TaskItem) : {ex.Message}");
        }
    }
}