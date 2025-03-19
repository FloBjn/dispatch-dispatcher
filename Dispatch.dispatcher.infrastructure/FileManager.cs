using System.Text;
using System.Text.Json;
using Dispatch.disapatcher.model;
using Dispatch.dispatcher.model;

namespace Dispatch.dispatcher.infrastructure;

public class FileManager : IFileManager
{
    private FileEncryptor _encryptor;
    private FileHasher _hasher;
    private TcpContainer _tcpContainer;

    public FileManager(FileEncryptor encryptor, TcpContainer tcpContainer, FileHasher hasher)
    {
        _encryptor = encryptor;
        _hasher = hasher;
        _tcpContainer = tcpContainer;
    }
    
    /// <summary>
    /// Méthode ayant pour but de sauvegarder un fichier grace à un fichier JSON
    /// </summary>
    /// <param name="user"></param>
    /// <param name="ourFile"></param>
    public void SaveFile(OurFile ourFile, string username)
    {
        Console.WriteLine("Mise à jour du fichier de configuration avec le fichier uploadé...");

        try
        {
            // Charger la configuration actuelle
            var config = Config.Load("config.json");

            // Trouver l'utilisateur correspondant
            var userWithFiles = config.Users.Find(u => u.User.Username == username);
            if (userWithFiles == null)
            {
                throw new Exception($"Utilisateur {username} non trouvé dans la configuration !");
            }
            
            // Générer le hash du nom du fichier (SHA3-384)
            var (hashFileName, hashFileContent) = _hasher.HashFile(ourFile); 

            // Déterminer le container de stockage 
            string container = "container_1"; // À remplacer par la logique réelle

            // Ajouter le fichier à la liste de l'utilisateur
            userWithFiles.Files.Add(new StoredFile
            {
                HashFileName = hashFileName,
                Containers = new List<string> { container }
            });

            // Sauvegarder la configuration mise à jour
            config.Save("config.json");

            Console.WriteLine($"Fichier {ourFile.FileName} ajouté pour l'utilisateur {username} !");
        }
        catch (Exception ex)
        {
            throw new Exception($"Erreur lors de la mise à jour du fichier de configuration : {ex.Message}");
        }
    }


    public void Encrypte(OurFile ourFile, string content)
    {
        //TODO Cohérent avec SOLID?
        Console.WriteLine("Cryptage... ");
        _encryptor.Encrypte(ourFile, content);
    }

    public void SendFile(OurFile ourFile)
    {
        Console.WriteLine("Envoie du fichier sur le container ...");
        var hashedFile = _hasher.HashFile(ourFile);
        var content = Encoding.UTF8.GetString(ourFile.Content);
        
        string message = $"POST {hashedFile.name} {ourFile.FileSize} {hashedFile.content}\n{content}";
        _tcpContainer.SendMessageToFirst(message);
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