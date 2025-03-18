using Dispatch.dispatcher.model;
using Dispatch.dispatcher.infrastructure;
using Dispatch.dispatcher.presenter;
using Dispatch.dispatcher.view;
using Dispatch.dispatcher.network;

namespace Dispatch.dispatcher.app;

public class Program
{
    public static void Main()
    {
        // Gestion du fichier de config
        const string configPath = "config.json";

        Config config = Config.Load(configPath);

        config.Save(configPath);
        // Gestion des Containers
        ContainerManager containerManager = new ContainerManager();
        
        //TCP pour communiquer avec le container
        var tcpContainer = new TcpContainer(containerManager, new UnicastConnection());
        
        // Gestion des fichiers 
        IFileManager fileManager = new FileManager(new FileEncryptor(), tcpContainer, new FileHasher());
        
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