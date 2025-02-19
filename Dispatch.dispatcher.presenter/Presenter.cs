using Dispatch.disapatcher.model;

namespace Dispatch.dispatcher.presenter;

public class Presenter(IMainView view, TcpServer server)
{
    public async Task StartAsync()
    {
        await server.StartAsync();
    }
}