using Dispatch.dispatcher.presenter;

namespace Dispatch.dispatcher.view;

public class View : IMainView
{
    public void DisplayMessageSentClient(string response)
    {
        Console.WriteLine("[UNICAST SENT CLIENT] : " + response);
    }

    public void DisplayMessageReceivedClient(string message)
    {
        Console.WriteLine("[UNICAST RECEIVED CLIENT] : " + message);
    }

    public void DisplayMessageReceivedContainer(string message)
    {
        Console.WriteLine("[MULTICAST RECEIVED CONTAINER] : " + message);
    }
    public void DisplayMainThreadMessage(string message)
    {
        Console.WriteLine("[MAIN THREAD] : " + message);
    }

}