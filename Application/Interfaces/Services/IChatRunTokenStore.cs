namespace Application.Interfaces.Services;

public interface IChatRunTokenStore
{
    public void Store(Guid runId, string token);
    public string Take(Guid runId);
}
