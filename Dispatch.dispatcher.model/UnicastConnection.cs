using System.Net.Sockets;
using System.Text;

namespace Dispatch.dispatcher.model
{
    public class UnicastConnection
    {
        private TcpClient _client;
        private NetworkStream? _stream;
        private bool _isConnected;

        private string? _ip;
        private int? _port;

        public UnicastConnection()
        {
            _client = new TcpClient();
            _isConnected = false;
        }

        public void Connect(string ip, int port)
        {
            Console.WriteLine($"Connexion TCP vers {ip}:{port}");

            _ip = ip;
            _port = port;

            _client.Connect(ip, port);
            _stream = _client.GetStream();
            _isConnected = true;

            Console.WriteLine($"Connexion TCP réussie");
        }

        public string ReadLine()
        {
            Console.WriteLine($"Lecture du message en provenance de {_ip}:{_port}");
            byte[] buffer = new byte[1000];
            int bytesRead = _stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Message reçu: '{message}'");
            return message;
        }

        public void SendMessage(string message)
        {
            Console.WriteLine($"Envoi du message [{message}] vers l'IP {_ip}:{_port}");
            byte[] data = Encoding.UTF8.GetBytes(message);
            _stream.Write(data, 0, data.Length);
            _stream.Flush();
        }

        public void SendBytes(byte[] data)
        {
            Console.WriteLine($"Envoi de {data.Length} octets vers {_ip}:{_port}");
            _stream.Write(data, 0, data.Length);
            _stream.Flush();
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client.Close();
            _client = new TcpClient();
            _isConnected = false;
            Console.WriteLine("Déconnexion TCP");
        }
    }
}