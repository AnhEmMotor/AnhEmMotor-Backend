namespace Application.Interfaces.Services;

public record ChatToolCatalogEntry(string Name, string Path, string Label, string Status = "active");

public interface IChatToolCatalogProvider
{
    public IReadOnlyList<ChatToolCatalogEntry> GetCatalog();
}
