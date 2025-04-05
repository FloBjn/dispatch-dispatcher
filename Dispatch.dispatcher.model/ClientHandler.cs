using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Dispatch.dispatcher.model;

public class ClientHandler
{
    private const int ByteBufferSize = 1024;
    private readonly Protocol _protocol = Protocol.GetInstance();
    private readonly X509Certificate2 _serverCertificate;
    private readonly List<Client> _activeClients = new();
    private readonly ClientMessageAnalyser _messageAnalyser;
    private readonly DownloadHandler _downloadHandler;

    public event Func<TaskItem, Task<string>>? OnTaskReceived;

    public event Func<string, string, Task<string>> OnLoginReceived;

    public event Func<string, string, Task<string>> OnRegisterReceived;

    public event Func<string, Task<string>>? OnCatalogReceived;
    
    public ClientHandler(Config config, ClientMessageAnalyser messageAnalyser, DownloadHandler downloadHandler)
    {
        _messageAnalyser = messageAnalyser;
        _downloadHandler = downloadHandler;
        _serverCertificate = new X509Certificate2(config.CertificatePath, config.CertificatePassword);
    }

    public async Task HandleClientAsync(TcpClient tcpClient)
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
                        Console.WriteLine("Connexion avec le client fermée.");
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
                    else if (_protocol.ParseSayonara(message))
                    {
                        var activeClient = _activeClients.FirstOrDefault(c => c.TcpClient == tcpClient);
                        if (activeClient != null)
                        {
                            // Fermer la connexion du client
                            activeClient.TcpClient.Close();
                            _activeClients.Remove(activeClient);
                            Console.WriteLine($"Client {activeClient.Username} déconnecté proprement.");
                        }
                        else
                        {
                            Console.WriteLine("Client non trouvé dans la liste des clients actifs.");
                        }
                        break; // Sortir de la boucle pour arrêter de traiter les messages de ce client
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
                                byte[] responseByte;
                                string headerByte;

                                if (containerResult == "GET_OK")
                                {
                                    (headerByte, responseByte) = _downloadHandler.HandleDownload(taskItem);
                                    if (responseByte == null)
                                    {
                                        response = _protocol.BuildDownloadError();
                                        await SendResponseAsync(sslStream, response);
                                    }
                                    else
                                    {
                                        await SendFileAsync(sslStream, headerByte, responseByte);
                                    }
                                }
                                else
                                {
                                    response = containerResult switch
                                                                    {
                                                                        "POST_OK" => _protocol.BuildSendOk(),
                                                                        "POST_ERROR" => _protocol.BuildSendError(),
                                                                        "DELETE_OK" => _protocol.BuildPurgeOk(),
                                                                        "DELETE_ERROR" => _protocol.BuildPurgeError(),
                                                                        "GET_ERROR" => _protocol.BuildDownloadError(),
                                                                        _ => "ERROR"
                                                                    };
                                }
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
        await sslStream.WriteAsync(responseBytes);
    }

    private async Task SendFileAsync(SslStream sslStream, string header, byte[] response)
    {
        byte[] headerBytes = Encoding.UTF8.GetBytes(header);
        await sslStream.WriteAsync(headerBytes);
        await sslStream.WriteAsync(response);
    }
}