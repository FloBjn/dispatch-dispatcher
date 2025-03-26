using System.Collections.Concurrent;

namespace Dispatch.dispatcher.model;

/// <summary>
/// Gère la file de tâches et le thread principal qui les délègue aux bons exécuteurs.
/// </summary>
public class TaskManager
{
    private readonly BlockingCollection<TaskItem> _taskQueue = new();
    private readonly ITaskDispatcher _dispatcher;

    public TaskManager(ITaskDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

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
                    _dispatcher.Dispatch(task);
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
    public void AddTask(TaskItem task)
    {
        _taskQueue.Add(task);
    }
} 