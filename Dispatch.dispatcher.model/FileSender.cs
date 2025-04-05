namespace Dispatch.dispatcher.model;

public class FileSender
{
    private readonly TcpContainer _tcpContainer;

    public FileSender(TcpContainer tcpContainer)
    {
        _tcpContainer = tcpContainer;
    }

    /// <summary>
    /// Envoie une tâche à tous les containers et attend une réponse.
    /// </summary>
    public string SendFile(TaskItem task)
    {
        EnsureCommandPresent(task);

        _tcpContainer.SendTaskToAll(task);

        return WaitForResult(task);
    }

    /// <summary>
    /// Envoie une tâche à un seul container (le premier disponible).
    /// </summary>
    public string SendTaskAtFirstContainerConnected(TaskItem task)
    {
        EnsureCommandPresent(task);

        _tcpContainer.SendTaskAtFirst(task);

        return WaitForResult(task);
    }

    /// <summary>
    /// Envoie une commande DELETE à chaque container concerné et attend les réponses.
    /// </summary>
    public void SendDeleteToEach(TaskItem task)
    {
        var subTasks = new List<Task<string>>();

        foreach (var containerId in task.ContainerId)
        {
            if (_tcpContainer.TryGetConnection(containerId, out var connection))
            {
                var subTask = CreateDeleteSubTask(task, containerId);
                subTasks.Add(subTask.CompletionSource.Task);
                connection.EnqueueTask(subTask);
            }
            else
            {
                Console.WriteLine($"[Delete] Container non connecté : {containerId}");
            }
        }

        Task.Run(async () =>
        {
            try
            {
                var results = await Task.WhenAll(subTasks);

                if (results.All(r => r == "DELETE_OK"))
                {
                    task.Status = TaskItem.TaskStatus.Completed;
                    task.CompletionSource?.TrySetResult("DELETE_OK");
                }
                else
                {
                    task.Status = TaskItem.TaskStatus.Error;
                    task.CompletionSource?.TrySetResult("DELETE_ERROR");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Delete] Erreur : {ex.Message}");
                task.Status = TaskItem.TaskStatus.Error;
                task.CompletionSource?.TrySetResult("DELETE_ERROR");
            }
        });
    }

    private string WaitForResult(TaskItem task, int timeoutMs = 5000)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        while (string.IsNullOrEmpty(task.Result))
        {
            if (sw.ElapsedMilliseconds > timeoutMs)
            {
                Console.WriteLine("[FileSender] Timeout : pas de réponse du container.");
                return "TIMEOUT";
            }

            Thread.Sleep(10);
        }

        return task.Result;
    }

    private void EnsureCommandPresent(TaskItem task)
    {
        if (string.IsNullOrEmpty(task.Command))
            throw new ArgumentException("Commande vide !");
    }

    private TaskItem CreateDeleteSubTask(TaskItem parent, string containerId)
    {
        return new TaskItem(
            TaskItem.TaskType.Delete,
            TaskItem.TaskStatus.InProgress,
            parent.FileName,
            parent.Command,
            parent.UserName,
            new List<string> { containerId }
        )
        {
            CompletionSource = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously)
        };
    }
}
