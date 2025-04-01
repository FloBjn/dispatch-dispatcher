namespace Dispatch.dispatcher.model
{
    public class TcpContainer
    {
        private readonly Dictionary<string, ContainerConnection> _connections = new();

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
    }
}