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

    public UserWithFiles GetFileByName(string downloadFile)
    {
        var config = Config.Load("config.json");
        
        // Parcourir tous les user pour voir si ils ont dowloadFile
        var file = config.Users.Find(files => files.Files.Any(f => f.FileName == downloadFile));

        if (file == null)
        {
            throw new Exception("Impossible de trouver le fichier");
        }

        return file;
    }


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