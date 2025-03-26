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

    public Presenter(IMainView view, TcpServer unicastServer,
        MulticastListener multicastListener, IUserManager userManager, TaskManager taskManager)
    {
        _view = view;
        _unicastServer = unicastServer;
        _multicastListener = multicastListener;
        _userManager = userManager;
        _taskManager = taskManager;

        _multicastListener.OnMessageContainerReceived += ShowMessageContainer;
        _unicastServer.OnTaskReceived += AnalyseTaskClient;
        _unicastServer.OnLoginReceived += LoginClient;
        _unicastServer.OnRegisterReceived += RegisterClient;
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

        _userManager.AddUser(new User {Username = userName, PasswordHash = password, AesKey = AesKeyGenerator.GenerateAesKey()});
        return Task.FromResult(DispatcherProtocol.BuildLoginOk());
    }

    private Task<string> LoginClient(string userName, string password)
    {
        var user = _userManager.VerifyUser(userName, password);
        return user != null ? Task.FromResult(DispatcherProtocol.BuildLoginOk()) : Task.FromResult(DispatcherProtocol.BuildLoginError());
    }

    private Task<string> AnalyseTaskClient(TaskItem task)
    {
        _taskManager.AddTask(task);
        _view.DisplayMessageReceivedClient($"Tâche ajoutée à la file : {task.Type} - {task.FileName}");
        return Task.FromResult("TASK_RECEIVED");
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