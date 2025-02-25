using Dispatch.disapatcher.model;
using File = Dispatch.disapatcher.model.File;

namespace Dispatch.dispatcher.presenter;

public class Presenter(IMainView view, TcpServer server, ClientMessageAnalyser analyser, FileEncryptor fileEncryptor)
{
    private bool fileUploadRequest = false;
    private File currentFile = null;
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
            fileEncryptor.Encrypte(currentFile);
            fileUploadRequest = false;
            currentFile = null;
        }
        //Détection du cas particulier UPLOAD
        if (response.Equals("UPLOAD"))
        {
            var parsedResult = DispatcherProtocol.ParseUpload(message);

            if (parsedResult.HasValue)
            {
                (string fileName, int size) = parsedResult.Value;
                
                currentFile = new File
                {
                    FileName = fileName,
                    FileSize = size,
                    //TODO utiliser fileManager pour transmettre le user?
                };
            }

            fileUploadRequest = true;
        }
        
        return Task.FromResult(string.IsNullOrEmpty(response) ? "Erreur lors de l'inscription" : response);
    }
}