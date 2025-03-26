namespace Dispatch.dispatcher.presenter;

public interface IMainView
{
    void DisplayMessageSentClient(string response);
    
    void DisplayMessageReceivedClient(string message);
    void DisplayMessageReceivedContainer(string message);
    void DisplayMainThreadMessage(string message);

}