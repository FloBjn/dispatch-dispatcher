using System.Collections.Concurrent;

namespace Dispatch.dispatcher.model;

/// <summary>
/// Gère la file de tâches et le thread principal qui les délègue aux bons exécuteurs.
/// </summary>
public class TaskManager(ITaskDispatcher dispatcher)
{
    private readonly BlockingCollection<TaskItem> _taskQueue = new();

    /// <summary>
    /// Lance le thread principal de traitement des tâches.
    /// </summary>
    public void Start(CancellationToken token)
    {
        Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var task = _taskQueue.Take(token);
                    dispatcher.Dispatch(task);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MainThread] Erreur : {ex.Message}");
                }
            }
        }, token);
    }

    /// <summary>
    /// Ajoute une tâche à la file d'attente.
    /// </summary>
    public Task<string> AddTask(TaskItem task)
    {
        var tcs = new TaskCompletionSource<string>();
        task.CompletionSource = tcs;

        _taskQueue.Add(task);
        return tcs.Task; // Tu pourras await ça dans TcpServer
    }

} 