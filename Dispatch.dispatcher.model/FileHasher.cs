using System.Security.Cryptography;
using System.Text;

namespace Dispatch.dispatcher.model;

public class FileHasher
{
    public (string hashFileName, string hashFileContent) HashFile(OurFile file)
    {
        Console.WriteLine("Hashage du fichier en cours ...");

        string hashFileName = ComputeSha384Hex(Encoding.UTF8.GetBytes(file.FileName));
        string hashFileContent = ComputeSha384Hex(file.EncryptedContent);

        Console.WriteLine($"hashFilename = {hashFileName}");
        Console.WriteLine($"hashFileContent = {hashFileContent}");

        return (hashFileName, hashFileContent);
    }

    private string ComputeSha384Hex(byte[] data)
    {
        using var sha384 = SHA384.Create();
        byte[] hash = sha384.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    public string HashFileName(string fileFileName) => ComputeSha384Hex(Encoding.UTF8.GetBytes(fileFileName));
}