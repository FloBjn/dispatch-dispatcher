namespace Dispatch.dispatcher.model
{
    public class FileSender
    {
        private readonly TcpContainer _tcpContainer;

        public FileSender(TcpContainer tcpContainer)
        {
            _tcpContainer = tcpContainer;
        }

        public void SendFile(TaskItem task)
        {
            Console.WriteLine("📡 Envoi du fichier à tous les Containers actifs...");

            if (string.IsNullOrEmpty(task.Command))
            {
                throw new ArgumentException("❌ La commande à envoyer est vide !");
            }

            // 🧠 Utilise la version non-async (multithreadée en interne)
            _tcpContainer.SendMessageToAll(task.Command);

            Console.WriteLine("✅ Commande envoyée à tous les Containers.");
        }
    }
}