namespace Dispatch.dispatcher.model;

public class TcpContainer
{
    private readonly Dictionary<string, ContainerConnection> _connections = new();

    /// <summary>
    /// Ajoute une connexion à la liste ou la met à jour.
    /// </summary>
    public void AddOrUpdateConnection(string name, string ip, int port)
    {
        if (!_connections.ContainsKey(name))
        {
            _connections[name] = new ContainerConnection(name, ip, port);
            Console.WriteLine($"[TcpContainer] Connexion ajoutée : {name} ({ip}:{port})");
        }
    }

    /// <summary>
    /// Essaie de récupérer une connexion existante.
    /// </summary>
    public bool TryGetConnection(string containerId, out ContainerConnection connection)
    {
        return _connections.TryGetValue(containerId, out connection);
    }

    /// <summary>
    /// Envoie une tâche à tous les containers actifs.
    /// </summary>
    public void SendTaskToAll(TaskItem task)
    {
        foreach (var (name, connection) in _connections)
        {
            try
            {
                Console.WriteLine($"[TcpContainer] Envoi de la tâche {task.Command} au container {name}");
                connection.EnqueueTask(task);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TcpContainer] Échec de l'envoi à {name} : {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Envoie la tâche au premier container concerné dans la liste de task.ContainerId
    /// </summary>
    public void SendTaskAtFirst(TaskItem task)
    {
        foreach (var id in task.ContainerId)
        {
            if (!_connections.TryGetValue(id, out var connection))
                continue;

            try
            {
                Console.WriteLine($"[TcpContainer] Envoi ciblé au container {id}");
                connection.EnqueueTask(task);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TcpContainer] Envoi échoué vers {id} : {ex.Message}");
            }
        }

        throw new InvalidOperationException("[TcpContainer] Aucun container disponible pour traiter la tâche.");
    }

    /// <summary>
    /// Retourne la liste des noms de containers connectés.
    /// </summary>
    public IEnumerable<string> GetAllContainerIds() => _connections.Keys;
}
