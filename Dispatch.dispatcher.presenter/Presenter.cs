using Dispatch.disapatcher.model;

namespace Dispatch.dispatcher.presenter;

public class Presenter(IMainView view, TcpServer server, ClientMessageAnalyser analyser, IFileManager fileManager)
{
    public async Task StartAsync()
    {
        server.OnMessageReceived += AnalyseMessageAsync;
        await server.StartAsync();
    }
    
    private Task<string> AnalyseMessageAsync(string message)
    {
        string response = analyser.Analyse(message);
        
       
        
        return Task.FromResult(string.IsNullOrEmpty(response) ? "Le message du client n'est pas reconnu." : response);
    }
}