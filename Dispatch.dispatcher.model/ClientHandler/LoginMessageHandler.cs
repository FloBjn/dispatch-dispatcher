namespace Dispatch.dispatcher.model;

public class LoginMessageHandler : IMessageHandler
{
    private readonly Protocol _protocol = Protocol.GetInstance();

    public bool CanHandle(string message)
    {
        return _protocol.ParseLogin(message) is not null;
    }

    public async Task<string?> HandleAsync(ClientHandlerContext context, string message)
    {
        var parsed = _protocol.ParseLogin(message);
        if (parsed is not (string username, string password))
            return _protocol.BuildLoginError();

        var response = await context.OnLoginReceived(username, password);

        if (response == _protocol.BuildLoginOk())
        {
            context.Client = new Client(context.TcpClient, username, false);
        }

        return response;
    }
}