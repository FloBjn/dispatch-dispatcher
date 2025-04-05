namespace Dispatch.dispatcher.model;

public class RegisterMessageHandler : IMessageHandler
{
    private readonly Protocol _protocol = Protocol.GetInstance();

    public bool CanHandle(string message)
    {
        return _protocol.ParseRegister(message) is not null;
    }

    public async Task<string?> HandleAsync(ClientHandlerContext context, string message)
    {
        var parsed = _protocol.ParseRegister(message);
        if (parsed is not (string newUser, string newPassword))
            return _protocol.BuildLoginError();

        var response = await context.OnRegisterReceived(newUser, newPassword);

        if (response == _protocol.BuildLoginOk())
        {
            var client = new Client(context.TcpClient, newUser, false);
            context.Client = client;
        }

        return response;
    }
}