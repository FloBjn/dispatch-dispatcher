using Dispatch.dispatcher.model.TaskHandler;

namespace Dispatch.dispatcher.model;

public class TaskDispatcher : ITaskDispatcher
{
    private readonly List<ITaskHandler> _handlers;

    public TaskDispatcher(IFileManager fileManager, FileSender fileSender)
    {
        _handlers = new List<ITaskHandler>
        {
            new UploadTaskHandler(fileManager, fileSender),
            new DownloadTaskHandler(fileSender),
            new DeleteTaskHandler(fileManager, fileSender)
        };
    }

    public void Dispatch(TaskItem task)
    {
        try
        {
            var handler = _handlers.FirstOrDefault(h => h.CanHandle(task.Type));
            if (handler != null)
            {
                handler.Handle(task);
            }
            else
            {
                Console.WriteLine($"[TaskDispatcher] Aucun handler pour le type : {task.Type}");
                task.Status = TaskItem.TaskStatus.Error;
                task.CompletionSource?.SetResult("UNKNOWN_TASK_TYPE");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TaskDispatcher] Erreur inattendue : {ex.Message}");
            task.Status = TaskItem.TaskStatus.Error;
            task.CompletionSource?.SetResult("ERROR");
        }
    }
}