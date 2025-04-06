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

### Prompts IA
#### Prompt 1
```
Demande:
comment utiliser le hash SHA3-384 CSharp dotnet 8 ?

Réponse:
En .NET 8, pour utiliser l'algorithme de hachage SHA3-384, tu dois passer par la bibliothèque System.Security.Cryptography, mais .NET ne supporte pas SHA-3 nativement (à ce jour). Tu dois donc utiliser une bibliothèque externe, comme BouncyCastle.

Utilité:
Au final SHA3-384 existe en CSharp dans la lib Cryptography, donc l'unitilité est nule...
```

#### prompt2:
```
Demande:
je dois sécurisé des échanges entre deux application grâce à TLS version 1.3 minimum... Comment savoir si cette condition est respectée (condition : version >= 1.3) ?

Réponse:
Dans ton code, la ligne suivante dans TlsConnectionHandler permet d'établir la connexion TLS
Cependant, pour s'assurer que TLS 1.3 minimum est utilisé, tu dois :
Spécifier la version TLS minimale dans la méthode d'authentification.
Vérifier la version de la connexion après établissement.
Ajoute un SslServerAuthenticationOptions pour définir explicitement TLS 1.3 minimum.

Utilité:
Il m'a permis de spécifié la version minimum que le disaptcher acceptera de TLS... Dès lors l'utilité a été parfaite, réponse rapide, efficace, ...
```
#### prompt3:
```
Demande:
Salut, j'ai une question, j'ai compris l'histoire du TLS et tout, et j'arrive à m'y connecté, mais je n'avais pas encore ajouté la condition que TLS doit être >= à la version 1.3, mais de 1 il n'existe pas de version suppérieur à 1.3 et en CSHarp je ne sais pas comment faire pour dire qu'il peut etre suppérieur ou égal à une version...
De plus on dirait que le client python n'accepte pas la version 1.3 TLS...

Réponse:
En C#, tu peux spécifier la version minimale de TLS en définissant ServicePointManager.SecurityProtocol. Toutefois, il n'existe pas d'option pour spécifier "TLS 1.3 ou supérieur", car il n'existe pas de version supérieure à 1.3.
Mais attention, ServicePointManager ne fonctionne pas avec SslStream, donc si tu utilises SslStream, il faut configurer un SslProtocols spécifique

Utilité:
utilité nulle car au final c'était un problème de compatibilité entre TLS1.3 et windows 10...
```
#### Prompt4:
```
Demande:
génère moi une méthode permettant de crypter un fichier en aes256 en mode AES GCM en c# en se basant sur un tableau de bytes et la clé aes.

Réponse:
Voici une méthode en C# permettant de chiffrer un fichier en AES-256 GCM en utilisant un tableau de bytes et une clé AES. Cette implémentation utilise System.Security.Cryptography.AesGcm pour effectuer le chiffrement sécurisé.

Explications :
Utilise AesGcm (disponible en .NET Core 3.0+).

Génère un IV (Nonce) de 12 octets aléatoires.

Produit un tag d’authentification de 16 octets.

Retourne un tableau contenant [IV + Données chiffrées + Tag].

Utilité:
ça m'a grandement aidé, génération très rapide, et explication claire...
```

#### Prompt 5
```
Demande :
With SHA384 is it possible to except certain type of character in hashed name

Réponse :
No, it is not possible to exclude certain types of characters in a hash generated using SHA-384 (or any cryptographic hash function). Hash functions like SHA-384 produce a fixed-length output that is binary data often represented in hexadecimal or base64 encoding. The hash output is deterministic and cannot be controlled to exclude specific characters.

Why?

Deterministic Nature: Given the same input, SHA-384 always produces the same output.

Uniform Distribution: The hash output is a pseudo-random string of bits, meaning specific characters cannot be excluded.

One-Way Function: You cannot modify the output structure without breaking the cryptographic properties.

Workarounds

Use a Different Encoding: If you're displaying the hash, you can encode it in Base32 (which avoids certain characters).

Post-processing: You can filter out unwanted characters from the hashed string, though this reduces entropy.

Alternative Hashing Approach: If you need a custom character set, you could use a different method like HMAC with a custom encoding scheme.

Utilité :
La réponse a clarifié un malentendu fondamental sur le fonctionnement des fonctions de hachage. Elle m’a permis de comprendre qu’on ne peut pas "forcer" certains caractères ou en exclure dans un hash, car il s’agit d’un résultat binaire encodé. Cela m’a évité de perdre du temps à chercher une solution inexistante, et m’a réorienté vers des alternatives comme l'encodage Base32 si nécessaire.
```
#### Prompt 6
```
Question :
En Rust comment dire que pour les échanges multicast je dois passer par une interface réseau ??

Réponse :
C’est un peu plus bas niveau, mais voici les grandes lignes pour :

Créer un socket UDP

Joindre un groupe multicast

Sur une interface réseau spécifique

Exemple en Rust avec socket2 ou les fonctions bas niveau de std::net, où tu peux configurer l’interface avec set_multicast_if_v4() ou en passant une IpAddr spécifique via SocketAddr.

Utilité :
Très utile, car cela m’a permis de comprendre comment contraindre Rust à utiliser une interface réseau précise pour le multicast. C’est crucial dans mon projet où plusieurs interfaces sont disponibles (loopback, Wi-Fi, Ethernet). Grâce à cet échange, j’ai pu stabiliser la communication entre mes applications sur le réseau réel et éviter que les paquets restent en local.
```