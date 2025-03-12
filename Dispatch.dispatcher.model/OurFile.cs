using System.Security.Cryptography;
using System.Text;

namespace Dispatch.dispatcher.model;

public class OurFile
{
    private string _fileName;
    
    public string FileName
    {
        get
        {
            return _fileName;
        }
        set
        {
            _fileName = value;
            Aad = Encoding.UTF8.GetBytes(_fileName);
        }
    }
    public int FileSize { get; set; }
    public User User { get; set; }
    public string Content { get; set; }
    public byte[] EncryptedContent { get; set; }
    public byte[] Iv { get; set; } // Signifie Initialisation Vector --> sers au chiffrement AESGCM
    public byte[] Aad { get; set; } // Signifie Additional Authenticated Data --> sers également au chiffrement AESGCM
    public byte[] Tag { get; set; }

    public OurFile()
    {
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(Iv);
        }
    }
}