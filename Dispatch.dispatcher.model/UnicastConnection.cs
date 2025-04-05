using System.Net.Sockets;
using System.Text;

namespace Dispatch.dispatcher.model
{
    public class UnicastConnection
    {
        private TcpClient _client;
        private NetworkStream? _stream;
        private bool _isConnected;

        public UnicastConnection()
        {
            _client = new TcpClient();
            _isConnected = false;
        }

        public void Connect(string ip, int port)
        {
            _client.Connect(ip, port);
            _stream = _client.GetStream();
            _isConnected = true;
        }

        public string ReadLine()
        {
            using var reader = new StreamReader(_stream, Encoding.UTF8, leaveOpen: true);
    
            // Lire la première ligne (ex : GET_OK nomfichier taille hexadécimal...)
            string firstLine = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(firstLine))
                return "";

            // Cas GET_OK → extraire taille attendue
            if (firstLine.StartsWith("GET_OK"))
            {
                var parts = firstLine.Split(' ');
                if (parts.Length >= 4 && int.TryParse(parts[2], out int size))
                {
                    int expectedHexLength = size * 2; // chaque octet = 2 caractères hex

                    // Lire le contenu hexadécimal (qui arrive après \r\n)
                    StringBuilder hexContentBuilder = new();

                    while (hexContentBuilder.Length < expectedHexLength)
                    {
                        var line = reader.ReadLine();
                        if (line == null) break;
                        hexContentBuilder.Append(line.Trim());
                    }

                    var fullMessage = firstLine + "\r\n" + hexContentBuilder;
                    Console.WriteLine($"Reception totale : {fullMessage.Length} caractères");
                    return fullMessage;
                }
            }

            // Cas normal : juste une ligne simple (ex: POST_OK, DELETE_OK)
            return firstLine;
        }


        public void SendBytes(byte[] data)
        {
            Console.WriteLine("Envoi de " + data.Length + " octets");
            _stream.Write(data, 0, data.Length);
            _stream.Flush();
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client.Close();
            _client = new TcpClient();
            _isConnected = false;
        }
    }
}