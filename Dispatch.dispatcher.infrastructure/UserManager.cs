using System.Text;
using Dispatch.disapatcher.model;

namespace Dispatch.dispatcher.infrastructure
{
    /// <summary>
    /// Classe responsable de la gestion des utilisateurs.
    /// Permet de gérer les utilisateurs stockés dans le fichier de configuration.
    /// </summary>
    public class UserManager : IUserManager
    {
        private readonly Config _config;

        /// <summary>
        /// Initialise une nouvelle instance de `UserManager` et charge la configuration.
        /// </summary>
        public UserManager(Config config)
        {
            _config = config;
        }

        /// <summary>
        /// Vérifie si un utilisateur existe et si son mot de passe est correct.
        /// </summary>
        public User? VerifyUser(string username, string password)
        {
            var userWithFiles = _config.Users.Find(u => u.User.Username == username);
            if (userWithFiles == null) return null;

            string hashedPassword = PasswordHasher.HashPassword(password);
            return userWithFiles.User.PasswordHash == hashedPassword ? userWithFiles.User : null;
        }

        /// <summary>
        /// Ajoute un nouvel utilisateur au fichier de configuration s'il n'existe pas déjà.
        /// </summary>
        public void AddUser(User newUser)
        {
            if (_config.Users.Exists(u => u.User.Username == newUser.Username))
            {
                Console.WriteLine("Utilisateur déjà existant !");
                return;
            }

            newUser.PasswordHash = PasswordHasher.HashPassword(newUser.PasswordHash);
            _config.Users.Add(new UserWithFiles { User = newUser, Files = new List<string>() });

            _config.Save("config.json"); // Sauvegarde la config mise à jour
            Console.WriteLine($"Utilisateur {newUser.Username} ajouté avec succès !");
        }
    }
}