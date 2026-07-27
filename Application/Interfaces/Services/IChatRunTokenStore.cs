namespace Application.Interfaces.Services;

public interface IChatRunTokenStore
{
    void Store(Guid runId, string token);
    string Take(Guid runId);
}
