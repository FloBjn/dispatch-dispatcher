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
    
    public event Func<TaskItem, Task<string>>? OnTaskReceived;    
    
    public event Func<string, string, Task<string>> OnLoginReceived;
    
    public event Func<string, string, Task<string>> OnRegisterReceived;

    public TcpServer(Config config, ClientMessageAnalyser messageAnalyser)
    {
        _config = config;
        _serverCertificate = new X509Certificate2(config.CertificatePath, config.CertificatePassword);
        _messageAnalyser = messageAnalyser;
    }

    /// <summary>
    /// Démarre le serveur TLS et écoute les connexions entrantes.
    /// </summary>
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

    /// <summary>
    /// Gère une connexion client via TLS.
    /// </summary>
    /// <param name="client"></param>
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
                    var message = await ReceiveMessageAsync(sslStream);
                    
                    if (message == null)
                    {
                        Console.WriteLine("Client déconnecté.");
                        break;
                    }

                    Console.WriteLine($"Message reçu : {message}");

                    var response = "";
                    
                    if (DispatcherProtocol.ParseLogin(message) is (string username, string password))
                    {
                        response = await OnLoginReceived.Invoke(username, password);
                        
                        if (response == DispatcherProtocol.BuildLoginOk())
                        {
                            // TODO: Retenir le client connecté
                            var client = new Client(tcpClient, username, false);
                        }
                        
                    } else if (DispatcherProtocol.ParseRegister(message) is (string newUser, string newPassword))
                    {
                        response = await OnRegisterReceived.Invoke(newUser, newPassword);
                        
                        if (response == DispatcherProtocol.BuildLoginOk())
                        {
                            // TODO: Retenir le client connecté
                            var client = new Client(tcpClient, newUser, false);
                            _activeClients.Add(client);
                        }
                    }
                    else if (DispatcherProtocol.ParseCatalogue(message) != null)
                    {
                        // TODO: Gérer le catalogue
                        var activeClient = _activeClients.FirstOrDefault(c => c.TcpClient == tcpClient);
                            
                        var taskItem = _messageAnalyser.AnalyseMessage(message, activeClient);

                        var storedFiles = taskItem.CatalogFiles;

                        string[] filesNames = storedFiles.Select(o => o.FileName).ToArray();
                        int[] filesSize = storedFiles.Select(o => o.Size).ToArray();

                        response = DispatcherProtocol.BuildCatalogue(filesNames, filesSize);
                    }
                    else
                    {
                        // TODO: Voir si le client est connecté
                        var activeClient = _activeClients.FirstOrDefault(c => c.TcpClient == tcpClient);

                        if (activeClient == null)
                        {
                            response = DispatcherProtocol.BuildLoginError();
                        }
                        else
                        {
                            var taskItem = _messageAnalyser.AnalyseMessage(message, activeClient);

                            if (taskItem != null && OnTaskReceived != null)
                            {
                                response = await OnTaskReceived.Invoke(taskItem);
                                await SendResponseAsync(sslStream, response);
                            }
                        }
                    }
                    
                    // Renvoyer la réponse au client
                    await SendResponseAsync(sslStream, response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur TLS : {ex.Message}");
            }
        }
    }


    /// <summary>
    /// Lit un message sécurisé du client.
    /// </summary>
    private async Task<string?> ReceiveMessageAsync(SslStream sslStream)
    {
        byte[] buffer = new byte[1024];
        int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
        
        if (bytesRead == 0)
        {
            return null;
        }

        return Encoding.UTF8.GetString(buffer, 0, bytesRead);
    }

    /// <summary>
    /// Envoie une réponse sécurisée au client.
    /// </summary>
    private async Task SendResponseAsync(SslStream sslStream, string response)
    {
        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        await sslStream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }

    /// <summary>
    /// Stoppe le serveur.
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _tcpListener?.Stop();
        Console.WriteLine("Serveur TLS arrêté.");
    }
}