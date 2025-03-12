using System.Security.Cryptography;
using System.Text;

namespace Dispatch.dispatcher.model
{
    /// <summary>
    /// Classe utilitaire pour le hachage des mots de passe avec SHA3-384.
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Hache un mot de passe en utilisant SHA3-384 et le retourne sous forme hexadécimale.
        /// </summary>
        /// <param name="password">Le mot de passe en clair.</param>
        /// <returns>Le hachage SHA3-384 du mot de passe sous forme de chaîne hexadécimale.</returns>
        public static string HashPassword(string password)
        {
            using (var sha384 = SHA384.Create()) // SHA3-384 en .NET (remplaçant SHA3)
            {
                byte[] hash = sha384.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hash).Replace("-", "").ToLower(); // Conversion en hexadécimal
            }
        }

        /// <summary>
        /// Vérifie si un mot de passe correspond à un hachage SHA3-384 donné.
        /// </summary>
        /// <param name="password">Le mot de passe en clair.</param>
        /// <param name="hashedPassword">Le hachage SHA3-384 attendu.</param>
        /// <returns>True si le mot de passe correspond au hachage, False sinon.</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
}
