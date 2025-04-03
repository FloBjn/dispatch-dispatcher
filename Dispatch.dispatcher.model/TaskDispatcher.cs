namespace Dispatch.dispatcher.model;

public class TaskDispatcher : ITaskDispatcher
{
    private readonly IFileManager _fileManager;
    private readonly FileSender _fileSender;
    
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
                    if (result == "POST_OK")
                    {
                        task.Status = TaskItem.TaskStatus.Completed;
                        _fileManager.SaveFile(task);
                    }
                    else
                    {
                        task.Status = TaskItem.TaskStatus.Error;
                    }
                    break;

                case TaskItem.TaskType.Receive:
                    HandleDownload(task); // TODO: compléter la logique
                    task.Status = TaskItem.TaskStatus.Completed;
                    result = "DOWNLOAD_OK"; // temporaire
                    break;

                case TaskItem.TaskType.Delete:
                    // on demande la suppression au container
                    HandleDelete(task);

                    // on attend la confirmation
                    if (task.CompletionSource != null)
                    {
                        var deleteResult = task.CompletionSource.Task.Result;
                        // on modifie le statut de la tâche
                        if (deleteResult == "DELETE_OK")
                        {
                            task.Status = TaskItem.TaskStatus.Completed;
                            _fileManager.DeleteFile(task);
                        }
                        else
                        {
                            task.Status = TaskItem.TaskStatus.Error;
                        }
                    }
                    return;


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
        return response;
    }

    private void HandleDownload(TaskItem task)
    {
        // TODO : Interroger le container pour récupérer le fichier
        _fileSender.SendTaskAt(task.ContainerId, task);
    }

    private void HandleDelete(TaskItem task)
    {
        _fileSender.SendDeleteToEach(task);
    }
}