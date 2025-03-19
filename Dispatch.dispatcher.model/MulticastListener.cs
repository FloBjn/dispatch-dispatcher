using System.Net;
using System.Net.Sockets;
using System.Text;
using Dispatch.dispatcher.model;

namespace Dispatch.dispatcher.network;

/// <summary>
/// Écouteur multicast pour détecter les Containers envoyant des messages ALIVE.
/// </summary>
public class MulticastListener
{
    private string _multicastAdr;
    private int _multicastPort;

    public MulticastListener(Config config)
    {
        _multicastAdr = config.MulticastAddress;
        _multicastPort = config.MulticastPort;
    }

    /// <summary>
    /// Démarre l'écoute des messages multicast envoyés par les Containers.
    /// </summary>
    public void StartListening()
    {
        Thread multicastThread = new Thread(async () =>
        {
            // Création d'un socket UDP pour écouter les messages multicast
            using UdpClient udpClient = new UdpClient();
            udpClient.ExclusiveAddressUse = false;

            // Lier le socket au port multicast, en acceptant les messages réseau envoyé sur n'importe quelle interface réseau.
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, _multicastPort);
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(localEndPoint);

            // Rejoindre le groupe multicast, ce qui permet au socket de recevoir les messages envoyé sur l'adresse IP 224.123.123.123
            udpClient.JoinMulticastGroup(IPAddress.Parse(_multicastAdr));

            Console.WriteLine($"écoute des Containers sur {_multicastAdr}:{_multicastPort}...");

            while (true)
            {
                try
                {
                    // Attente de messages multicast
                    UdpReceiveResult result = await udpClient.ReceiveAsync();
                    string message = Encoding.UTF8.GetString(result.Buffer);

                    Console.WriteLine($"Message reçu : {message}");

                    // Vérification du message ALIVE avec le protocole
                    if (DispatcherProtocol.ParseAlive(message) is (string containerName, int tcpPort))
                    {
                        string containerIp = result.RemoteEndPoint.Address.ToString();
                        Console.WriteLine($"Nouveau Container détecté : {containerName} ({containerIp}:{tcpPort})");

                        // Ajouter le Container détecté dans la liste
                        ContainerManager.AddContainer(containerName, containerIp, tcpPort);
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
        });
        multicastThread.IsBackground = true;
        multicastThread.Start();
    }
}