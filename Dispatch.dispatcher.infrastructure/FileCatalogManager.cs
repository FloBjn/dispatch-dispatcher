using Dispatch.dispatcher.model;

namespace Dispatch.dispatcher.infrastructure;

public class FileCatalogManager(Config config) : IFileCatalogManager
{
    public List<StoredFile> LoadCatalog(string connectedUser)
    {
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
            return userFilesCatalog;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erreur lors de la lecture du fichier de configuration : {ex.Message}");
        }
        
    }
}