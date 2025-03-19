using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Dispatch.dispatcher.model;

/// <summary>
/// Écouteur multicast pour détecter les Containers envoyant des messages ALIVE.
/// </summary>
public class MulticastListener
{
    private readonly string _multicastAdr;
    private readonly int _multicastPort;
    private readonly ContainerManager _containerManager;

    public MulticastListener(Config config, ContainerManager containerManager)
    {
        _multicastAdr = config.MulticastAddress;
        _multicastPort = config.MulticastPort;
        _containerManager = containerManager;
    }

    /// <summary>
    /// Démarre l'écoute des messages multicast envoyés par les Containers.
    /// </summary>
    public void StartListening()
    {
        Thread multicastThread = new Thread(async () =>
        {
            using UdpClient udpClient = new UdpClient();
            udpClient.ExclusiveAddressUse = false;
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, _multicastPort);
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(localEndPoint);
            udpClient.JoinMulticastGroup(IPAddress.Parse(_multicastAdr));

            Console.WriteLine($"Écoute des Containers sur {_multicastAdr}:{_multicastPort}...");

            while (true)
            {
                try
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync();
                    string message = Encoding.UTF8.GetString(result.Buffer);

                    Console.WriteLine($"Message reçu : {message}");

                    if (DispatcherProtocol.ParseAlive(message) is (string containerName, int tcpPort))
                    {
                        string containerIp = result.RemoteEndPoint.Address.ToString();
                        Console.WriteLine($"Nouveau Container détecté : {containerName} ({containerIp}:{tcpPort})");
                        _containerManager.AddOrUpdateContainer(containerName, containerIp, tcpPort);
                    }
                    else
                    {
                        Console.WriteLine($"Message ignoré (format invalide) : {message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur dans l'écoute multicast : {ex.Message}");
                }
            }
        })
        {
            IsBackground = true
        };
        multicastThread.Start();
    }
}