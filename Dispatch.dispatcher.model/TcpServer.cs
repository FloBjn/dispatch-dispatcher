using System.Net;
using System.Net.Sockets;

namespace Dispatch.dispatcher.model;

public class TcpServer
{
    private readonly Config _config;
    private TcpListener _tcpListener;
    private bool _isRunning;
    private readonly ClientMessageAnalyser _messageAnalyser;
    private readonly DownloadHandler _downloadHandler;
    private ClientHandler _clientHandler;
    public event Func<TaskItem, Task<string>>? OnTaskReceived;

    public event Func<string, string, Task<string>> OnLoginReceived;

    public event Func<string, string, Task<string>> OnRegisterReceived;

    public event Func<string, Task<string>>? OnCatalogReceived;

    public TcpServer(Config config, ClientMessageAnalyser messageAnalyser, DownloadHandler downloadHandler)
    {
        _config = config;
        _messageAnalyser = messageAnalyser;
        _downloadHandler = downloadHandler;
    }

    public async Task StartAsync()
    {
        _tcpListener = new TcpListener(IPAddress.Any, _config.UnicastPort);
        _tcpListener.Start();
        _isRunning = true;

        while (_isRunning)
        {            
            var client = await _tcpListener.AcceptTcpClientAsync();
            _clientHandler = new ClientHandler(_config, _messageAnalyser, _downloadHandler);
            _clientHandler.OnLoginReceived +=OnLoginReceived.Invoke;
            _clientHandler.OnRegisterReceived +=OnRegisterReceived.Invoke;
            _clientHandler.OnCatalogReceived +=OnCatalogReceived!.Invoke;
            _clientHandler.OnTaskReceived +=OnTaskReceived!.Invoke;
            _ = Task.Run(() => _clientHandler.HandleClientAsync(client));
        }
    }
}