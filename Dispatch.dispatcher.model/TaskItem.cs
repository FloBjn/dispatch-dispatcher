namespace Dispatch.dispatcher.model;

public class TaskItem
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
    public TaskType Type { get; set; }

    /// <summary>
    /// Nom original du fichier concerné par la tâche.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Commande associée à la tâche (disp_post, disp_delete, disp_get).
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    /// Utilisateur ayant initié la tâche.
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// Identifiant du Container concerné par la tâche.
    /// </summary>
    public IList<string> ContainerId { get; set; }

    /// <summary>
    /// Statut actuel de la tâche (en cours, terminée, erreur).
    /// </summary>
    public TaskStatus Status { get; set; }
}