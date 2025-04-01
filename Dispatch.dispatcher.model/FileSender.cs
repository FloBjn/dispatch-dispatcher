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
        public string SendFile(TaskItem task)
        {
            if (string.IsNullOrEmpty(task.Command))
                throw new ArgumentException("La commande à envoyer est vide !");

            _tcpContainer.SendTaskToAll(task);

            // ici, on attend la réponse (synchronisation simple pour l'instant)
            while (string.IsNullOrEmpty(task.Result))
            {
                Thread.Sleep(10); // ou mieux : une Task avec WaitHandle si on a le temps (plus propre)
            }

            return task.Result;
        }

    }
}