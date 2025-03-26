namespace Dispatch.dispatcher.model;

public class TaskItem(
    TaskItem.TaskType type,
    string fileName,
    string command,
    string userName,
    IList<string> containerId)
{
    public enum TaskType
    {
        Send,
        Receive,
        Delete
    }

    public enum TaskStatus
    {
        InProgress,
        Completed,
        Error
    }

    /// <summary>
    /// Type de la tâche (envoi, réception, suppression d'un fichier).
    /// </summary>
    public TaskType Type { get; set; } = type;

    /// <summary>
    /// Nom original du fichier concerné par la tâche.
    /// </summary>
    public string FileName { get; set; } = fileName;

    /// <summary>
    /// Commande associée à la tâche (disp_post, disp_delete, disp_get).
    /// </summary>
    public string Command { get; set; } = command;

    /// <summary>
    /// Utilisateur ayant initié la tâche.
    /// </summary>
    public string UserName { get; set; } = userName;

    /// <summary>
    /// Identifiant du Container concerné par la tâche.
    /// </summary>
    public IList<string> ContainerId { get; set; } = containerId;

    /// <summary>
    /// Statut actuel de la tâche (en cours, terminée, erreur).
    /// </summary>
    public TaskStatus Status { get; set; } = TaskStatus.InProgress;
}