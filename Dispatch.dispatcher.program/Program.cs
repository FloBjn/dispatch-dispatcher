using System.Net.NetworkInformation;
using Dispatch.disapatcher.model;
using Dispatch.dispatcher.model;
using Dispatch.dispatcher.infrastructure;
using Dispatch.dispatcher.presenter;
using Dispatch.dispatcher.view;

namespace Dispatch.dispatcher.app;

public class Program
{
    public static void Main()
    {
        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            Console.WriteLine($"Name: {nic.Name}");
            Console.WriteLine($"Description: {nic.Description}");
            Console.WriteLine($"ID: {nic.Id}");
            Console.WriteLine($"Type: {nic.NetworkInterfaceType}");
            Console.WriteLine($"Operational: {nic.OperationalStatus}");
            Console.WriteLine(new string('-', 40));
        }
        // Gestion du fichier de config
        const string configPath = "config.json";

        Config config = Config.Load(configPath);

        config.Save(configPath);
        // Gestion des Containers
        ContainerManager containerManager = new ContainerManager();
        containerManager.StartCleanupTask();
        
        //TCP pour communiquer avec le container
        var tcpContainer = new TcpContainer(containerManager, new UnicastConnection());
        
        // Gestion des fichiers 
        IFileManager fileManager = new FileManager(new FileEncryptor(), tcpContainer, new FileHasher(), containerManager);
        
        // Gestion des utilisateurs
        ClientMessageAnalyser analyser = new ClientMessageAnalyser(new UserManager(config), fileManager);
        
        // Serveur TCP qui écoute les clients
        var server = new TcpServer(config);

        // Interface utilisateur
        IMainView view = new View();
        var presenter = new Presenter(view, server, analyser, fileManager);

        // Démarrer l'écoute multicast 
        MulticastListener multicastListener = new MulticastListener(config, containerManager);
        multicastListener.StartListening();
        
        // Démarrer le serveur TCP en mode bloquant (reste dans la boucle d'écoute)
        presenter.StartAsync().Wait();
    }
}