using System.Security.Cryptography;
using System.Text;

namespace Dispatch.disapatcher.model;

public class FileEncryptor
{
    public string Encrypte(File file)
    {
        //(byte[] ciphertext, byte[] tag) = EncryptAESGCM(fileContent, aesKey, iv, aad);
        
        //Console.WriteLine("Texte chiffré: " + Convert.ToBase64String(ciphertext));
        //Console.WriteLine("Tag d'authentification: " + Convert.ToBase64String(tag));
        
        Console.WriteLine("UPLOADING TEST :\n" + file.Content);
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