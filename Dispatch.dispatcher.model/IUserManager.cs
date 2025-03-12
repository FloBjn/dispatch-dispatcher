namespace Dispatch.dispatcher.model
{
    /// <summary>
    /// Interface définissant les opérations de gestion des utilisateurs.
    /// </summary>
    public interface IUserManager
    {
        /// <summary>
        /// Ajoute un nouvel utilisateur au système.
        /// </summary>
        /// <param name="user">L'utilisateur à ajouter.</param>
        public void AddUser(User user);

        public User? VerifyUser(string username, string password);
    }
}