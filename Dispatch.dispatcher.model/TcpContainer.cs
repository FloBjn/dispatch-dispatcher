namespace Dispatch.dispatcher.model;

/// <summary>
/// Classe permettant d'envoyer des messages en unicast aux Containers connectés.
/// </summary>
public class TcpContainer
{
    private readonly Dictionary<string, ContainerConnection> _connections = new();

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
   
    public void AddOrUpdateConnection(string name, string ip, int port)
    {
        if (!_connections.ContainsKey(name))
        {
            _connections[name] = new ContainerConnection(name, ip, port);
        }
    }
}
