using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Dispatch.dispatcher.model;

public class TcpServer
{
    private readonly Config _config;
    private readonly X509Certificate2 _serverCertificate;
    private TcpListener _tcpListener;
    private bool _isRunning;
    public event Func<string, Task<string>>? OnMessageReceived; 
    

    public TcpServer(Config config)
    {
        _config = config;
        _serverCertificate = new X509Certificate2(config.CertificatePath, config.CertificatePassword);
    }

    public async Task StartAsync()
    {
        _tcpListener = new TcpListener(IPAddress.Any, _config.UnicastPort);
        _tcpListener.Start();
        _isRunning = true;
        Console.WriteLine($"Serveur TLS démarré sur le port {_config.UnicastPort}...");

        while (_isRunning)
        {
            var client = await _tcpListener.AcceptTcpClientAsync();
            Console.WriteLine($"Client sécurisé connecté : {client.Client.RemoteEndPoint}");

            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        {
            var handler = new TlsConnectionHandler(client, _serverCertificate, OnMessageReceived);
            await handler.HandleConnectionAsync();
        }
    }


    public void Stop()
    {
        _isRunning = false;
        _tcpListener?.Stop();
        Console.WriteLine("Serveur TLS arrêté.");
    }
}