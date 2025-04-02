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

        public string SendTaskAtFirstContainerConnected(TaskItem task)
        {
            if (string.IsNullOrEmpty((task.Command)))
                throw new ArgumentException("La commande est vide");
                
            _tcpContainer.SendTaskAtFirst(task);
            
            while (string.IsNullOrEmpty(task.Result))
            {
                Thread.Sleep(10);
            }
            
            return task.Result;
        }

        public void SendDeleteToEach(TaskItem task)
        {
            // crée une liste de tâches asynchrones de suppression
            var tasks = new List<Task<string>>();

            foreach (var containerId in task.ContainerId)
            {
                // vérifie si le container est actif
                if (_tcpContainer.TryGetConnection(containerId, out var connection))
                {
                    // crée une tâche de suppression
                    var deleteTask = new TaskItem(
                        TaskItem.TaskType.Delete,
                        TaskItem.TaskStatus.InProgress,
                        task.FileName,
                        task.Command,
                        task.UserName,
                        new List<string> { containerId }
                    )
                    {
                        CompletionSource =
                            new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously)
                    };
                    // ajoute la tâche de suppression à la liste
                    tasks.Add(deleteTask.CompletionSource.Task);
                    // envoie la tâche de suppression au container
                    connection.EnqueueTask(deleteTask);
                }
                else
                {
                    Console.WriteLine($"Aucune connexion trouvée pour le containerId : {containerId}");
                }
            }
            
            // Nouvelle tâche en arrière-plan pour attendre les résultats
            Task.Run(async () =>
            {
                try
                {
                    // attend la fin de toutes les tâches de suppression
                    var results = await Task.WhenAll(tasks);
                    
                    // verifie si toutes les tâches de suppression ont fonctionnées
                    if (results.All(r => r == "DELETE_OK"))
                    {
                        task.CompletionSource?.SetResult("DELETE_OK");
                        task.Status = TaskItem.TaskStatus.Completed;
                    }
                    else
                    {
                        Console.WriteLine("Certaines tâches de suppression ont échoué.");
                        task.CompletionSource?.SetResult("DELETE_ERROR");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'attente des suppressions : {ex.Message}");
                    task.CompletionSource?.SetResult("DELETE_ERROR");
                }
            });
        }
    }
}