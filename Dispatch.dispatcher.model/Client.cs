using System.Net.Sockets;

namespace Dispatch.dispatcher.model;

public class Client(TcpClient tcpClient, string username, bool awaitingUploadContent)
{
    public TcpClient TcpClient { get; set; } = tcpClient;
    public string Username { get; set; } = username;
    public bool AwaitingUploadContent { get; set; } = awaitingUploadContent;

    public OurFile? CurrentFile { get; set; }
}