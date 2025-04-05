using System.Net.Sockets;
using System.Text;

namespace Dispatch.dispatcher.model;

public class UnicastConnection
{
    private TcpClient _client = new();
    private NetworkStream? _stream;
    private bool _isConnected;

    public void Connect(string ip, int port)
    {
        try
        {
            _client.Connect(ip, port);
            _stream = _client.GetStream();
            _isConnected = true;
            Console.WriteLine($"[UnicastConnection] Connecté à {ip}:{port}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"[UnicastConnection] Erreur de connexion : {ex.Message}", ex);
        }
    }

    public string ReadLine()
    {
        if (_stream == null)
            throw new InvalidOperationException("[UnicastConnection] Lecture impossible : flux non initialisé.");

        using var reader = new StreamReader(_stream, Encoding.UTF8, leaveOpen: true);

        string? firstLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(firstLine))
            return string.Empty;

        if (firstLine.StartsWith("GET_OK"))
            return ReadGetOkResponse(firstLine, reader);

        return firstLine;
    }

    private string ReadGetOkResponse(string firstLine, StreamReader reader)
    {
        var parts = firstLine.Split(' ');
        if (parts.Length < 4 || !int.TryParse(parts[2], out int size))
            return firstLine;

        int expectedHexLength = size * 2;
        var hexContentBuilder = new StringBuilder();

        while (hexContentBuilder.Length < expectedHexLength)
        {
            string? line = reader.ReadLine();
            if (line == null) break;
            hexContentBuilder.Append(line.Trim());
        }

        string fullMessage = firstLine + "\r\n" + hexContentBuilder;
        Console.WriteLine($"[UnicastConnection] Réception complète : {fullMessage.Length} caractères");
        return fullMessage;
    }

    public void SendBytes(byte[] data)
    {
        if (_stream == null)
            throw new InvalidOperationException("[UnicastConnection] Envoi impossible : flux non initialisé.");

        Console.WriteLine($"[UnicastConnection] Envoi de {data.Length} octets");
        _stream.Write(data, 0, data.Length);
        _stream.Flush();
    }

    public void Disconnect()
    {
        try
        {
            _stream?.Close();
            _client.Close();
        }
        finally
        {
            _client = new TcpClient();
            _isConnected = false;
            Console.WriteLine("[UnicastConnection] Déconnecté proprement.");
        }
    }
}
