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

            foreach (var task in _taskQueue.GetConsumingEnumerable())
            {
                try
                {
                    connection.Connect(_ip, _port);

                    byte[] header = System.Text.Encoding.UTF8.GetBytes(task.Command );
                    connection.SendBytes(header);

                    if (task.BinaryPayload.Length > 0)
                    {
                        connection.SendBytes(task.BinaryPayload);
                    }
                    
                    string response = connection.ReadLine();
                    task.Result = response.Trim(); 
                    
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