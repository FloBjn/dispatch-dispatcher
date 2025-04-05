using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Dispatch.dispatcher.model;

public class TcpServer
{
    private readonly Config _config;
    private readonly X509Certificate2 _serverCertificate;
    private TcpListener _tcpListener;
    private bool _isRunning;
    private readonly ClientMessageAnalyser _messageAnalyser;
    private readonly DownloadHandler _downloadHandler;
    public event Func<TaskItem, Task<string>>? OnTaskReceived;
    public event Func<string, string, Task<string>> OnLoginReceived;
    public event Func<string, string, Task<string>> OnRegisterReceived;
    public event Func<string, Task<string>>? OnCatalogReceived;

    public TcpServer(Config config, ClientMessageAnalyser messageAnalyser, DownloadHandler downloadHandler)
    {
        _config = config;
        _serverCertificate = new X509Certificate2(config.CertificatePath, config.CertificatePassword);
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
            ClientHandler clientHandler = new ClientHandler(_messageAnalyser, _downloadHandler, _serverCertificate);
            clientHandler.OnCatalogReceived += OnCatalogReceived!.Invoke;
            clientHandler.OnLoginReceived += OnLoginReceived.Invoke;
            clientHandler.OnRegisterReceived += OnRegisterReceived.Invoke;
            clientHandler.OnTaskReceived += OnTaskReceived!.Invoke;
            var client = await _tcpListener.AcceptTcpClientAsync();
            _ = Task.Run(() => clientHandler.HandleClientAsync(client));
        }
    }
}