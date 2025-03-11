using System.Text.Json;
using Dispatch.disapatcher.model;

namespace Dispatch.dispatcher.infrastructure;

public class FileManager : IFileManager
{
    private const string FilePath = "ressources/file.json";
    
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
    }

    public void Encrypte(OurFile ourFile, string content)
    {
        //Cohérent avec SOLID?
        encryptor.Encrypte(ourFile, content);
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