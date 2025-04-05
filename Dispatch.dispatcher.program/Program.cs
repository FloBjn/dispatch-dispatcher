using System.Net.NetworkInformation;
using Dispatch.dispatcher.model;
using Dispatch.dispatcher.infrastructure;
using Dispatch.dispatcher.presenter;
using Dispatch.dispatcher.view;

namespace Dispatch.dispatcher.app;

public class Program
{
    public static void Main()
    {
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
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
        var tcpContainer = new TcpContainer();
        
        // Gestion des fichiers 
        IFileManager fileManager = new FileManager(new FileEncryptor());
        
        // Gestion du catalogue
        IFileCatalogManager fileCatalogManager = new FileCatalogManager(config);
        
        // Gestion des utilisateurs
        var analyser = new ClientMessageAnalyser(new UserManager(config), fileManager, containerManager);
        
        // Gestion des tasks
        var dispatcher = new TaskDispatcher(fileManager, new FileSender(tcpContainer));
        var taskManager = new TaskManager(dispatcher);
        
        // Serveur TCP qui écoute les clients
        var server = new TcpServer(config, analyser, new DownloadHandler(fileManager));
        
        var multicastListener = new MulticastListener(config, containerManager, tcpContainer);
        
        multicastListener.StartListening();
        
        
        // Interface utilisateur
        IMainView view = new View();
        var presenter = new Presenter(view, server, multicastListener, new UserManager(config), taskManager, fileCatalogManager);
        var cts = new CancellationTokenSource();

        // Démarrage du programme
        presenter.StartAsync(cts.Token).Wait();
    }
}