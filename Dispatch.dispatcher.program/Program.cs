using Dispatch.disapatcher.model;
using Dispatch.dispatcher.presenter;
using Dispatch.dispatcher.view;

namespace Dispatch.dispatcher.app;

public class Program
{
    public static void Main()
    {
        var server = new TcpServer(1234);
        IMainView view = new View();
        var presenter = new Presenter(view, server);
        
        presenter.StartAsync().Wait();
    }
}