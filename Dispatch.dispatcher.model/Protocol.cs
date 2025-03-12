using System.Text.RegularExpressions;

namespace Dispatch.dispatcher.model
{
    
    public static class DispatcherProtocol
    {
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

        // Méthodes de génération des messages
        public static string BuildLogin(string username, string password)
        {
            if (!IsValid(username, Username) || !IsValid(password, Password))
                throw new ArgumentException("Format invalide pour LOGIN.");

            return $"LOGIN {username} {password}\r\n";
        }

        public static string BuildRegister(string username, string password)
        {
            if (!IsValid(username, Username) || !IsValid(password, Password))
                throw new ArgumentException("Format invalide pour REGISTER.");

            return $"REGISTER {username} {password}\r\n";
        }

        public static string BuildUpload(string filename, int size)
        {
            if (!IsValid(filename, Filename) || !IsValid(size.ToString(), Size))
                throw new ArgumentException("Format invalide pour UPLOAD.");

            return $"UPLOAD {filename} {size}\r\n";
        }

        public static string BuildDownload(string filename)
        {
            if (!IsValid(filename, Filename))
                throw new ArgumentException("Format invalide pour DOWNLOAD.");

            return $"DOWNLOAD {filename}\r\n";
        }

        public static string BuildPurge(string filename)
        {
            if (!IsValid(filename, Filename))
                throw new ArgumentException("Format invalide pour PURGE.");

            return $"PURGE {filename}\r\n";
        }

        public static string BuildCatalogue()
        {
            return "CATALOGUE\r\n";
        }

        public static string BuildAlive(string domain, int port)
        {
            if (!IsValid(domain, Domain) || !IsValid(port.ToString(), Port))
                throw new ArgumentException("Format invalide pour ALIVE.");

            return $"ALIVE {domain} {port}\r\n";
        }

        public static string BuildPost(string hashFilename, int size, string hashFileContent)
        {
            if (!IsValid(hashFilename, HashFilename) || !IsValid(size.ToString(), Size) || !IsValid(hashFileContent, HashFileContent))
                throw new ArgumentException("Format invalide pour POST.");

            return $"POST {hashFilename} {size} {hashFileContent}\r\n";
        }

        public static string BuildDelete(string hashFilename)
        {
            if (!IsValid(hashFilename, HashFilename))
                throw new ArgumentException("Format invalide pour DELETE.");

            return $"DELETE {hashFilename}\r\n";
        }

        public static string BuildGet(string hashFilename)
        {
            if (!IsValid(hashFilename, HashFilename))
                throw new ArgumentException("Format invalide pour GET.");

            return $"GET {hashFilename}\r\n";
        }

        // Méthodes de parsing des messages
        public static (string username, string password)? ParseLogin(string message)
        {
            Match match = LoginRegex.Match(message);
            return match.Success ? (match.Groups["username"].Value, match.Groups["password"].Value) : null;
        }

        public static (string username, string password)? ParseRegister(string message)
        {
            Match match = RegisterRegex.Match(message);
            return match.Success ? (match.Groups["username"].Value, match.Groups["password"].Value) : null;
        }

        public static (string filename, int size)? ParseUpload(string message)
        {
            Match match = UploadRegex.Match(message);
            return match.Success ? (match.Groups["filename"].Value, int.Parse(match.Groups["size"].Value)) : null;
        }

        public static string? ParseDownload(string message)
        {
            Match match = DownloadRegex.Match(message);
            return match.Success ? match.Groups["filename"].Value : null;
        }

        public static string? ParsePurge(string message)
        {
            Match match = PurgeRegex.Match(message);
            return match.Success ? match.Groups["filename"].Value : null;
        }

        public static string? ParseCatalogue(string message)
        {
            Match match = CatalogueRegex.Match(message);
            return match.Success ? match.Groups["catalogue"].Value : null;
        }

        public static (string domain, int port)? ParseAlive(string message)
        {
            Match match = AliveRegex.Match(message);
            return match.Success ? (match.Groups["domain"].Value, int.Parse(match.Groups["port"].Value)) : null;
        }

        public static (string hashFilename, int size, string hashFileContent)? ParsePost(string message)
        {
            Match match = PostRegex.Match(message);
            return match.Success ? (match.Groups["hash_filename"].Value, int.Parse(match.Groups["size"].Value), match.Groups["hash_filecontent"].Value) : null;
        }

        public static string? ParseDelete(string message)
        {
            Match match = DeleteRegex.Match(message);
            return match.Success ? match.Groups["hash_filename"].Value : null;
        }

        public static string? ParseGet(string message)
        {
            Match match = GetRegex.Match(message);
            return match.Success ? match.Groups["hash_filename"].Value : null;
        }

        private static bool IsValid(string value, string pattern) => Regex.IsMatch(value, $"^{pattern}$");

        public static string BuildLoginOk()
        {
            return "LOGIN_OK\r\n";
        }

        public static string BuildLoginEr()
        {
            return "LOGIN_ERROR\r\n";
        }
    }
}
