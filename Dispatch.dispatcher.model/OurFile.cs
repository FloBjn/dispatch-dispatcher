using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace Dispatch.dispatcher.model;

public class OurFile
{
    private string _fileName;
    private int _fileSize;

    public string FileName
    {
        get => _fileName;
        set
        {
            _fileName = value;
            Aad = Encoding.UTF8.GetBytes(_fileName); // Pour AES-GCM
        }
    }

    public int FileSize
    {
        get => _fileSize;
        set => _fileSize = value;
    }

    public User User { get; set; }

    // Nouveau : buffer dynamique pour accumulation
    private readonly MemoryStream _stream = new();

    public byte[] Content => _stream.ToArray();
    
    public byte[] EncryptedContent { get; set; } = Array.Empty<byte>();


    public byte[] Iv { get; private set; }
    public byte[] Aad { get; private set; }
    public byte[] Tag { get; set; } = new byte[16];

    public string HashFileName { get; set; } = "";
    public string HashFileContent { get; set; } = "";

    public int CurrentContentNumber => (int)Math.Ceiling((double)_stream.Length / 1024);

    public OurFile()
    {
        using var rng = RandomNumberGenerator.Create();
        Iv = new byte[12];
        rng.GetBytes(Iv);
    }

    /// <summary>
    /// Ajoute un morceau binaire reçu au contenu du fichier.
    /// </summary>
    public void AppendChunk(byte[] data, int length)
    {
        _stream.Write(data, 0, length);
    }

    /// <summary>
    /// Indique si le fichier est entièrement reçu.
    /// </summary>
    public bool IsComplete()
    {
        return _stream.Length >= FileSize;
    }
}