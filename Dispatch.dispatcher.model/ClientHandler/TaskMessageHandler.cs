using System.Text;

namespace Dispatch.dispatcher.model;

public class TaskMessageHandler : IMessageHandler
{
    private readonly Protocol _protocol = Protocol.GetInstance();

    public bool CanHandle(string message)
    {
        return true;
    }

    public async Task<string?> HandleAsync(ClientHandlerContext context, string message)
    {
        var client = context.Client;
        if (client == null)
        {
            Console.WriteLine("[ERREUR] Aucun client connecté.");
            return _protocol.BuildLoginError();
        }

        byte[] buffer = Encoding.UTF8.GetBytes(message);
        int length = buffer.Length;

        if (client.AwaitingUploadContent)
        {
            var taskItem = context.MessageAnalyser.AnalyseMessage(buffer, length, client);

            if (taskItem != null && context.OnTaskReceived != null)
            {
                taskItem.CompletionSource = new TaskCompletionSource<string>();
                await context.OnTaskReceived.Invoke(taskItem);
                var result = await taskItem.CompletionSource.Task;


                if (result == "GET_OK")
                {
                    var (header, data) = context.DownloadHandler.HandleDownload(taskItem);

                    if (data == null)
                    {
                        Console.WriteLine("[UPLOAD] Erreur lors du téléchargement, data null.");
                        return _protocol.BuildDownloadError();
                    }

                    await ClientHandler.SendFileAsync(context.SslStream, header, data);
                    return null;
                }

                return result switch
                {
                    "POST_OK" => _protocol.BuildSendOk(),
                    "POST_ERROR" => _protocol.BuildSendError(),
                    "DELETE_OK" => _protocol.BuildPurgeOk(),
                    "DELETE_ERROR" => _protocol.BuildPurgeError(),
                    "GET_ERROR" => _protocol.BuildDownloadError(),
                    _ => "ERROR"
                };
            }

            return null;
        }
        
        var task = context.MessageAnalyser.AnalyseMessage(buffer, buffer.Length, client);

        if (task == null)
        {
            return null;
        }


        if (context.OnTaskReceived == null)
        {
            Console.WriteLine("[ERREUR] OnTaskReceived est null.");
            return "ERROR";
        }

        task.CompletionSource = new TaskCompletionSource<string>();
        await context.OnTaskReceived.Invoke(task);

        var resultFinal = await task.CompletionSource.Task;

        if (resultFinal == "GET_OK")
        {
            var (header, data) = context.DownloadHandler.HandleDownload(task);

            if (data == null)
            {
                Console.WriteLine("[COMMAND] Data null → erreur");
                return _protocol.BuildDownloadError();
            }

            await ClientHandler.SendFileAsync(context.SslStream, header, data);
            return null;
        }

        return resultFinal switch
        {
            "POST_OK" => _protocol.BuildSendOk(),
            "POST_ERROR" => _protocol.BuildSendError(),
            "DELETE_OK" => _protocol.BuildPurgeOk(),
            "DELETE_ERROR" => _protocol.BuildPurgeError(),
            "GET_ERROR" => _protocol.BuildDownloadError(),
            _ => "ERROR"
        };
    }
    public async Task<string?> HandleBinaryAsync(ClientHandlerContext context, byte[] buffer, int bytesRead)
    {
        var client = context.Client;
        if (client == null) return null;

        var taskItem = context.MessageAnalyser.AnalyseMessage(buffer, bytesRead, client);

        if (taskItem != null && context.OnTaskReceived != null)
        {
            taskItem.CompletionSource = new TaskCompletionSource<string>();
            await context.OnTaskReceived.Invoke(taskItem);
            var result = await taskItem.CompletionSource.Task;

            if (result == "GET_OK")
            {
                var (header, data) = context.DownloadHandler.HandleDownload(taskItem);
                if (data == null)
                    return _protocol.BuildDownloadError();

                await ClientHandler.SendFileAsync(context.SslStream, header, data);
                return null;
            }

            return result switch
            {
                "POST_OK" => _protocol.BuildSendOk(),
                "POST_ERROR" => _protocol.BuildSendError(),
                "DELETE_OK" => _protocol.BuildPurgeOk(),
                "DELETE_ERROR" => _protocol.BuildPurgeError(),
                _ => "ERROR"
            };
        }

        return null;
    }

}