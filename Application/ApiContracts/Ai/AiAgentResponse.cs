namespace Application.ApiContracts.Ai;

public class AiAgentResponse<T>
{
    public T? Result { get; set; }

    public string Status { get; set; } = string.Empty;
}
