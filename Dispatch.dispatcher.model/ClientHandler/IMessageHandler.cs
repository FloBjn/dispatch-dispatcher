namespace Dispatch.dispatcher.model;

public interface IMessageHandler
{
    bool CanHandle(string message);
    Task<string?> HandleAsync(ClientHandlerContext context, string message);
}
