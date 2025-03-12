namespace Dispatch.dispatcher.model
{
    /// <summary>
    /// Analyseur de messages clients reçus par le Dispatcher.
    /// Il identifie les commandes `LOGIN` et `REGISTER` envoyées par les clients et effectue les actions appropriées.
    /// </summary>
    public class ClientMessageAnalyser
    {
        private readonly IUserManager _userManager;
        private User _connectedUser;

        /// <summary>
        /// Initialise une nouvelle instance de `ClientMessageAnalyser` avec un gestionnaire d'utilisateurs.
        /// </summary>
        /// <param name="userManager">Instance de `IUserManager` permettant la gestion des utilisateurs.</param>
        public ClientMessageAnalyser(IUserManager userManager)
        {
            _userManager = userManager;
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
            // 🔍 Vérification d'une tentative de connexion
            if (DispatcherProtocol.ParseLogin(message) is (string username, string password))
            {
                Console.WriteLine($"Tentative de connexion : {username}");
                if (_userManager.VerifyUser(username, password) != null)
                {
                    _connectedUser = _userManager.VerifyUser(username, password);
                    return DispatcherProtocol.BuildLoginOk();
                }
                else
                {
                    return DispatcherProtocol.BuildLoginEr();
                }
            }

            // 🔍 Vérification d'une tentative d'inscription
            if (DispatcherProtocol.ParseRegister(message) is (string newUser, string newPassword))
            {
                Console.WriteLine($"Tentative d'enregistrement : {newUser}, Password : {newPassword}");

                // Création d'un nouvel utilisateur
                _connectedUser = new User
                {
                    Username = newUser,
                    PasswordHash = newPassword, 
                    AesKey = AesKeyGenerator.GenerateAesKey()
                };

                // Ajout de l'utilisateur au fichier JSON
                _userManager.AddUser(_connectedUser);
            }
            
            // 🔍 Vérification d'une tentative d'upload
            if (DispatcherProtocol.ParseUpload(message) is (string fileName, int size))
            {
                //TODO récupérer l'utilisateur pour le lier avec le fichier via le fileManager 
                
                Console.WriteLine($"Tentative d'upload du fichier : {fileName}, taille : {size}");

                return "UPLOAD";
            }

            if (String.IsNullOrEmpty(DispatcherProtocol.ParseCatalogue(message)))
            {
                // TODO : récupéré les fichiers du client connecté avec _connectedUser
            }

            return String.Empty;
        }
    }
}
