using Dispatch.disapatcher.model;

namespace Dispatch.dispatcher.model;

/// <summary>
/// Classe permettant d'envoyer des messages en unicast aux Containers connectés.
/// </summary>
public class TcpContainer
{
    private readonly Dictionary<string, ContainerConnection> _connections = new();
    private readonly ContainerManager _containerManager;
    private readonly UnicastConnection _connection;

    public TcpContainer(ContainerManager containerManager, UnicastConnection connection)
    {
        _containerManager = containerManager;
        _connection = connection;
    }

    /// <summary>
    /// Envoie un message à **tous** les Containers actifs en parallèle.
    /// </summary>
    /// <param name="message">Message à envoyer</param>
    public void SendMessageToAll(string message)
    {
        foreach (var conn in _connections.Values)
        {
            conn.EnqueueMessage(message);
        }
    }
    /// <summary>
    /// Envoie un message à un container spécifique via TCP.
    /// </summary>
    private void SendMessage(string ip, int port, string message)
    {
        try
        {
            _connection.Connect(ip, port);
            _connection.SendMessage(message);
            _connection.Disconnect();
            Console.WriteLine($"Message envoyé à {ip}:{port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur d'envoi vers {ip}:{port} : {ex.Message}");
        }
    }
    public void AddOrUpdateConnection(string name, string ip, int port)
    {
        if (!_connections.ContainsKey(name))
        {
            Console.WriteLine($"🔌 Initialisation du thread de communication pour {name}...");
            _connections[name] = new ContainerConnection(name, ip, port);
        }
    }
}
