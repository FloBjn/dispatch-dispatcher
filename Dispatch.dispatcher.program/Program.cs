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
        
        // Gestion des fichiers 
        IFileManager fileManager = new FileManager(new FileEncryptor());
        
        // Gestion des utilisateurs
        ClientMessageAnalyser analyser = new ClientMessageAnalyser(new UserManager(), fileManager);

        // Serveur TCP qui écoute les clients
        var server = new TcpServer(1234);

        // Interface utilisateur
        IMainView view = new View();
        var presenter = new Presenter(view, server, analyser, fileManager);

        // Démarrer l'écoute multicast 
        MulticastListener multicastListener = new MulticastListener();
        multicastListener.StartListening();
        
        // Démarrer le serveur TCP en mode bloquant (reste dans la boucle d'écoute)
        presenter.StartAsync().Wait();
    }
}