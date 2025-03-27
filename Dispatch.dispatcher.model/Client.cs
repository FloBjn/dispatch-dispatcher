using System.Net.Sockets;

namespace Dispatch.dispatcher.model;

public class Client
{

    public Client(TcpClient tcpClient, string username, bool awaitingUploadContent)
    {
        TcpClient = tcpClient;
        Username = username;
        AwaitingUploadContent = awaitingUploadContent;
    }

    public TcpClient TcpClient { get; set; }
    public string Username { get; set; }
    public bool AwaitingUploadContent { get; set; }
    
    public OurFile? CurrentFile { get; set; }
}