using System.Collections.Concurrent;
using Dispatch.disapatcher.model;

namespace Dispatch.dispatcher.model;

public class ContainerConnection
{
    private readonly string _name;
    private readonly string _ip;
    private readonly int _port;
    private readonly BlockingCollection<string> _messageQueue = new();
    private readonly Thread _thread;

    public ContainerConnection(string name, string ip, int port)
    {
        _name = name;
        _ip = ip;
        _port = port;

        _thread = new Thread(ProcessMessages)
        {
            IsBackground = true
        };
        _thread.Start();
    }

    public void EnqueueMessage(string message)
    {
        _messageQueue.Add(message);
    }

    private void ProcessMessages()
    {
        var connection = new UnicastConnection();

        foreach (var message in _messageQueue.GetConsumingEnumerable())
        {
            try
            {
                connection.Connect(_ip, _port);
                connection.SendMessage(message);
                connection.Disconnect();
                Console.WriteLine($"✅ Message envoyé à {_name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur d'envoi vers {_name} : {ex.Message}");
            }
        }
    }
}