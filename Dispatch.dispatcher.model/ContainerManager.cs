namespace Dispatch.dispatcher.model;

public class ContainerManager
{
    private readonly Dictionary<string, ContainerInfo> _containers = new();
    private readonly object _lock = new();

    /// <summary>
    /// Ajoute ou met à jour un Container actif.
    /// </summary>
    public void AddOrUpdateContainer(string name, string ip, int port)
    {
        lock (_lock)
        {
            if (!_containers.ContainsKey(name))
            {
                _containers[name] = new ContainerInfo { IP = ip, Port = port, LastSeen = DateTime.UtcNow };
            }
            else
            {
                _containers[name].IP = ip;
                _containers[name].Port = port;
                _containers[name].LastSeen = DateTime.UtcNow;
            }
        }
    }
    public void StartCleanupTask()
    {
        Thread cleanupThread = new Thread(() =>
        {
            while (true)
            {
                Thread.Sleep(30000); // Vérifie toutes les 30 secondes
                CleanupInactiveContainers();
            }
        })
        {
            IsBackground = true
        };
        cleanupThread.Start();
    }

    /// <summary>
    /// Nettoie les Containers inactifs.
    /// </summary>
    public void CleanupInactiveContainers()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var inactiveContainers = _containers
                .Where(c => (now - c.Value.LastSeen).TotalSeconds > 90)
                .Select(c => c.Key)
                .ToList();

            foreach (var name in inactiveContainers)
            {
                _containers.Remove(name);
                Console.WriteLine($"Container supprimé : {name} (inactif)");
            }
        }
    }

    /// <summary>
    /// Liste les Containers actifs.
    /// </summary>
    public void ListContainers()
    {
        Console.WriteLine("Liste des Containers actifs :");
        foreach (var (name, container) in _containers)
        {
            Console.WriteLine($"{name} - {container.IP}:{container.Port}");
        }
    }

    public List<(string Name, string IP, int Port)> GetActiveContainers()
    {
        lock (_lock)
        {
            return _containers.Select(c => (c.Key, c.Value.IP, c.Value.Port)).ToList();
        }
    }


    public object GetFirstContainerIp()
    {
        throw new NotImplementedException();
    }
}