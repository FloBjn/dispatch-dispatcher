using System.Collections.Concurrent;
using System.Text;

namespace Dispatch.dispatcher.model;

public class ContainerConnection
{
    private readonly string _name;
    private readonly string _ip;
    private readonly int _port;
    private readonly BlockingCollection<TaskItem> _taskQueue = new();

    public ContainerConnection(string name, string ip, int port)
    {
        _name = name;
        _ip = ip;
        _port = port;

        var thread = new Thread(ProcessTasks)
        {
            IsBackground = true,
            Name = $"ContainerWorker-{name}"
        };
        thread.Start();
    }

    public void EnqueueTask(TaskItem task)
    {
        _taskQueue.Add(task);
    }

    private void ProcessTasks()
    {
        var connection = new UnicastConnection();

        foreach (var task in _taskQueue.GetConsumingEnumerable())
        {
            try
            {
                connection.Connect(_ip, _port);
                Console.WriteLine($"[ContainerConnection:{_name}] Connexion établie avec {_ip}:{_port}");

                SendCommandHeader(connection, task);

                if (task.BinaryPayload is { Length: > 0 })
                {
                    var ack = connection.ReadLine()?.Trim();
                    if (ack != "READY")
                    {
                        Console.WriteLine($"[ContainerConnection:{_name}] Réponse inattendue : {ack}");
                        task.CompletionSource?.TrySetResult("POST_ERROR");
                        connection.Disconnect();
                        continue;
                    }

                    SendOptionalPayload(connection, task);
                }

                var response = connection.ReadLine()?.Trim();

                if (!string.IsNullOrEmpty(response))
                {
                    task.Result = response;

                    var parts = response.Split(new[] { ' ' }, 2);
                    task.CompletionSource?.TrySetResult(parts[0]);
                }
                else
                {
                    Console.WriteLine($"[ContainerConnection:{_name}] Réponse vide.");
                    task.CompletionSource?.TrySetResult("ERROR");
                }

                connection.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ContainerConnection:{_name}] Erreur : {ex.Message}");
                task.CompletionSource?.TrySetResult("ERROR");
            }
        }
    }

    private void SendCommandHeader(UnicastConnection connection, TaskItem task)
    {
        if (string.IsNullOrWhiteSpace(task.Command))
            throw new InvalidOperationException("Commande de tâche vide.");

        byte[] header = Encoding.UTF8.GetBytes(task.Command);
        connection.SendBytes(header);
    }

    private void SendOptionalPayload(UnicastConnection connection, TaskItem task)
    {
        if (task.BinaryPayload is { Length: > 0 })
        {
            connection.SendBytes(task.BinaryPayload);
        }
    }

    public override string ToString() => $"{_name} ({_ip}:{_port})";
}
