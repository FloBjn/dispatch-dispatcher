using Dispatch.disapatcher.model;

namespace Dispatch.dispatcher.presenter;

public class Presenter(IMainView view, TcpServer server, ClientMessageAnalyser analyser, IFileManager fileManager)
{
    private bool _fileUploadRequest = false;
    private OurFile _currentOurFile = null;
    public async Task StartAsync()
    {
        server.OnMessageReceived += AnalyseMessageAsync;
        await server.StartAsync();
    }
    
    private Task<string> AnalyseMessageAsync(string message)
    {
        string response = analyser.Analyse(message);
        
        /*TODO SORTIR DANS UNE AUTRE CLASSE DU MODEL*/
        
        //Chiffrement des données du fichier UPLOAD si le cas à été détecté à la dernière analyse
        if (_fileUploadRequest)
        {
            _currentOurFile.Content = message;
            fileManager.Encrypte(_currentOurFile);
            //TODO Envoyer le fichier une fois chiffrer grace au fileManager
            _fileUploadRequest = false;
            _currentOurFile = null;
        }
        //Détection du cas particulier UPLOAD
        if (response.Equals("UPLOAD"))
        {
            var parsedResult = DispatcherProtocol.ParseUpload(message);

            if (parsedResult.HasValue)
            {
                (string fileName, int size) = parsedResult.Value;
                
                _currentOurFile = new OurFile
                {
                    FileName = fileName,
                    FileSize = size,
                    //TODO récupérer l'utilisateur qui upload le fichier pour avoir sa clé AES --> into cryptage
                };
            }

            _fileUploadRequest = true;
        }
        
        /*SORTIR DANS UNE AUTRE CLASSE DU MODEL*/
        
        return Task.FromResult(string.IsNullOrEmpty(response) ? "Erreur lors de l'inscription" : response);
    }
}