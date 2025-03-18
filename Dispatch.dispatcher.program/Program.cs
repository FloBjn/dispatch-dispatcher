using System;
using System.Threading;
using Dispatch.disapatcher.model;
using Dispatch.dispatcher.infrastructure;
using Dispatch.dispatcher.presenter;
using Dispatch.dispatcher.view;
using Dispatch.dispatcher.network;

namespace Dispatch.dispatcher.app;

public class Program
{
    public static void Main()
    {
        // Gestion des Containers
        ContainerManager containerManager = new ContainerManager();
        
        //TCP pour communiquer avec le container
        var tcpContainer = new TcpContainer(containerManager, new UnicastConnection());
        
        // Gestion des fichiers 
        IFileManager fileManager = new FileManager(new FileEncryptor(), tcpContainer, new FileHasher());
        
        // Gestion des utilisateurs
        ClientMessageAnalyser analyser = new ClientMessageAnalyser(new UserManager(), fileManager);

        // Serveur TCP qui écoute les clients
        var server = new TcpServer(12345);

        // Interface utilisateur
        IMainView view = new View();
        var presenter = new Presenter(view, server, analyser, fileManager);

        // Démarrer l'écoute multicast 
        MulticastListener multicastListener = new MulticastListener(containerManager);
        multicastListener.StartListening();
        
        // Démarrer le serveur TCP en mode bloquant (reste dans la boucle d'écoute)
        presenter.StartAsync().Wait();
    }
}