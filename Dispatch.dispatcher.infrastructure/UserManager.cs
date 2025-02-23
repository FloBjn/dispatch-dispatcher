using System.Text;
using Dispatch.disapatcher.model;

namespace Dispatch.dispatcher.infrastructure
{
    /// <summary>
    /// Classe responsable de la gestion des utilisateurs.
    /// Permet d'ajouter des utilisateurs et de sauvegarder leurs informations dans un fichier JSON.
    /// </summary>
    public class UserManager : IUserManager
    {
        /// <summary>
        /// Chemin du fichier JSON contenant les utilisateurs.
        /// </summary>
        private const string FilePath = "ressources/users.json";

        /// <summary>
        /// Liste des utilisateurs chargés depuis le fichier JSON.
        /// </summary>
        private List<User> users;

        /// <summary>
        /// Initialise une nouvelle instance de `UserManager` et charge les utilisateurs existants.
        /// </summary>
        public UserManager()
        {
            users = LoadUsers();
        }

        /// <summary>
        /// Charge les utilisateurs depuis le fichier JSON.
        /// </summary>
        /// <returns>Une liste d'objets `User` chargés depuis le fichier JSON.</returns>
        private List<User> LoadUsers()
        {
            if (!File.Exists(FilePath))
                return new List<User>();

            string json = File.ReadAllText(FilePath);
            return ParseJson(json);
        }

        /// <summary>
        /// Ajoute un nouvel utilisateur au fichier JSON s'il n'existe pas déjà.
        /// </summary>
        /// <param name="newUser">L'utilisateur à ajouter.</param>
        public void AddUser(User newUser)
        {
            if (users.Exists(u => u.Username == newUser.Username))
            {
                Console.WriteLine("❌ Utilisateur déjà existant !");
                return;
            }

            newUser.PasswordHash = PasswordHasher.HashPassword(newUser.PasswordHash);
            users.Add(newUser);
            SaveUsers();
            Console.WriteLine($"✅ Utilisateur {newUser.Username} ajouté avec succès !");
        }
        
        public User? VerifyUser(string username, string password)
        {
            User? user = users.Find(u => u.Username == username);
            if (user == null) return null;

            string hashedPassword = PasswordHasher.HashPassword(password);
            if (user.PasswordHash == hashedPassword)
            {
                return user;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Sauvegarde la liste des utilisateurs dans le fichier JSON.
        /// </summary>
        private void SaveUsers()
        {
            string json = ConvertToJson(users);
            File.WriteAllText(FilePath, json);
        }

        /// <summary>
        /// Convertit une chaîne JSON en liste d'objets `User`.
        /// </summary>
        /// <param name="json">Le texte JSON contenant les utilisateurs.</param>
        /// <returns>Une liste d'objets `User`.</returns>
        private List<User> ParseJson(string json)
        {
            List<User> userList = new List<User>();

            if (!json.Contains("\"users\": [")) return userList;

            int start = json.IndexOf("[") + 1;
            int end = json.LastIndexOf("]");
            string usersArray = json.Substring(start, end - start).Trim();

            if (string.IsNullOrWhiteSpace(usersArray)) return userList;

            string[] userEntries = usersArray.Split("},");
            foreach (string entry in userEntries)
            {
                string cleanedEntry = entry.Replace("{", "").Replace("}", "").Trim();
                string[] properties = cleanedEntry.Split(",");

                string username = ExtractJsonValue(properties[0]);
                string passwordHash = ExtractJsonValue(properties[1]);
                string aesKey = ExtractJsonValue(properties[2]);

                userList.Add(new User { Username = username, PasswordHash = passwordHash, AesKey = aesKey });
            }

            return userList;
        }

        /// <summary>
        /// Convertit une liste d'objets `User` en texte JSON.
        /// </summary>
        /// <param name="users">La liste des utilisateurs à convertir.</param>
        /// <returns>Une chaîne JSON formatée contenant la liste des utilisateurs.</returns>
        private string ConvertToJson(List<User> users)
        {
            StringBuilder json = new StringBuilder();
            json.Append("{ \"users\": [\n");

            for (int i = 0; i < users.Count; i++)
            {
                json.Append($"    {{ \"username\": \"{users[i].Username}\", \"password_hash\": \"{users[i].PasswordHash}\", \"aes_key\": \"{users[i].AesKey}\" }}");
                if (i < users.Count - 1) json.Append(",");
                json.Append("\n");
            }

            json.Append("] }");
            return json.ToString();
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
}
