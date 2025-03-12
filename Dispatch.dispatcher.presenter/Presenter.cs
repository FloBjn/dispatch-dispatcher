using Dispatch.disapatcher.model;

namespace Dispatch.dispatcher.presenter;

public class Presenter(
    IMainView view,
    TcpServer server,
    ClientMessageAnalyser analyser,
    IFileManager fileManager)
{
    private bool fileUploadRequest = false;
    private OurFile _currentOurFile = null;
    public async Task StartAsync()
    {
        server.OnMessageReceived += AnalyseMessageAsync;
        await server.StartAsync();
    }
    
    private Task<string> AnalyseMessageAsync(string message)
    {
        string response = analyser.Analyse(message);
        
        //Chiffrement des données du fichier UPLOAD si le cas à été détecté à la dernière analyse
        if (fileUploadRequest)
        {
            _currentOurFile.Content = message;
            fileManager.Encrypte(_currentOurFile);
            //TODO Envoyer le fichier une fois chiffrer grace au fileManager
            fileUploadRequest = false;
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
                    //TODO récupérer l'utilisateur qui upload le fichier pour avoir sa clé AES
                };
            }

            fileUploadRequest = true;
        }
        
        return Task.FromResult(string.IsNullOrEmpty(response) ? "Erreur lors de l'inscription" : response);
    }
}