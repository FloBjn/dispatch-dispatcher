using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Dispatch.dispatcher.model;

public class ClientHandler
{
    private readonly List<IMessageHandler> _handlers = new();
    private readonly ClientMessageAnalyser _messageAnalyser;
    private readonly DownloadHandler _downloadHandler;
    private readonly X509Certificate2 _serverCertificate;

    private const int ByteBufferSize = 1024;

    public event Func<TaskItem, Task<string>>? OnTaskReceived;
    public event Func<string, string, Task<string>> OnLoginReceived;
    public event Func<string, string, Task<string>> OnRegisterReceived;
    public event Func<string, Task<string>>? OnCatalogReceived;

    public ClientHandler(ClientMessageAnalyser messageAnalyser, DownloadHandler downloadHandler,
        X509Certificate2 serverCertificate)
    {
        _messageAnalyser = messageAnalyser;
        _downloadHandler = downloadHandler;
        _serverCertificate = serverCertificate;

        _handlers.Add(new LoginMessageHandler());
        _handlers.Add(new RegisterMessageHandler());
        _handlers.Add(new CatalogMessageHandler());
        _handlers.Add(new SayonaraMessageHandler());
        _handlers.Add(new TaskMessageHandler());
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
                    ServerCertificate = _serverCertificate,
                    EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls13
                };

                await sslStream.AuthenticateAsServerAsync(options);

                var context = new ClientHandlerContext(
                    tcpClient,
                    sslStream,
                    OnLoginReceived,
                    OnRegisterReceived,
                    OnCatalogReceived,
                    OnTaskReceived,
                    _messageAnalyser,
                    _downloadHandler
                );

                while (tcpClient.Connected)
                {
                    byte[] buffer = new byte[ByteBufferSize];
                    int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Connexion avec le client fermée.");
                        break;
                    }

                    string? response = null;
                    bool handled = false;

                    if (context.Client?.AwaitingUploadContent == true)
                    {
                        foreach (var handler in _handlers)
                        {
                            if (handler is TaskMessageHandler taskHandler)
                            {
                                response = await taskHandler.HandleBinaryAsync(context, buffer, bytesRead);
                                handled = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        foreach (var handler in _handlers)
                        {
                            if (handler.CanHandle(message))
                            {
                                response = await handler.HandleAsync(context, message);
                                handled = true;
                                break;
                            }
                        }

                        if (!handled)
                        {
                            Console.WriteLine($"[UNHANDLED MESSAGE] : {message}");
                            response = "COMMAND_NOT_SUPPORTED\r\n";
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

    public static async Task SendFileAsync(SslStream sslStream, string header, byte[] response)
    {
        byte[] headerBytes = Encoding.UTF8.GetBytes(header);
        await sslStream.WriteAsync(headerBytes);
        await sslStream.WriteAsync(response);
    }
}