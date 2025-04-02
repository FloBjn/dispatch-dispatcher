namespace Dispatch.dispatcher.model;

public class TaskDispatcher : ITaskDispatcher
{
    private readonly IFileManager _fileManager;
    private readonly FileSender _fileSender;
    
    private readonly Protocol _protocol = Protocol.GetInstance();
    
    public TaskDispatcher(IFileManager fileManager, FileSender fileSender)
    {
        _fileManager = fileManager;
        _fileSender = fileSender;
    }

    public void Dispatch(TaskItem task)
    {
        try
        {
            string result = "";

            switch (task.Type)
            {
                case TaskItem.TaskType.Send:
                    result = HandleUpload(task);
                    Console.WriteLine("CACA" + result);
                    task.Status = result == "POST_OK"
                        ? TaskItem.TaskStatus.Completed
                        : TaskItem.TaskStatus.Error;
                    break;

                case TaskItem.TaskType.Receive:
                    HandleDownload(task); // TODO: compléter la logique
                    task.Status = TaskItem.TaskStatus.Completed;
                    result = "DOWNLOAD_OK"; // temporaire
                    break;

                case TaskItem.TaskType.Delete:
                    HandleDelete(task); // TODO: compléter
                    task.Status = TaskItem.TaskStatus.Completed;
                    result = "DELETE_OK"; // temporaire
                    break;

                default:
                    task.Status = TaskItem.TaskStatus.Error;
                    result = "UNKNOWN_TASK_TYPE";
                    break;
            }

            task.CompletionSource?.SetResult(result);
        }
        catch (Exception ex)
        {
            task.Status = TaskItem.TaskStatus.Error;
            task.CompletionSource?.SetResult("ERROR");
            Console.WriteLine($"[TaskDispatcher] Erreur : {ex.Message}");
        }
    }


    private string HandleUpload(TaskItem task)
    {
        string response = _fileSender.SendFile(task);
        _fileManager.SaveFile(task);
        return response;
    }

    private void HandleDownload(TaskItem task)
    {
        // TODO : Interroger le container pour récupérer le fichier
        _fileSender.SendTaskAt(task.ContainerId, task);
    }

    private void HandleDelete(TaskItem task)
    {
        // TODO : Envoyer une commande DELETE au container
    }
}