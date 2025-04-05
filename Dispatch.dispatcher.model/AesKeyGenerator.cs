using System.Security.Cryptography;

namespace Dispatch.dispatcher.model
{
    /// <summary>
    /// Classe permettant de générer une clé secrète AES de 256 bits.
    /// Cette clé est utilisée pour chiffrer les fichiers des utilisateurs.
    /// </summary>
    public class AesKeyGenerator
    {
        /// <summary>
        /// Génère une clé secrète AES de 256 bits et la retourne au format Base64.
        /// La clé générée est unique et aléatoire à chaque appel.
        /// </summary>
        /// <returns>
        /// Une chaîne Base64 représentant une clé AES de 256 bits.
        /// </returns>
        public static string GenerateAesKey()
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256; // Définit la taille de la clé à 256 bits
                aes.GenerateKey(); // Génère une clé aléatoire
                return Convert.ToBase64String(aes.Key); // Convertit la clé en Base64 pour stockage
            }
        }
    }
}