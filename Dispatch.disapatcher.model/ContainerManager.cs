using System;
using System.Collections.Generic;

namespace Dispatch.dispatcher.network
{
    public static class ContainerManager
    {
        private static readonly Dictionary<string, (string IP, int Port)> Containers = new();

        /// <summary>
        /// Ajoute un Container à la liste des Containers actifs.
        /// </summary>
        public static void AddContainer(string name, string ip, int port)
        {
            if (!Containers.ContainsKey(name))
            {
                Containers[name] = (ip, port);
                Console.WriteLine($"🟢 Container ajouté : {name} ({ip}:{port})");
            }
            else
            {
                Console.WriteLine($"🟡 Mise à jour du Container : {name} ({ip}:{port})");
                Containers[name] = (ip, port);
            }
        }

        /// <summary>
        /// Liste les Containers actifs.
        /// </summary>
        public static void ListContainers()
        {
            Console.WriteLine("📋 Liste des Containers actifs :");
            foreach (var (name, (ip, port)) in Containers)
            {
                Console.WriteLine($"🔹 {name} - {ip}:{port}");
            }
        }
    }
}