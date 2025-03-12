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
        // Gestion des fichiers 
        IFileManager fileManager = new FileManager(new FileEncryptor());

        // Gestion du fichier de config
        const string configPath = "config.json";

        Config config = Config.Load(configPath);

        config.Save(configPath);
        // Gestion des fichiers 
        IFileManager fileManager = new FileManager(new FileEncryptor());

        // Gestion des utilisateurs
        ClientMessageAnalyser analyser = new ClientMessageAnalyser(new UserManager(config), fileManager);

        
        // Serveur TCP qui écoute les clients
        var server = new TcpServer(config);

        // Interface utilisateur
        IMainView view = new View();
        var presenter = new Presenter(view, server, analyser, fileManager);

        // Démarrer l'écoute multicast 
        MulticastListener multicastListener = new MulticastListener(config);
        ClientMessageAnalyser analyser = new ClientMessageAnalyser(new UserManager(config), fileManager);
        
        var server = new TcpServer(config);
        var presenter = new Presenter(view, server, analyser, fileManager);
        MulticastListener multicastListener = new MulticastListener(config);
        multicastListener.StartListening();
        
        // Démarrer le serveur TCP en mode bloquant (reste dans la boucle d'écoute)
        presenter.StartAsync().Wait();
    }
}