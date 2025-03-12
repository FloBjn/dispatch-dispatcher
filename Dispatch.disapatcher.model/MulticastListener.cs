using System.Net;
using System.Net.Sockets;
using System.Text;
using Dispatch.disapatcher.model;

namespace Dispatch.dispatcher.network;

/// <summary>
/// Écouteur multicast pour détecter les Containers envoyant des messages ALIVE.
/// </summary>
public class MulticastListener
{
    private const string MULTICAST_ADDR = "224.123.123.123";
    private const int MULTICAST_PORT = 50204;
    
    public ContainerManager ContainerManager { get; set; }

    public MulticastListener(ContainerManager containerManager)
    {
        ContainerManager = containerManager;
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
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, MULTICAST_PORT);
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(localEndPoint);

            // Rejoindre le groupe multicast, ce qui permet au socket de recevoir les messages envoyé sur l'adresse IP 224.123.123.123
            udpClient.JoinMulticastGroup(IPAddress.Parse(MULTICAST_ADDR));

            Console.WriteLine($"écoute des Containers sur {MULTICAST_ADDR}:{MULTICAST_PORT}...");

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