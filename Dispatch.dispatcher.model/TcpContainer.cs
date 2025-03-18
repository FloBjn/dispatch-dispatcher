using Dispatch.dispatcher.network;

namespace Dispatch.disapatcher.model;

/// <summary>
/// Classe faisant le lien unicast du Dispatcher vers les containers de manière individuel
/// </summary>
public class TcpContainer
{
    private ContainerManager _containerManager;
    private UnicastConnection _connection;

    public TcpContainer(ContainerManager containerManager, UnicastConnection connection)
    {
        _containerManager = containerManager;
        _connection = connection;
    }

    /// <summary>
    /// Méthode temporaire envoyant le message au premier container de la liste
    /// </summary>
    /// <param name="message"></param>
    public void SendMessageToFirst(string message)
    {
        var containerIp = _containerManager.GetFirstContainerIp();
        SendMessage(containerIp.Item1, containerIp.Item2, message);
    }
    /// <summary>
    /// Méthode envoyant un message à un container sur base de son ip et de son port
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="message"></param>
    private void SendMessage(string ip, int port, string message)
    {
        _connection.Connect(ip, port);
        _connection.SendMessage(message);
        _connection.Disconnect();
    }
}