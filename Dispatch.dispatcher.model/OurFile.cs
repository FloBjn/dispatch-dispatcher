using System.Security.Cryptography;
using System.Text;

namespace Dispatch.dispatcher.model;

public class OurFile
{
    private string _fileName;
    private int _fileSize;
    
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
    

    public int FileSize
    {
        get
        {
            return _fileSize;
        }
        set
        {
            _fileSize = value;
            Content = new byte[_fileSize];
        }
    }
    public User User { get; set; }
    public byte[] Content { get; set; }
    public byte[] Iv { get; set; } // Signifie Initialisation Vector --> sers au chiffrement AESGCM
    public byte[] Aad { get; set; } // Signifie Additional Authenticated Data --> sers également au chiffrement AESGCM
    public byte[] Tag { get; set; }
    
    public string HashFileName { get; set; } = "";
    public string HashFileContent { get; set; } = "";
    public int CurrentContentNumber { get; set; }

    public OurFile()
    {
        Tag = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            Iv = new byte[12];
            rng.GetBytes(Iv);
        }
    }
}