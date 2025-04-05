namespace Dispatch.dispatcher.model
{
    public class TcpContainer
    {
        private readonly Dictionary<string, ContainerConnection?> _connections = new();

        /// <summary>
        /// Envoie une tâche à tous les Containers actifs en parallèle.
        /// </summary>
        /// <param name="task">Tâche à envoyer</param>
        public void SendTaskToAll(TaskItem task)
        {
            foreach (var conn in _connections.Values)
            {
                conn.EnqueueTask(task);
            }
        }

        public void AddOrUpdateConnection(string name, string ip, int port)
        {
            if (!_connections.ContainsKey(name))
            {
                _connections[name] = new ContainerConnection(name, ip, port);
            }
        }

        public void SendTaskAtFirst(TaskItem task)
        {
            var taskSent = false;
            foreach (var id in task.ContainerId)
            {
                if (!_connections.TryGetValue(id, out var connection)) continue;
                try
                {
                    connection.EnqueueTask(task);
                    taskSent = true;
                    break;
                }
                catch (Exception)
                {
                    throw new InvalidOperationException("Erreur lors de l'envoie");
                }
            }

            if (!taskSent)
            {
                throw new InvalidOperationException("Failed to send task to any container.");
            }
        }

        public bool TryGetConnection(string containerId, out ContainerConnection? connection)
        {
            return _connections.TryGetValue(containerId, out connection);
        }
    }
}