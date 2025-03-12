using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Dispatch.dispatcher.model;

public class TcpServer
{
    private TcpListener? _listener;
    private bool _isRunning;
    private Config _config;

    public event Func<string, Task<string>>? OnMessageReceived; // Événement pour transmettre le message au Presenter

    public TcpServer(Config config)
    {
        _config = config;
    }

    /// <summary>
    /// Démarre le serveur TCP et écoute les connexions entrantes.
    /// </summary>
    public async Task StartAsync()
    {
        _listener = new TcpListener(IPAddress.Any, _config.UnicastPort);
        _listener.Start();
        _isRunning = true;
        Console.WriteLine($"Serveur démarré et écoute sur le port {_config.UnicastPort}...");

        while (_isRunning)
        {
            var client = await _listener.AcceptTcpClientAsync();
            Console.WriteLine("Client connecté!");
            _ = HandleClientAsync(client);
        }
    }

    /// <summary>
    /// Gère les messages reçus d'un client.
    /// </summary>
    /// <param name="client"></param>
    private async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Client déconnecté.");
                        break; // Client déconnecté
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    // Console.WriteLine($"🔍 Message brut : [{message}]");

                    if (OnMessageReceived != null)
                    {
                        string response = await OnMessageReceived.Invoke(message);
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        await stream.WriteAsync(responseBytes);
                    }
                    else
                    {
                        Console.WriteLine("Aucun gestionnaire de message défini.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la gestion du client : {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Stoppe le serveur TCP.
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _listener?.Stop();
        Console.WriteLine("Serveur arrêté.");
    }
}