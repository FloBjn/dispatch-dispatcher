namespace Dispatch.dispatcher.model;

public interface ITaskDispatcher
{
    void Dispatch(TaskItem task);
}