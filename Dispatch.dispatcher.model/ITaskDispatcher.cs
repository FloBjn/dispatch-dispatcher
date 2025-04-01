namespace Dispatch.dispatcher.model;

public interface ITaskDispatcher
{
    string Dispatch(TaskItem task);
}