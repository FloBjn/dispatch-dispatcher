using System.Security.Cryptography;
using System.Text;

namespace Dispatch.dispatcher.model;

public class FileEncryptor
{
    public string Encrypte(OurFile ourFile/*, User user*/)    //Besoin du User pour sa clé AES
    {
        //TODO Vérifier si cette méthode n'a pas des failles de sécurité
        //TODO Récupérer l'aes key de l'utilisateur 
        var aes = Convert.FromBase64String(AesKeyGenerator.GenerateAesKey());
        
        (ourFile.EncryptedContent, ourFile.Tag) = EncryptAESGCM(ourFile.Content, aes, ourFile.Iv, ourFile.Aad);
        
        return String.Empty;
    }
    
    private (byte[] encryptedText, byte[] tag) EncryptAESGCM(string text, byte[] aesKey, byte[] iv, byte[] aad)
    {
        using (AesGcm aesGcm = new AesGcm(aesKey))
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            byte[] encryptedText = new byte[textBytes.Length];
            byte[] tag = new byte[16]; // 128 bits

            aesGcm.Encrypt(iv, textBytes, encryptedText, tag, aad);
            return (encryptedText, tag);
        }
    }
    
    private string DecryptAESGCM(byte[] encryptedText, byte[] tag, byte[] aesKey, byte[] iv, byte[] aad)
    {
        using (AesGcm aesGcm = new AesGcm(aesKey))
        {
            byte[] decryptedBytes = new byte[encryptedText.Length];

            aesGcm.Decrypt(iv, encryptedText, tag, decryptedBytes, aad);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}