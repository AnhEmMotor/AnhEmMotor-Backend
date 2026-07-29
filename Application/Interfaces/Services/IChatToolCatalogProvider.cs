namespace Application.Interfaces.Services;

public record ChatToolCatalogEntry(string Name, string Path, string Label);

public interface IChatToolCatalogProvider
{
    public IReadOnlyList<ChatToolCatalogEntry> GetCatalog();
}
