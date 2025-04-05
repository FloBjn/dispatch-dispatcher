using System.Threading.Tasks;

namespace Dispatch.dispatcher.model;

public class SayonaraMessageHandler : IMessageHandler
{
    private readonly Protocol _protocol = Protocol.GetInstance();

    public bool CanHandle(string message)
    {
        return _protocol.ParseSayonara(message);
    }

    public async Task<string?> HandleAsync(ClientHandlerContext context, string message)
    {
        context.TcpClient.Close();

        if (context.Client != null)
        {
            Console.WriteLine($"Client {context.Client.Username} déconnecté.");
        }
        else
        {
            Console.WriteLine("Client inconnu a fermé la connexion.");
        }

        return null; 
    }
}