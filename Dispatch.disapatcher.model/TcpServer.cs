using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Dispatch.disapatcher.model;

public class TcpServer(int port)
{
    private TcpListener? _listener;
    private bool _isRunning;

    /**
     * Méthode pour démarrer le serveur.
     */
    public async Task StartAsync()
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        _isRunning = true;
        Console.WriteLine($"Serveur démarré et écoute sur le port {port}...");

        while (_isRunning)
        {
            var client = await _listener.AcceptTcpClientAsync();
            Console.WriteLine("Client connecté!");
            _ = HandleClientAsync(client); // Gérer chaque client dans une tâche séparée
        }
    }
    
    /**
     * Méthode pour arrêter le serveur.
     */
    public void Stop()
    {
        _isRunning = false;
        _listener?.Stop();
        Console.WriteLine("Serveur arrêté.");
    }

    
    /**
     * Méthode pour gérer un client.
     */
    private async Task HandleClientAsync(TcpClient client)
    {
        using (client) // Fermer automatiquement la connexion
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // Client déconnecté

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"🔍 Message brut : [{message}]");

                    // Optionnel : Répondre au client
                    const string response = "Message bien reçu!";
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la gestion du client : {ex.Message}");
            }
        }

        Console.WriteLine("Client déconnecté.");
    }
}