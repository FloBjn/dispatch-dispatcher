using Dispatch.dispatcher.model;

namespace Dispatch.dispatcher.presenter;

public class Presenter
{
    private readonly IMainView _view;
    private readonly TcpServer _unicastServer;
    private readonly MulticastListener _multicastListener;
    private readonly ClientMessageAnalyser _analyser;

    public Presenter(IMainView view, TcpServer unicastServer,
        MulticastListener multicastListener, ClientMessageAnalyser analyser)
    {
        _view = view;
        _unicastServer = unicastServer;
        _multicastListener = multicastListener;
        _analyser = analyser;
        
        _multicastListener.OnMessageReceived += ShowMessageContainer;
        _unicastServer.OnMessageReceived += AnalyseMessageClient;
    }
    
    /// <summary>
    /// Démarre le serveur unicast
    /// </summary>
    public async Task StartAsync()
    {
        await _unicastServer.StartAsync();
    }

    /// <summary>
    /// Analyse le message reçu du client
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private Task<string> AnalyseMessageClient(string message)
    {
        // Affiche le message reçu du client
        _view.DisplayMessageReceivedClient(message);
        
        // Analyse le message reçu
        string response = _analyser.Analyse(message);
        
        // Affiche le message envoyé au client
        _view.DisplayMessageSentClient(response);
        
        return Task.FromResult(string.IsNullOrEmpty(response) ? "Le message du client n'est pas reconnu." : response);
    }
    
    /// <summary>
    /// Montre le message reçu dans le container
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private Task ShowMessageContainer(string message)
    {
        _view.DisplayMessageReceivedContainer(message);
        return Task.FromResult("Message reçu");
    }
}