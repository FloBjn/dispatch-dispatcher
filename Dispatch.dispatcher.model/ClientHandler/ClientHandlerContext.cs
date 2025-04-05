using System.Net.Security;
using System.Net.Sockets;
using Dispatch.dispatcher.model;

public class ClientHandlerContext
{
    public Client? Client { get; set; }
    public TcpClient TcpClient { get; }
    public SslStream SslStream { get; }

    public Func<string, string, Task<string>> OnLoginReceived { get; }
    public Func<string, string, Task<string>> OnRegisterReceived { get; }
    public Func<string, Task<string>>? OnCatalogReceived { get; }
    public Func<TaskItem, Task<string>>? OnTaskReceived { get; }

    public ClientMessageAnalyser MessageAnalyser { get; }
    public DownloadHandler DownloadHandler { get; }

    public ClientHandlerContext(
        TcpClient tcpClient,
        SslStream sslStream,
        Func<string, string, Task<string>> onLoginReceived,
        Func<string, string, Task<string>> onRegisterReceived,
        Func<string, Task<string>>? onCatalogReceived,
        Func<TaskItem, Task<string>>? onTaskReceived,
        ClientMessageAnalyser messageAnalyser,
        DownloadHandler downloadHandler
    )
    {
        TcpClient = tcpClient;
        SslStream = sslStream;
        OnLoginReceived = onLoginReceived;
        OnRegisterReceived = onRegisterReceived;
        OnCatalogReceived = onCatalogReceived;
        OnTaskReceived = onTaskReceived;
        MessageAnalyser = messageAnalyser;
        DownloadHandler = downloadHandler;
    }
}