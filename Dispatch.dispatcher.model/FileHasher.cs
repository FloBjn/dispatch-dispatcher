using System.Security.Cryptography;
using System.Text;

namespace Dispatch.dispatcher.model;

public class FileHasher
{
    public (string hashFileName, string hashFileContent) HashFile(OurFile file)
    {

        var composite = $"{file.User.Username}_{file.FileName}";
        var hashFileName = ComputeSha384Hex(Encoding.UTF8.GetBytes(composite));
        var hashFileContent = ComputeSha384Hex(file.EncryptedContent);

        return (hashFileName, hashFileContent);
    }


    private string ComputeSha384Hex(byte[] data)
    {
        using var sha384 = SHA384.Create();
        var hash = sha384.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    public string HashFileName(string fileFileName) => ComputeSha384Hex(Encoding.UTF8.GetBytes(fileFileName));
}