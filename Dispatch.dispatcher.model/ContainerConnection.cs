using System.Collections.Concurrent;
using Dispatch.disapatcher.model;

namespace Dispatch.dispatcher.model
{
    public class ContainerConnection
    {
        private readonly string _name;
        private readonly string _ip;
        private readonly int _port;
        private readonly BlockingCollection<TaskItem> _taskQueue = new();
        private readonly Thread _thread;

        public ContainerConnection(string name, string ip, int port)
        {
            _name = name;
            _ip = ip;
            _port = port;

            _thread = new Thread(ProcessTasks)
            {
                IsBackground = true
            };
            _thread.Start();
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

                    byte[] header = System.Text.Encoding.UTF8.GetBytes(task.Command + "\r\n");
                    connection.SendBytes(header);

                    if (task.BinaryPayload.Length > 0)
                    {
                        connection.SendBytes(task.BinaryPayload);
                    }

                    connection.Disconnect();
                    Console.WriteLine($"\u2705 Message binaire envoyé à {_name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\u26a0\ufe0f Erreur d'envoi vers {_name} : {ex.Message}");
                }
            }
        }
    }
}