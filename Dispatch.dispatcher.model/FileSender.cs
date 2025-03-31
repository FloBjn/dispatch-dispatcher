namespace Dispatch.dispatcher.model
{
    public class FileSender
    {
        private readonly TcpContainer _tcpContainer;

        public FileSender(TcpContainer tcpContainer)
        {
            _tcpContainer = tcpContainer;
        }

        /// <summary>
        /// Envoie une tâche (commande + contenu binaire) à tous les containers.
        /// </summary>
        public void SendFile(TaskItem task)
        {
            if (string.IsNullOrEmpty(task.Command))
            {
                throw new ArgumentException("La commande à envoyer est vide !");
            }

            _tcpContainer.SendTaskToAll(task);
        }
    }
}