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

    public string Dispatch(TaskItem task)
    {
        try
        {
            string result = task.Type switch
            {
                TaskItem.TaskType.Send => HandleUpload(task) == "POST_OK"
                    ? DispatcherProtocol.BuildSendOk()
                    : DispatcherProtocol.BuildSendError(),

                TaskItem.TaskType.Receive => "", //TODO 
                TaskItem.TaskType.Delete => "",  //TODO 

                _ => ""
            };

            task.Status = TaskItem.TaskStatus.Completed;

            task.CompletionSource?.SetResult(result);

            return result;
        }
        catch (Exception ex)
        {
            task.Status = TaskItem.TaskStatus.Error;

            task.CompletionSource?.SetResult("ERROR"); 
            return "";
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