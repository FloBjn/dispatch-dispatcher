using Dispatch.dispatcher.model;

namespace Dispatch.dispatcher.infrastructure;

public class FileNamesCatalogManager : IFileNamesCatalogManager
{
    /// <summary>
    /// Chemin du fichier JSON contenant les infos des fichiers du catalogue.
    /// </summary>
    private const string FilePath = "ressources/file.json";

    /// <summary>
    /// Liste des infos des fichiers du catalogue chargés depuis le fichier JSON.
    /// </summary>
    private List<FilesNamesCatalog> filesNames;

    private string ConnectedUser;

    public FileNamesCatalogManager(string connectedUser)
    {
        filesNames = LoadCatalog();
        ConnectedUser = connectedUser;
    }
    

    private List<FilesNamesCatalog> LoadCatalog()
    {
        if (!File.Exists(FilePath))
        {
            return new List<FilesNamesCatalog>();
        }
        
        string json = File.ReadAllText(FilePath);
        return ParseJson(json);
    }

    private List<FilesNamesCatalog> ParseJson(string json)
    {
        List<FilesNamesCatalog> jsonFilesNames = new List<FilesNamesCatalog>();

        if (!json.Contains("\"files\": [")) return jsonFilesNames;

        int start = json.IndexOf("[") + 1;
        int end = json.LastIndexOf("]");
        string fileArray = json.Substring(start, end - start).Trim();

        if (string.IsNullOrWhiteSpace(fileArray)) return jsonFilesNames;

        string[] fileEntries = fileArray.Split("},");
        foreach (string entry in fileEntries)
        {
            string cleanedEntry = entry.Replace("{", "").Replace("}", "").Trim();
            string[] properties = cleanedEntry.Split(",");

            if (ConnectedUser == properties[2])
            {
                string fileName = ExtractJsonValue(properties[0]);
                int size = int.Parse(ExtractJsonValue(properties[1]));
                
                jsonFilesNames.Add(new FilesNamesCatalog { FileName = fileName, Size = size });
            }
        }
        
        return jsonFilesNames; 
    }
        
        /// <summary>
        /// Extrait la valeur d'un champ JSON.
        /// </summary>
        /// <param name="property">Une propriété JSON sous forme de chaîne.</param>
        /// <returns>La valeur extraite, nettoyée des guillemets.</returns>
        private string ExtractJsonValue(string property)
        {
            return property.Split(":")[1].Trim().Replace("\"", "");
        }
}