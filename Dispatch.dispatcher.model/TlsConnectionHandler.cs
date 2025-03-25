using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Dispatch.dispatcher.model;

public class TlsConnectionHandler
{
    private readonly TcpClient _client;
    private readonly X509Certificate2 _serverCertificate;
    private readonly Func<string, Task<string>>? _onMessageReceived;

    public TlsConnectionHandler(
        TcpClient client,
        X509Certificate2 serverCertificate,
        Func<string, Task<string>>? onMessageReceived)
    {
        _client = client;
        _serverCertificate = serverCertificate;
        _onMessageReceived = onMessageReceived;
    }

    public async Task HandleConnectionAsync()
    {
        using NetworkStream networkStream = _client.GetStream();
        using SslStream sslStream = new SslStream(networkStream, false);

        try
        {
            var options = new SslServerAuthenticationOptions
            {
                ServerCertificate = _serverCertificate
            };

            await sslStream.AuthenticateAsServerAsync(options);

            Console.WriteLine($"Connexion sécurisée établie avec TLS : {sslStream.SslProtocol}");

            await CommunicateAsync(sslStream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur TLS : {ex.Message}");
        }
    }

    private async Task CommunicateAsync(SslStream sslStream)
    {
        byte[] buffer = new byte[1024];

        while (true)
        {
            int bytesRead;

            try
            {
                bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Erreur de lecture TLS : {ex.Message}");
                break;
            }

            if (bytesRead == 0)
            {
                Console.WriteLine("Client a fermé la connexion.");
                break;
            }

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Message reçu (sécurisé) : {message}");

            string response;

            if (_onMessageReceived != null)
            {
                response = await _onMessageReceived.Invoke(message);
            }
            else
            {
                Console.WriteLine("Aucun gestionnaire de message défini.");
                response = string.Empty;
            }

            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            await sslStream.WriteAsync(responseBytes, 0, responseBytes.Length);
            await sslStream.FlushAsync();
        }
    }

}