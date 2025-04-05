namespace Dispatch.dispatcher.model.TaskHandler;

public class DownloadTaskHandler : ITaskHandler
{
    private readonly FileSender _fileSender;

    public DownloadTaskHandler(FileSender fileSender)
    {
        _fileSender = fileSender;
    }

    public bool CanHandle(TaskItem.TaskType type)
    {
        return type == TaskItem.TaskType.Receive;
    }

    public void Handle(TaskItem task)
    {
        try
        {
            var response = _fileSender.SendTaskAtFirstContainerConnected(task);

            if (!string.IsNullOrEmpty(response) && response == "GET_OK")
            {
                task.Status = TaskItem.TaskStatus.Completed;
            }
            else
            {
                task.Status = TaskItem.TaskStatus.Error;
            }

            task.CompletionSource?.TrySetResult(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DownloadTaskHandler] Erreur : {ex.Message}");
            task.Status = TaskItem.TaskStatus.Error;
            task.CompletionSource?.SetResult("GET_ERROR");
        }
    }
}