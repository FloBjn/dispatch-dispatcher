using System.Text.Json;
using Dispatch.disapatcher.model;

public class Config
{
    public string MulticastAddress { get; set; }
    public int MulticastPort { get; set; }
    public int UnicastPort { get; set; }
    public string NetworkInterface { get; set; }
    public bool Tls { get; set; }
    public string WorkingFolderPath { get; set; }
    
    public List<UserWithFiles> Users { get; set; } = new List<UserWithFiles>();

    public static Config Load(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Fichier de configuration non trouvé : {filePath}");

        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new Exception("Erreur de désérialisation du fichier JSON !");
    }

    public void Save(string filePath)
    {
        string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }
}