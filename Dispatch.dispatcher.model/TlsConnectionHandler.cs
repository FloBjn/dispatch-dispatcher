using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Dispatch.dispatcher.model;

public class TlsConnectionHandler
{
    private readonly TcpClient _client;
    private readonly X509Certificate2 _serverCertificate;
    private readonly ClientMessageAnalyser _clientMessageAnalyser;

    public TlsConnectionHandler(TcpClient client, X509Certificate2 serverCertificate, ClientMessageAnalyser clientMessageAnalyser)
    {
        _client = client;
        _serverCertificate = serverCertificate;
        _clientMessageAnalyser = clientMessageAnalyser;
    }

    public async Task HandleConnectionAsync()
    {
        using NetworkStream networkStream = _client.GetStream();
        using SslStream sslStream = new SslStream(networkStream, false);

        try
        {
            var options = new SslServerAuthenticationOptions
            {
                ServerCertificate = _serverCertificate,
                // EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls13
            };

            await sslStream.AuthenticateAsServerAsync(options);
        
            Console.WriteLine($"Connexion sécurisée établie avec TLS : {sslStream.SslProtocol}");
            Console.WriteLine($"Version TLS utilisée : {sslStream.SslProtocol}");

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
        int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine($"Message reçu (sécurisé) : {message}");

        string response = _clientMessageAnalyser.Analyse(message);
        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        await sslStream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }
}