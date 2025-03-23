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
        
        MulticastListener multicastListener = new MulticastListener(config, containerManager);
        
        // Interface utilisateur
        IMainView view = new View();
        var presenter = new Presenter(view, server, multicastListener, analyser);
        
        // Démarrage du programme
        presenter.StartAsync().Wait();
    }
}