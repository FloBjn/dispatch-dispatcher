using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Text;

namespace Dispatch.dispatcher.model;

public class TcpServer
{
    private readonly Config _config;
    private readonly X509Certificate2 _serverCertificate;
    private TcpListener _tcpListener;
    private bool _isRunning;
    private readonly ClientMessageAnalyser _messageAnalyser;
    private readonly List<Client> _activeClients = new();
    private readonly DownloadHandler _downloadHandler;
    
    private const int ByteBufferSize = 1024;
    
    private readonly Protocol _protocol = Protocol.GetInstance();

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
            var client = await _tcpListener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    private async Task HandleClientAsync(TcpClient tcpClient)
    {
        using (tcpClient)
        {
            await using NetworkStream networkStream = tcpClient.GetStream();
            await using SslStream sslStream = new SslStream(networkStream, false);

            try
            {
                var options = new SslServerAuthenticationOptions
                {
                    ServerCertificate = _serverCertificate
                };

                await sslStream.AuthenticateAsServerAsync(options);

                while (tcpClient.Connected)
                {
                    byte[] buffer = new byte[ByteBufferSize];
                    int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    string response = "";
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    if (_protocol.ParseLogin(message) is (string username, string password))
                    {
                        response = await OnLoginReceived.Invoke(username, password);
                        if (response == _protocol.BuildLoginOk())
                        {
                            var client = new Client(tcpClient, username, false);
                            _activeClients.Add(client);
                        }
                    }
                    else if (_protocol.ParseRegister(message) is (string newUser, string newPassword))
                    {
                        response = await OnRegisterReceived.Invoke(newUser, newPassword);
                        if (response == _protocol.BuildLoginOk())
                        {
                            var client = new Client(tcpClient, newUser, false);
                            _activeClients.Add(client);
                        }
                    }
                    else if (_protocol.ParseCatalogue(message) != null)
                    {
                        var activeClient = _activeClients.Find(c => c.TcpClient == tcpClient);
                        if (activeClient != null && OnCatalogReceived != null)
                        {
                            response = await OnCatalogReceived.Invoke(activeClient.Username);
                        }
                    }
                    else
                    {
                        var activeClient = _activeClients.FirstOrDefault(c => c.TcpClient == tcpClient);
                        if (activeClient == null)
                        {
                            response = _protocol.BuildLoginError();
                        }
                        else
                        {
                            TaskItem? taskItem = _messageAnalyser.AnalyseMessage(buffer, bytesRead, activeClient);
                            if (taskItem != null && OnTaskReceived != null)
                            {
                                taskItem.CompletionSource = new TaskCompletionSource<string>();

                                await OnTaskReceived.Invoke(taskItem);

                                var containerResult = await taskItem.CompletionSource.Task;
                                response = containerResult switch
                                {
                                    "POST_OK"    => _protocol.BuildSendOk(),     
                                    "POST_ERROR" => _protocol.BuildSendError(),  
                                    "DELETE_OK"  => _protocol.BuildPurgeOk(),   
                                    "DELETE_ERROR" => _protocol.BuildPurgeError(),
                                    "GET_OK" => _downloadHandler.HandleDownload(taskItem),
                                    "GET_ERROR" => _protocol.BuildDownloadError(),
                                    _            => "ERROR"                      
                                };
                            }

                        }
                    }

                    if (!string.IsNullOrEmpty(response))
                    {
                        await SendResponseAsync(sslStream, response);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur TLS : {ex.Message}");
            }
        }
    }

    private async Task SendResponseAsync(SslStream sslStream, string response)
    {
        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        await sslStream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }

    public void Stop()
    {
        _isRunning = false;
        _tcpListener?.Stop();
        Console.WriteLine("Serveur TLS arrêté.");
    }
}