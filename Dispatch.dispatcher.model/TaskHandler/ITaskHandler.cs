namespace Dispatch.dispatcher.model.TaskHandler;

public interface ITaskHandler
{
    bool CanHandle(TaskItem.TaskType type);
    void Handle(TaskItem task);
}
