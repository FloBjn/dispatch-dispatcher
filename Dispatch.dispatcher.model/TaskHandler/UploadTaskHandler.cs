namespace Dispatch.dispatcher.model.TaskHandler;

public class UploadTaskHandler : ITaskHandler
{
    private readonly IFileManager _fileManager;
    private readonly FileSender _fileSender;

    public UploadTaskHandler(IFileManager fileManager, FileSender fileSender)
    {
        _fileManager = fileManager;
        _fileSender = fileSender;
    }

    public bool CanHandle(TaskItem.TaskType type)
    {
        return type == TaskItem.TaskType.Send;
    }

    public void Handle(TaskItem task)
    {
        try
        {
            string response = _fileSender.SendFile(task);

            if (response == "POST_OK")
            {
                task.Status = TaskItem.TaskStatus.Completed;
                _fileManager.SaveFile(task);
            }
            else
            {
                task.Status = TaskItem.TaskStatus.Error;
            }

            task.CompletionSource?.TrySetResult(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UploadTaskHandler] Erreur : {ex.Message}");
            task.Status = TaskItem.TaskStatus.Error;
            task.CompletionSource?.SetResult("POST_ERROR");
        }
    }
}