using System.Text.Json;
using Dispatch.disapatcher.model;

namespace Dispatch.dispatcher.infrastructure;

public class FileManager : IFileManager
{
    private const string FilePath = "ressources/link.json";
    
    private FileEncryptor encryptor;

    public FileManager(FileEncryptor encryptor)
    {
        this.encryptor = encryptor;
    }
    
    /// <summary>
    /// Méthode ayant pour but de sauvegarder un fichier grace à un fichier JSON
    /// </summary>
    /// <param name="user"></param>
    /// <param name="ourFile"></param>
    public void SaveFile(OurFile ourFile)
    {
        File.WriteAllText(FilePath, ToJson(ourFile));
    }

    public void Encrypte(OurFile ourFile)
    {
        //Cohérent avec SOLID?
        //TODO Utiliser le fileEncryptor
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