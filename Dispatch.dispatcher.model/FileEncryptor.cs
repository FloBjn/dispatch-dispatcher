using System.Security.Cryptography;
using System.Text;

namespace Dispatch.dispatcher.model;

public class FileEncryptor
{
    public void Encrypte(OurFile ourFile, string content) 
    {
        var aes = Convert.FromBase64String(ourFile.User.AesKey);
        
        (ourFile.Content, ourFile.Tag) = EncryptAesgcm(content, aes, ourFile.Iv, ourFile.Aad);
    }
    
    private (byte[] encryptedText, byte[] tag) EncryptAesgcm(string text, byte[] aesKey, byte[] iv, byte[] aad)
    {
        using (AesGcm aesGcm = new AesGcm(aesKey, 16))
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            byte[] encryptedText = new byte[textBytes.Length];
            byte[] tag = new byte[16]; // 128 bits

            aesGcm.Encrypt(iv, textBytes, encryptedText, tag, aad);
            return (encryptedText, tag);
        }
    }
    
    private string DecryptAesgcm(byte[] encryptedText, byte[] tag, byte[] aesKey, byte[] iv, byte[] aad)
    {
        using (AesGcm aesGcm = new AesGcm(aesKey))
        {
            byte[] decryptedBytes = new byte[encryptedText.Length];

            aesGcm.Decrypt(iv, encryptedText, tag, decryptedBytes, aad);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}