using System.Net;
using System.Net.NetworkInformation;
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
    private readonly string _networkInterfaceName;
    private readonly ContainerManager _containerManager;
    private readonly TcpContainer _tcpContainer;
    
    private readonly Protocol _protocol = Protocol.GetInstance();

    public MulticastListener(Config config, ContainerManager containerManager, TcpContainer tcpContainer)
    {
        _multicastAdr = config.MulticastAddress;
        _multicastPort = config.MulticastPort;
        _networkInterfaceName = config.NetworkInterface;
        _containerManager = containerManager;
        _tcpContainer = tcpContainer;
    }

    public Func<string, Task> OnMessageContainerReceived { get; set; }

    /// <summary>
    /// Démarre l'écoute des messages multicast envoyés par les Containers.
    /// </summary>
    public void StartListening()
    {
        Thread multicastThread = new Thread(async () =>
        {
            using UdpClient udpClient = new UdpClient
            {
                ExclusiveAddressUse = false
            };

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, _multicastPort);
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(localEndPoint);

            var selectedNic = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n => n.Name == _networkInterfaceName && 
                                     n.OperationalStatus == OperationalStatus.Up && 
                                     n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            if (selectedNic == null)
            {
                Console.WriteLine($"Interface réseau non trouvée : {_networkInterfaceName}");
                return;
            }

            var localIp = GetLocalIPAddress(selectedNic);
            if (localIp == null)
            {
                Console.WriteLine($"Impossible d'obtenir l'adresse IP de l'interface {_networkInterfaceName}");
                return;
            }

            udpClient.JoinMulticastGroup(IPAddress.Parse(_multicastAdr), localIp);

            while (true)
            {
                try
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync();
                    string message = Encoding.UTF8.GetString(result.Buffer);

                    if (OnMessageContainerReceived != null)
                        await OnMessageContainerReceived.Invoke(message);

                    if (_protocol.ParseAlive(message) is (string containerName, int tcpPort))
                    {
                        string containerIp = result.RemoteEndPoint.Address.ToString();
                        Console.WriteLine($"Nouveau Container détecté : {containerName} ({containerIp}:{tcpPort})");

                        _containerManager.AddOrUpdateContainer(containerName, containerIp, tcpPort);
                        _tcpContainer.AddOrUpdateConnection(containerName, containerIp, tcpPort);
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

    private static IPAddress? GetLocalIPAddress(NetworkInterface nic)
    {
        return nic.GetIPProperties().UnicastAddresses
            .Select(ip => ip.Address)
            .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
    }
}
