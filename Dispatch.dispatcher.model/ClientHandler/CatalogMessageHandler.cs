namespace Dispatch.dispatcher.model;

public class CatalogMessageHandler : IMessageHandler
{
    private readonly Protocol _protocol = Protocol.GetInstance();

    public bool CanHandle(string message)
    {
        return _protocol.ParseCatalogue(message) is not null;
    }

    public async Task<string?> HandleAsync(ClientHandlerContext context, string message)
    {
        if (context.Client == null)
        {
            return _protocol.BuildLoginError(); 
        }

        string response = await context.OnCatalogReceived.Invoke(context.Client.Username);
        return response;
    }
}