namespace Dispatch.dispatcher.model;

public class TaskDispatcher : ITaskDispatcher
{
    private readonly IFileManager _fileManager;
    private readonly FileSender _fileSender;
    public TaskDispatcher(IFileManager fileManager, FileSender fileSender)
    {
        _fileManager = fileManager;
        _fileSender = fileSender;
    }

    public void Dispatch(TaskItem task)
    {
        try
        {
            switch (task.Type)
            {
                case TaskItem.TaskType.Send:
                    HandleUpload(task);
                    break;

                case TaskItem.TaskType.Receive:
                    HandleDownload(task);
                    break;

                case TaskItem.TaskType.Delete:
                    HandleDelete(task);
                    break;

                default:
                    Console.WriteLine($"[Dispatcher] Type de tâche non reconnu : {task.Type}");
                    break;
            }

            task.Status = TaskItem.TaskStatus.Completed;
            Console.WriteLine($"[Dispatcher] Tâche {task.Type} terminée avec succès !");
        }
        catch (Exception ex)
        {
            task.Status = TaskItem.TaskStatus.Error;
            Console.WriteLine($"[Dispatcher] Erreur pendant la tâche : {ex.Message}");
        }
    }

    private void HandleUpload(TaskItem task)
    {
        Console.WriteLine($"[Dispatcher] Upload : Envoi vers Container...");
        _fileSender.SendFile(task);
        _fileManager.SaveFile(task);
    }

    private void HandleDownload(TaskItem task)
    {
        Console.WriteLine($"[Dispatcher] Download : TODO - à implémenter.");
        // TODO : Interroger le container pour récupérer le fichier
    }

    private void HandleDelete(TaskItem task)
    {
        Console.WriteLine($"[Dispatcher] Delete : TODO - à implémenter.");
        // TODO : Envoyer une commande DELETE au container
    }
}