namespace Application.ApiContracts.ManagerChat.Requests;

public class CreateManagerChatSessionRequest
{
    public string Title { get; set; } = string.Empty;
    public string InitialMessage { get; set; } = string.Empty;
}
