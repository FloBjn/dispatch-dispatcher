using System.Security.Cryptography;

namespace Dispatch.dispatcher.model;

public class FileEncryptor
{
    public void Encrypte(OurFile ourFile, byte[] content)
    {
        var aesKey = Convert.FromBase64String(ourFile.User.AesKey);
        (ourFile.EncryptedContent, ourFile.Tag) = EncryptAesgcm(content, aesKey, ourFile.Iv, ourFile.Aad);
    }

    private (byte[] encryptedText, byte[] tag) EncryptAesgcm(byte[] plaintext, byte[] aesKey, byte[] iv, byte[] aad)
    {
        using var aesGcm = new AesGcm(aesKey);
        
        byte[] encryptedText = new byte[plaintext.Length];
        byte[] tag = new byte[16]; // 128 bits

        aesGcm.Encrypt(iv, plaintext, encryptedText, tag, aad);

        return (encryptedText, tag);
    }

    public byte[] DecryptAesgcm(byte[] encryptedText, byte[] tag, byte[] aesKey, byte[] iv, byte[] aad)
    {
        using var aesGcm = new AesGcm(aesKey);
    
        byte[] decryptedBytes = new byte[encryptedText.Length];
        aesGcm.Decrypt(iv, encryptedText, tag, decryptedBytes, aad);
    
        return decryptedBytes;
    }

}