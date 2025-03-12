using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Dispatch.dispatcher.network
{
    public class ContainerManager
    {
        private readonly Dictionary<string, (string IP, int Port)> _containers = new();

        /// <summary>
        /// Ajoute un Container à la liste des Containers actifs.
        /// </summary>
        public void AddContainer(string name, string ip, int port)
        {
            if (!_containers.ContainsKey(name))
            {
                _containers[name] = (ip, port);
                Console.WriteLine($"🟢 Container ajouté : {name} ({ip}:{port})");
            }
            else
            {
                Console.WriteLine($"🟡 Mise à jour du Container : {name} ({ip}:{port})");
                _containers[name] = (ip, port);
            }
        }

        /// <summary>
        /// Liste les Containers actifs.
        /// </summary>
        public void ListContainers()
        {
            Console.WriteLine("📋 Liste des Containers actifs :");
            foreach (var (name, (ip, port)) in _containers)
            {
                Console.WriteLine($"🔹 {name} - {ip}:{port}");
            }
        }

        public List<(string, int)> GetContainersIp()
        {
            var containers = new List<(string, int)>();

            foreach (var container in _containers)
            {
                containers.Add((container.Value.Item1, container.Value.Item2));
            }

            return containers;
        }

        /// <summary>
        /// Méthode temporaire pour récupérer le premier container de la liste
        /// </summary>
        /// <returns></returns>
        public (string, int) GetFirstContainerIp()
        {
            return GetContainersIp().First();
        }
    }
}