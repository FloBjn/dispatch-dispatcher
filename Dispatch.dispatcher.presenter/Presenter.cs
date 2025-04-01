using System.Collections.ObjectModel;
using Dispatch.dispatcher.model;

namespace Dispatch.dispatcher.presenter;

public class Presenter
{
    private readonly IMainView _view;
    private readonly TcpServer _unicastServer;
    private readonly MulticastListener _multicastListener;
    private readonly IUserManager _userManager;
    private readonly TaskManager _taskManager;
    private readonly IFileCatalogManager _fileCatalogManager;

    public Presenter(IMainView view, TcpServer unicastServer,
        MulticastListener multicastListener, IUserManager userManager, TaskManager taskManager, IFileCatalogManager fileCatalogManager)
    {
        _view = view;
        _unicastServer = unicastServer;
        _multicastListener = multicastListener;
        _userManager = userManager;
        _taskManager = taskManager;
        _fileCatalogManager = fileCatalogManager;

        _multicastListener.OnMessageContainerReceived += ShowMessageContainer;
        _unicastServer.OnTaskReceived += AnalyseTaskClient;
        _unicastServer.OnLoginReceived += LoginClient;
        _unicastServer.OnRegisterReceived += RegisterClient;
        _unicastServer.OnCatalogReceived += CatalogClient;
    }


    /// <summary>
    /// Démarre le serveur unicast
    /// </summary>
    public async Task StartAsync(CancellationToken token)
    {
        _taskManager.Start(token);
        await _unicastServer.StartAsync();
    }

    private Task<string> RegisterClient(string userName, string password)
    {
        if (_userManager.VerifyUser(userName, password) != null)
        {
            return Task.FromResult(DispatcherProtocol.BuildLoginError());
        }

        _userManager.AddUser(new User
            { Username = userName, PasswordHash = password, AesKey = AesKeyGenerator.GenerateAesKey() });
        return Task.FromResult(DispatcherProtocol.BuildLoginOk());
    }

    private Task<string> LoginClient(string userName, string password)
    {
        var user = _userManager.VerifyUser(userName, password);
        return user != null
            ? Task.FromResult(DispatcherProtocol.BuildLoginOk())
            : Task.FromResult(DispatcherProtocol.BuildLoginError());
    }

    private Task<string> CatalogClient(string currentUser)
    {
        var catalog = _fileCatalogManager.LoadCatalog(currentUser);
        Console.WriteLine("Catalogue lu.");
        
        var filesNames = catalog.Select(o => o.FileName).ToArray();
        var filesSize = catalog.Select(o => o.Size).ToArray();
        
        Console.WriteLine(filesNames.ToString());
        
        return Task.FromResult(DispatcherProtocol.BuildCatalogue(filesNames, filesSize));
    }

    private Task<string> AnalyseTaskClient(TaskItem task)
    {
        _view.DisplayMainThreadMessage($"Tâche reçue : {task.Type} - {task.FileName}");
        var taskResult = _taskManager.AddTask(task);
        _view.DisplayMainThreadMessage($"Tâche ajoutée à la file : {task.Type} - {task.FileName}");
        return taskResult;
    }


    /// <summary>
    /// Montre le message reçu dans le container
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private Task ShowMessageContainer(string message)
    {
        _view.DisplayMessageReceivedContainer(message);
        return Task.FromResult("Message reçu");
    }
}