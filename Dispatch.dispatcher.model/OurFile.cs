using System.Security.Cryptography;
using System.Text;

namespace Dispatch.dispatcher.model;

public class OurFile
{
    private readonly MemoryStream _stream = new();  
    private readonly string _fileName;

    public string FileName
    {
        get => _fileName;
        init
        {
            _fileName = value;
            Aad = Encoding.UTF8.GetBytes(_fileName);
        }
    }
    

    public int FileSize { get; set; }

    public User User { get; set; }
    
    public byte[] Content => _stream.ToArray();
    
    public byte[] Iv { get; set; } // Signifie Initialisation Vector --> sers au chiffrement AESGCM
    public byte[] Aad { get; set; } // Signifie Additional Authenticated Data --> sers également au chiffrement AESGCM
    public byte[] Tag { get; set; }
    
    public string HashFileName { get; set; } = "";
    public string HashFileContent { get; set; } = "";
    public int CurrentContentNumber => (int)Math.Ceiling((double)_stream.Length / 1024);


    public OurFile()
    {
        Tag = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            Iv = new byte[12];
            rng.GetBytes(Iv);
        }
    }
    public void AppendChunk(byte[] data, int length)
    {
        _stream.Write(data, 0, length);
    }
    
    public bool IsComplete()
    {
        return _stream.Length >= FileSize;
    }
    public byte[] EncryptedContent { get; set; } = Array.Empty<byte>();
}