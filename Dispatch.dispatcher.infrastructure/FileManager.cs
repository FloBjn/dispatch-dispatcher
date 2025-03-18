using System.Text;
using System.Text.Json;
using Dispatch.disapatcher.model;
using Dispatch.dispatcher.model;

namespace Dispatch.dispatcher.infrastructure;

public class FileManager : IFileManager
{
    private const string FilePath = "ressources/file.json";
    
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
    public void SaveFile(OurFile ourFile)
    {
        Console.WriteLine("Sauvegarde du fichier sur le dispatcher ...");
        string temp;
        
        try
        {
            //Copie des fichiers déja existants 
            temp = File.ReadAllText(FilePath) + "\n" + ToJson(ourFile);
        }
        catch (Exception ex)
        {
            temp = ToJson(ourFile);
        }
        //Écriture du fichier 
        File.WriteAllText(FilePath, temp);
        
        Console.WriteLine("Fichier " + ourFile.FileName + " uploadé!");
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