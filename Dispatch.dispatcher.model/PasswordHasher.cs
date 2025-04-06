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
            using (var sha3384 = SHA3_384.Create()) 
            {
                byte[] hash = sha3384.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToHexString(hash).Replace("-", "").ToLower(); // Conversion en hexadécimal
            }
        }
    }
}
