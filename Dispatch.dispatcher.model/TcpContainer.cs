using Dispatch.disapatcher.model;

namespace Dispatch.dispatcher.model;

/// <summary>
/// Classe permettant d'envoyer des messages en unicast aux Containers connectés.
/// </summary>
public class TcpContainer
{
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
    public async Task SendMessageToAllAsync(string message)
    {
        var activeContainers = _containerManager.GetActiveContainers();
        if (activeContainers.Count == 0)
        {
            Console.WriteLine("Aucun Container actif trouvé !");
            return;
        }

        Console.WriteLine($"Envoi du message à {activeContainers.Count} Containers...");

        // Créer une tâche pour chaque envoi et les exécuter en parallèle
        var sendTasks = activeContainers
            .Select(container => Task.Run(() => SendMessage(container.IP, container.Port, message)))
            .ToList();

        // Attendre la fin de tous les envois
        await Task.WhenAll(sendTasks);

        Console.WriteLine("Message envoyé à tous les Containers !");
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
}
