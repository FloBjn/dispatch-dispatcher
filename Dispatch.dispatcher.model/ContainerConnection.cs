using System.Collections.Concurrent;

namespace Dispatch.dispatcher.model
{
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
                IsBackground = true
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
            // Pour chaque tâche de la file d'attente
            foreach (var task in _taskQueue.GetConsumingEnumerable())
            {
                try
                {
                    connection.Connect(_ip, _port);

                    // Envoi du header de la tâche
                    var header = System.Text.Encoding.UTF8.GetBytes(task.Command );
                    connection.SendBytes(header);
                    
                    // Envoi du payload de la tâche
                    if (task.BinaryPayload.Length > 0)
                    {
                        connection.SendBytes(task.BinaryPayload);
                    }
                    
                    var response = connection.ReadLine();
                    task.Result = response.Trim(); 
                    // On set le résultat de la tâche
                    if (!string.IsNullOrEmpty(task.Result))
                    {
                        // Divise task.Result en deux parties au maximum en utilisant l'espace comme séparateur
                        string?[] parts = task.Result.Split(new[] { ' ' }, 2);

                        // Définit le résultat de CompletionSource avec la première partie
                        task.CompletionSource?.SetResult(parts[0]);
                    }

                    connection.Disconnect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\u26a0\ufe0f Erreur d'envoi vers {_name} : {ex.Message}");
                }
            }
        }
    }
}