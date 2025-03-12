using System.Security.Cryptography;
using System.Text;

namespace Dispatch.disapatcher.model;

public class FileHasher
{
    public (string name, string content) HashFile(OurFile file)
    {
        Console.WriteLine("Hashage du fichier en cours ...");
        string name = file.FileName;
        string content;

        var byteContent = file.Content;
        var byteName = Encoding.UTF8.GetBytes(name);

        using (var sha384 = SHA384.Create())
        {
            content = BitConverter.ToString(byteContent).Replace("-", "").ToLower();
            name = BitConverter.ToString(byteName).Replace("-", "").ToLower();
        }

        return (name, content);
    }
}