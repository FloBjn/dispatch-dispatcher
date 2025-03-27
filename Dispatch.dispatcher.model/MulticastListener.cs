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
            using UdpClient udpClient = new UdpClient();

            udpClient.ExclusiveAddressUse = false;

            // Lier le socket au port multicast
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, _multicastPort);
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(localEndPoint);

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up ||
                    nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                var ip = GetLocalIPAddress(nic);
                if (ip == null) continue;

                try
                {
                    udpClient.JoinMulticastGroup(IPAddress.Parse(_multicastAdr), ip);
                    Console.WriteLine($"Interface {nic.Name} ({ip}) rejointe au groupe multicast.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Impossible de joindre le groupe multicast via {nic.Name} ({ip}) : {ex.Message}");
                }
            }

           
            Console.WriteLine($"Écoute des Containers sur {_multicastAdr}:{_multicastPort} via {_networkInterfaceName}...");

            while (true)
            {
                try
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync();
                    
                    string message = Encoding.UTF8.GetString(result.Buffer);
                    
                    if (OnMessageContainerReceived != null)
                    {
                        await OnMessageContainerReceived.Invoke(message);
                    }


                    if (DispatcherProtocol.ParseAlive(message) is (string containerName, int tcpPort))
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

    /// <summary>
    /// 🔍 Récupère l'adresse IP locale associée à une interface réseau.
    /// </summary>
    private static IPAddress? GetLocalIPAddress(NetworkInterface nic)
    {
        return nic.GetIPProperties().UnicastAddresses
            .Select(ip => ip.Address)
            .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
    }
}