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
            byte[] buffer = new byte[1000];
            int bytesRead = _stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("readLine : " + message);
            Console.WriteLine("Reception de " + bytesRead + " octets");
            return message;
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