# Dispatcher

## Description
Le Dispatcher sert d'intermédiaire entre :

Les Clients (connectés en TCP) qui envoient des requêtes :

- UPLOAD (ajouter un fichier)
- DOWNLOAD (récupérer un fichier)
- DELETE (supprimer un fichier)

Les Containers (connectés en TCP) qui gèrent les fichiers.
- POST (ajouter un fichier)
- GET (récupérer un fichier)
- PURGE (supprimer un fichier)

## Compilation et exécution

### .NET
```bash
# Compilation
dotnet build

# Exécution
cd .\Dispatch.dispatcher.program\
dotnet run
```

### RUST
```bash
# Compilation
cargo build

# Exécution
cargo run

# Exécutable 
.\container.exe # mettre le fichier config dans le répertoire C:\temp\config.ini
```

## Configs
### exemple pour le Dispatcher
```json
{
  "MulticastAddress": "224.123.123.123",
  "MulticastPort": 50204,
  "UnicastPort": 12345,
  "NetworkInterface": "Wi-Fi",
  "Tls": true,
  "WorkingFolderPath": "C:\\TEMP\\DISPATCHER",
  "CertificatePath": "certificat/labs25.swi.ovh.p12",
  "CertificatePassword": "labo2025",
  "Users": [
    {
      "User": {
        "Username": "louis",
        "PasswordHash": "755e3d3e6750d92efca34a94cdbd4f27f28a9dffe326cc669c9ae9b52694f20485075636655a1e4a41462f4867769d32",
        "AesKey": "lt+7ZZGToz7kTnQgYqSe1bDQaO21QHdMuztPxDiAZtY="
      },
      "Files": [
        {
          "FileName": "fichier01.txt",
          "Size": 20,
          "Containers": ["container_1"]
        }
      ]
    }
  ]
}
```

- "MulticastAddress" : adresse de multicast pour le Dispatcher
- "MulticastPort" : port de multicast pour le Dispatcher
- "UnicastPort" : port de unicast pour le Dispatcher
- "NetworkInterface" : interface réseau à utiliser pour le Dispatcher
- "Tls" : true si le Dispatcher doit utiliser TLS
- "WorkingFolderPath" : répertoire de travail pour le Dispatcher
- "CertificatePath" : chemin vers le certificat pour le Dispatcher
- "CertificatePassword" : mot de passe du certificat pour le Dispatcher



### Container
```ini
[container_config]
dispatcher_ip=127.0.0.1
container_id=container01
working_folder_path=C:\TEMP\CONTAINER1
multicast_address=224.123.123.123
multicast_port=50204
multicast_delay_in_seconds=60
unicast_port=1234
network_interface=eth0
```
- **dispatcher_ip** : adresse IP du Dispatcher
- **container_id** : identifiant du Container
- **working_folder_path** : répertoire de travail pour le Container
- **multicast_address** : adresse de multicast pour le Container
- **multicast_port** : port de multicast pour le Container
- **multicast_delay_in_seconds** : délai de multicast pour le Container
- **unicast_port** : port de unicast pour le Container
- **network_interface** : interface réseau à utiliser pour le Container

## Environnements et versions

### Version
- **.NET 8.0** pour le Dispatcher
- **cargo 1.85.1** pour le Container

## Librairies principales

### .NET
| Librairie | Version    |
|--------------|-------------|
| BouncyCastle.NetCore| 2.2.1|

### Rust
## Librairies Rust

| Librairie     | Version      |
|--------------|-------------|
| tokio        | 1           |
| regex        | 1.10        |
| lazy_static  | 1.5.0       |
| ini          | 1.3.0       |
| get_if_addrs | 0.5         |
| socket2      | 0.4         |
| sha3         | 0.11.0-pre.5|
| hex          | 0.4.3       |
| sha2         | 0.11.0-pre.5|