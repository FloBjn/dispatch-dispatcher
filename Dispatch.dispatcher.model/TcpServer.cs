using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Text;

namespace Dispatch.dispatcher.model;

public class TcpServer
{
    private readonly Config _config;
    private readonly X509Certificate2 _serverCertificate;
    private TcpListener _tcpListener;
    private bool _isRunning;

    // 📡 Événement pour transmettre les messages au `Presenter`
    public event Func<string, Task<string>>? OnMessageReceived;
    
    public TcpServer(Config config)
    {
        _config = config;
        _serverCertificate = new X509Certificate2(config.CertificatePath, config.CertificatePassword);
    }

    /// <summary>
    /// Démarre le serveur TLS et écoute les connexions entrantes.
    /// </summary>
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

    /// <summary>
    /// Gère une connexion client via TLS.
    /// </summary>
    /// <param name="client"></param>
    private async Task HandleClientAsync(TcpClient client)
    {
        Console.WriteLine($"Client connecté : {client.Client.RemoteEndPoint}");
    
        using (client)
        {
            using NetworkStream networkStream = client.GetStream();
            using SslStream sslStream = new SslStream(networkStream, false);

            try
            {
                Console.WriteLine("Authentification TLS en cours...");
                var options = new SslServerAuthenticationOptions
                {
                    ServerCertificate = _serverCertificate
                };

                await sslStream.AuthenticateAsServerAsync(options);
                Console.WriteLine($"Connexion TLS établie avec protocole : {sslStream.SslProtocol}");

                while (client.Connected)
                {
                    string? message = await ReceiveMessageAsync(sslStream);

                    if (string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("Le client a fermé la connexion.");
                        break;
                    }
                    
                    if (OnMessageReceived != null)
                    {
                        string response = await OnMessageReceived.Invoke(message);
                        await SendResponseAsync(sslStream, response);
                    }
                    else
                    {
                        Console.WriteLine("Aucun gestionnaire de message défini.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur TLS : {ex.Message}");
            }
        }
    }


    /// <summary>
    /// Lit un message sécurisé du client.
    /// </summary>
    private async Task<string?> ReceiveMessageAsync(SslStream sslStream)
    {
        byte[] buffer = new byte[1024];
        int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
        
        if (bytesRead == 0)
        {
            return null;
        }

        return Encoding.UTF8.GetString(buffer, 0, bytesRead);
    }

    /// <summary>
    /// Envoie une réponse sécurisée au client.
    /// </summary>
    private async Task SendResponseAsync(SslStream sslStream, string response)
    {
        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        await sslStream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }

    /// <summary>
    /// Stoppe le serveur.
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _tcpListener?.Stop();
        Console.WriteLine("Serveur TLS arrêté.");
    }
}