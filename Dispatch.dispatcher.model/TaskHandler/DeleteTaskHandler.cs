namespace Dispatch.dispatcher.model.TaskHandler;

public class DeleteTaskHandler : ITaskHandler
{
    private readonly IFileManager _fileManager;
    private readonly FileSender _fileSender;

    public DeleteTaskHandler(IFileManager fileManager, FileSender fileSender)
    {
        _fileManager = fileManager;
        _fileSender = fileSender;
    }

    public bool CanHandle(TaskItem.TaskType type)
    {
        return type == TaskItem.TaskType.Delete;
    }

    public void Handle(TaskItem task)
    {
        try
        {
            _fileSender.SendDeleteToEach(task);

            if (task.CompletionSource != null)
            {
                var deleteResult = task.CompletionSource.Task.Result;

                if (deleteResult == "DELETE_OK")
                {
                    _fileManager.DeleteFile(task);
                    task.Status = TaskItem.TaskStatus.Completed;
                }
                else
                {
                    task.Status = TaskItem.TaskStatus.Error;
                }

                task.CompletionSource.TrySetResult(deleteResult);
            }
            else
            {
                task.Status = TaskItem.TaskStatus.Error;
                Console.WriteLine("[DeleteTaskHandler] Pas de CompletionSource définie.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeleteTaskHandler] Erreur : {ex.Message}");
            task.Status = TaskItem.TaskStatus.Error;
            task.CompletionSource?.SetResult("DELETE_ERROR");
        }
    }
}