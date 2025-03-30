using Dispatch.dispatcher.model;

namespace Dispatch.dispatcher.infrastructure;

public class FileCatalogManager : IFileCatalogManager
{

    /// <summary>
    /// Liste des infos des fichiers du catalogue chargés depuis le fichier config JSON.
    /// </summary>
    private readonly Config _config;

    public FileCatalogManager(Config config)
    {
        _config = config;
    }

    public List<StoredFile> LoadCatalog(string connectedUser)
    {
        Console.WriteLine("Lecture du fichier de configuration pour les fichiers de l'utilisateur connecté...");
        try
        {
            // Charger la configuration actuelle
            var config = Config.Load("config.json");

            // Trouver l'utilisateur correspondant
            var userWithFiles = config.Users.Find(u => u.User.Username == connectedUser);
            if (userWithFiles == null)
            {
                throw new Exception($"Utilisateur {connectedUser} non trouvé dans la configuration !");
            }
            
            var userFilesCatalog = userWithFiles.Files;

            Console.WriteLine($"Fichier de configuration lu avec succès pour l'utilisateur {connectedUser} !");
            
            return userFilesCatalog;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erreur lors de la lecture du fichier de configuration : {ex.Message}");
        }
        
    }
}