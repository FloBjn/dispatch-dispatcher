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
            switch (task.Type)
            {
                case TaskItem.TaskType.Send:
                    string result = HandleUpload(task);
                    if (result == "POST_OK")
                    {
                        task.Status = TaskItem.TaskStatus.Completed;
                        task.CompletionSource?.SetResult(DispatcherProtocol.BuildSendOk());
                    }
                    else
                    {
                        task.Status = TaskItem.TaskStatus.Error;
                        task.CompletionSource?.SetResult(DispatcherProtocol.BuildSendError());
                    }
                    break;

                case TaskItem.TaskType.Receive:
                    HandleDownload(task);
                    // Gère le statut + réponse via CompletionSource plus tard
                    break;

                case TaskItem.TaskType.Delete:
                    HandleDelete(task);
                    // Idem
                    break;

                default:
                    task.Status = TaskItem.TaskStatus.Error;
                    task.CompletionSource?.SetResult("UNKNOWN_TASK_TYPE");
                    break;
            }
        }
        catch (Exception ex)
        {
            task.Status = TaskItem.TaskStatus.Error;
            task.CompletionSource?.SetResult("ERROR: " + ex.Message);
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
    }

    private void HandleDelete(TaskItem task)
    {
        // TODO : Envoyer une commande DELETE au container
    }
}