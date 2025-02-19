namespace Dispatch.disapatcher.model
{
    /// <summary>
    /// Analyseur de messages clients reçus par le Dispatcher.
    /// Il identifie les commandes `LOGIN` et `REGISTER` envoyées par les clients et effectue les actions appropriées.
    /// </summary>
    public class ClientMessageAnalyser
    {
        private readonly IUserManager _userManager;

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
        /// </summary>
        /// <param name="message">Le message client à analyser.</param>
        public void Analyse(string message)
        {
            // 🔍 Vérification d'une tentative de connexion
            if (DispatcherProtocol.ParseLogin(message) is (string username, string password))
            {
                Console.WriteLine($"🔑 Tentative de connexion : {username}, Password : {password}");
                // 🚧 TODO : Ajouter l'authentification de l'utilisateur
            }

            // 🔍 Vérification d'une tentative d'inscription
            if (DispatcherProtocol.ParseRegister(message) is (string newUser, string newPassword))
            {
                Console.WriteLine($"🆕 Tentative d'enregistrement : {newUser}, Password : {newPassword}");

                // Création d'un nouvel utilisateur
                User user = new User
                {
                    Username = newUser,
                    PasswordHash = newPassword, // 🚧 TODO : Hacher le mot de passe avec SHA3-384
                    AesKey = AesKeyGenerator.GenerateAesKey()
                };

                // Ajout de l'utilisateur au fichier JSON
                _userManager.AddUser(user);
            }
        }
    }
}
