namespace Dispatch.dispatcher.model;

/// <summary>
/// Analyseur de messages clients reçus par le Dispatcher.
/// Il identifie les commandes `LOGIN` et `REGISTER` envoyées par les clients et effectue les actions appropriées.
/// </summary>
public class ClientMessageAnalyser
{
    private readonly IUserManager _userManager;
    private User? _connectedUser;

    private bool _fileUploadRequest = false;

    private readonly IFileManager _fileManager;
    private OurFile? _currentOurFile;

    /// <summary>
    /// Initialise une nouvelle instance de `ClientMessageAnalyser` avec un gestionnaire d'utilisateurs.
    /// </summary>
    /// <param name="userManager">Instance de `IUserManager` permettant la gestion des utilisateurs.</param>
    /// <param name="fileManager"></param>
    public ClientMessageAnalyser(IUserManager userManager, IFileManager fileManager)
    {
        _userManager = userManager;
        _fileManager = fileManager;
    }

    /// <summary>
    /// Analyse un message client et déclenche l'action correspondante.
    /// Si le message correspond à une commande `LOGIN`, il tente une authentification.
    /// Si le message correspond à `REGISTER`, il enregistre un nouvel utilisateur.
    /// Si le message correspond à 'UPLOAD', il ????????
    /// </summary>
    /// <param name="message">Le message client à analyser.</param>
    public String Analyse(string message)
    {
        try
        {
            //Upload du contenu
            if (_fileUploadRequest)
            {
                try
                {
                    Console.WriteLine("Début de l'upload ...");
                    //Chiffre le contenu du fichier avec AESGCM
                    _fileManager.Encrypte(_currentOurFile, message);
                    //Sauvegarde le fichier sur la config (et est lié a son user) 
                    _fileManager.SaveFile(_currentOurFile, _connectedUser.Username);
                    //Envoie le fichier au container en TCP au bon format
                    _fileManager.SendFile(_currentOurFile);

                    _currentOurFile = null;
                    _fileUploadRequest = false;
                }
                catch (Exception ex)
                {
                    _fileUploadRequest = false;
                    throw new Exception($"Erreur dans l'enregistrement du fichier sur le container:\n{ex.Message}::{ex.StackTrace}");
                }

                return "Upload du fichier sur le container réussi";
            }

            // 🔍 Vérification d'une tentative de connexion
            if (DispatcherProtocol.ParseLogin(message) is (string username, string password))
            {
                try
                {
                    if (_userManager.VerifyUser(username, password) != null)
                    {
                        _connectedUser = _userManager.VerifyUser(username, password);
                        return "LOGIN_OK";
                    }

                    return "LOGIN_ERROR";
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erreur dans la connexion : {ex.Message}");
                }
            }

            // 🔍 Vérification d'une tentative d'inscription
            if (DispatcherProtocol.ParseRegister(message) is (string newUser, string newPassword))
            {
                try
                {
                    // Création d'un nouvel utilisateur
                    _connectedUser = new User
                    {
                        Username = newUser,
                        PasswordHash = newPassword,
                        AesKey = AesKeyGenerator.GenerateAesKey()
                    };

                    // Ajout de l'utilisateur au fichier JSON
                    _userManager.AddUser(_connectedUser);

                    return "Inscription utilisateur réussie";
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erreur dans l'inscription : {ex.Message}");
                }
            }

            // 🔍 Vérification d'une tentative d'upload
            if (DispatcherProtocol.ParseUpload(message) is (string fileName, int size1))
            {
                try
                {
                    _currentOurFile = new OurFile
                    {
                        FileName = fileName,
                        FileSize = size1,
                        User = _connectedUser
                    };

                    _fileUploadRequest = true;

                    return $"Upload du fichier {fileName}";
                }
                catch (Exception ex)
                {
                    throw new Exception($"Problème dans l'upload du fichier: {ex.Message}");
                }
            }

            //Vérification d'une tentative de download
            if (DispatcherProtocol.ParseDownload(message) is (string filename1))
            {
                try
                {
                    //TODO Analyse download
                    return $"Téléchargement du fichier {filename1}";
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erreur durant le download du fichier : : {ex.Message}");
                }
            }

            //Vérification d'une demande de purge
            if (DispatcherProtocol.ParsePurge(message) is string filename2)
            {
                try
                {
                    //TODO Analyse purge
                    return $"Exécution de la purge sur {filename2}";
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erreur durant la purge : {ex.Message}");
                }
            }

            //Vérification d'une demande de catalogue
            if (String.IsNullOrEmpty(DispatcherProtocol.ParseCatalogue(message)))
            {
                try
                {
                    // TODO : récupéré les fichiers du client connecté avec _connectedUser
                    return "Envoie du catalogue";
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erreur durant la récupération du catalogue : {ex.Message}");
                }
            }

            //Vérification d'une demande de post
            if (DispatcherProtocol.ParsePost(message) is (string filename3, int size, string hashFileContent))
            {
                try
                {
                    //TODO Analyse post
                    return $"Post du fichier {filename3}";
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erreur durant le post : {ex.Message}");
                }
            }

            //Vérification d'une demande de delete
            if (DispatcherProtocol.ParseDelete(message) is string hashFileName1)
            {
                try
                {
                    //TODO Analyse delete
                    return $"Suppression du fichier : {hashFileName1}";
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erreur durant la suppression : {ex.Message}");
                }
            }

            //Vérification d'une demande get
            if (DispatcherProtocol.ParseGet(message) is string hashFileName2)
            {
                try
                {
                    //TODO Analyse get
                    return $"Get du fichier {hashFileName2} en cours";
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erreur durant le get : {ex.Message}");
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            return ex.Message;
        } 

        return "";
    }
    
    public TaskItem AnalyseMessage(string message, string user)
    {
        // upload
        if (DispatcherProtocol.ParseUpload(message) is (string fileName, int size1))
        {
            return new TaskItem(TaskItem.TaskType.Send, fileName, "GET", user, new List<string>());
        }
        
        // download
        if (DispatcherProtocol.ParseDownload(message) == "")
        {
            return new TaskItem(TaskItem.TaskType.Receive, "", "GET", user, new List<string>());
        }
        
        // purge
        if (DispatcherProtocol.ParsePurge(message) == "")
        {
            return new TaskItem(TaskItem.TaskType.Delete, "", "DELETE", user, new List<string>());
        }

        throw new NotImplementedException();
    }
}