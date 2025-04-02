using System.Text.RegularExpressions;

namespace Dispatch.dispatcher.model
{
    public class Protocol
    {
        private static Protocol? _instance;
        
        // Définition des regex basées sur l'ABNF
        private const string Letter = "[A-Za-z]";
        private const string Digit = "[0-9]";
        private const string LetterDigit = $"({Letter}|{Digit})";
        private const string Esp = @"\x20";
        private const string Eol = @"\x0D\x0A";
        private const string CarPass = @"[\x24-\xFF]";
        private const string Hash = "[A-Za-z0-9]{50,200}";

        // Définition des formats spécifiques
        private const string Username = $"(?<username>{LetterDigit}{{5,20}})";
        private const string Password = $"(?<password>{CarPass}{{5,50}})";
        private const string Filename = $"(?<filename>{CarPass}{{1,20}})";
        private const string Domain = $"(?<domain>{LetterDigit}{{5,20}}\\.)";
        private const string Port = $"(?<port>[0-9]{{1,5}})";
        private const string Size = $"(?<size>[0-9]{{1,10}})";
        private const string HashFilename = $"(?<hash_filename>{Hash})";
        private const string HashFileContent = $"(?<hash_filecontent>{Hash})";
        private const string InfoFile = $"{HashFilename}{Esp}{Size}{Esp}{HashFileContent}";

        // Expressions régulières pour chaque type de message
        private static readonly Regex LoginRegex = new Regex($"^LOGIN{Esp}{Username}{Esp}{Password}{Eol}$");
        private static readonly Regex RegisterRegex = new Regex($"^REGISTER{Esp}{Username}{Esp}{Password}{Eol}$");
        private static readonly Regex UploadRegex = new Regex($"^UPLOAD{Esp}{Filename}{Esp}{Size}{Eol}$");
        private static readonly Regex DownloadRegex = new Regex($"^DOWNLOAD{Esp}{Filename}{Eol}$");
        private static readonly Regex PurgeRegex = new Regex($"^PURGE{Esp}{Filename}{Eol}$");
        private static readonly Regex CatalogueRegex = new Regex($"^CATALOGUE{Eol}$");
        private static readonly Regex AliveRegex = new Regex($"^ALIVE{Esp}{Domain}{Esp}{Port}{Eol}$");
        private static readonly Regex PostRegex = new Regex($"^POST{Esp}{InfoFile}{Eol}");
        private static readonly Regex DeleteRegex = new Regex($"^DELETE{Esp}{HashFilename}{Eol}$");
        private static readonly Regex GetRegex = new Regex($"^GET{Esp}{HashFilename}{Eol}$");

        public static Protocol GetInstance()
        {
            return _instance ??= new Protocol();
        }
        
        // Méthodes de génération des messages

        public string BuildCatalogue(string[] fileName, int[] size)
        {
            if (fileName.Length != size.Length)
                throw new ArgumentException("Tout les fichiers doivent avoir une taille associée.");

            var fileInfos = fileName
                .Select((name, i) =>
                {
                    if (!IsValid(name, Filename) || !IsValid(size[i].ToString(), Size))
                        throw new ArgumentException("Format invalide pour CATALOGUE.");
                    return $"{name}#{size[i]}";
                });

            return $"INVENTORY {string.Join(" ", fileInfos)}\r\n";
        }

        public string BuildPost(string hashFilename, int size, string hashFileContent)
        {
            if (!IsValid(hashFilename, HashFilename) || !IsValid(size.ToString(), Size) ||
                !IsValid(hashFileContent, HashFileContent))
                throw new ArgumentException("Format invalide pour POST.");

            return $"POST {hashFilename} {size} {hashFileContent}\r\n";
        }

        public string BuildGet(string hashFilename)
        {
            if (!IsValid(hashFilename, HashFilename))
                throw new ArgumentException("Format invalide pour GET.");

            return $"GET {hashFilename}\r\n";
        }

        // Méthodes de parsing des messages
        public (string username, string password)? ParseLogin(string message)
        {
            Match match = LoginRegex.Match(message);
            return match.Success ? (match.Groups["username"].Value, match.Groups["password"].Value) : null;
        }

        public (string username, string password)? ParseRegister(string message)
        {
            Match match = RegisterRegex.Match(message);
            return match.Success ? (match.Groups["username"].Value, match.Groups["password"].Value) : null;
        }

        public (string filename, int size)? ParseUpload(string message)
        {
            Match match = UploadRegex.Match(message);
            return match.Success ? (match.Groups["filename"].Value, int.Parse(match.Groups["size"].Value)) : null;
        }

        public string? ParseDownload(string message)
        {
            Match match = DownloadRegex.Match(message);
            return match.Success ? match.Groups["filename"].Value : null;
        }

        public string? ParsePurge(string message)
        {
            Match match = PurgeRegex.Match(message);
            return match.Success ? match.Groups["filename"].Value : null;
        }

        public string? ParseCatalogue(string message)
        {
            Match match = CatalogueRegex.Match(message);
            return match.Success ? match.Groups["catalogue"].Value : null;
        }

        public (string domain, int port)? ParseAlive(string message)
        {
            Match match = AliveRegex.Match(message);
            return match.Success ? (match.Groups["domain"].Value, int.Parse(match.Groups["port"].Value)) : null;
        }

        private static bool IsValid(string value, string pattern) => Regex.IsMatch(value, $"^{pattern}$");

        public string BuildLoginError()
        {
            return "LOGIN_ERROR\r\n";
        }

        public string BuildLoginOk()
        {
            return "LOGIN_OK\r\n";
        }

        public string BuildSendOk()
        {
            return "UPLOAD_OK\r\n";
        }

        public string BuildSendError()
        {
            return "UPLOAD_ERROR\r\n";
        }
    }
}