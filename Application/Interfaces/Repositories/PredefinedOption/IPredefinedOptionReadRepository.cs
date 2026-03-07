namespace Application.Interfaces.Repositories.PredefinedOption;

public interface IPredefinedOptionReadRepository
{
    public Task<Dictionary<string, string>> GetAllAsDictionaryAsync(CancellationToken cancellationToken);

    public Task<IReadOnlyCollection<string>> GetAllKeysAsync(CancellationToken cancellationToken);
}
