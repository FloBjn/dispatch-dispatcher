using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Dispatch.dispatcher.model;

public class MulticastListener
{
    private readonly string _multicastAdr;
    private readonly int _multicastPort;
    private readonly string _networkInterfaceName;
    private readonly ContainerManager _containerManager;

    public MulticastListener(Config config, ContainerManager containerManager)
    {
        _multicastAdr = config.MulticastAddress;
        _multicastPort = config.MulticastPort;
        _networkInterfaceName = config.NetworkInterface; 
        _containerManager = containerManager;
    }

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
                    Console.WriteLine($"✅ Interface {nic.Name} ({ip}) rejointe au groupe multicast.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ Impossible de joindre le groupe multicast via {nic.Name} ({ip}) : {ex.Message}");
                }
            }

           
            Console.WriteLine($"Écoute des Containers sur {_multicastAdr}:{_multicastPort} via {_networkInterfaceName}...");

            while (true)
            {
                try
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync();
                    
                    string message = Encoding.UTF8.GetString(result.Buffer);
                    Console.WriteLine($"📡 Reçu depuis {result.RemoteEndPoint}: {message}");

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
                    Console.WriteLine($"⚠Erreur dans l'écoute multicast : {ex.Message}");
                }
            }
        })
        {
            IsBackground = true
        };
        multicastThread.Start();
    }

    /// <summary>
    /// 🔍 Trouve une interface réseau spécifique en fonction de son nom.
    /// </summary>
    private static NetworkInterface? GetNetworkInterface(string interfaceName)
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(nic => nic.Name.Equals(interfaceName, StringComparison.OrdinalIgnoreCase));
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