using System.Security.Cryptography;
using System.Text;

namespace Dispatch.dispatcher.model;

public class FileHasher
{
    public (string hashFileName, string hashFileContent) HashFile(OurFile file)
    {

        string composite = $"{file.User.Username}_{file.FileName}";
        string hashFileName = ComputeSha3384Hex(Encoding.UTF8.GetBytes(composite));
        string hashFileContent = ComputeSha3384Hex(file.EncryptedContent);

        return (hashFileName, hashFileContent);
    }


    private string ComputeSha3384Hex(byte[] data)
    {
        using var sha384 = SHA3_384.Create();
        byte[] hash = sha384.ComputeHash(data);
        return Convert.ToHexString(hash).Replace("-", "").ToLower();
    }

    public string HashFileName(string fileFileName) => ComputeSha3384Hex(Encoding.UTF8.GetBytes(fileFileName));
}