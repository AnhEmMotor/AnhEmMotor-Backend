using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Services.Ai;

public class ChatToolCatalogProvider(ILogger<ChatToolCatalogProvider> logger) : IChatToolCatalogProvider
{
    private const string SolutionFileName = "AnhEmMotor-Backend.sln";
    private const string CatalogRelativePath = "SharedConfig/chat-tools-catalog.json";
    private IReadOnlyList<ChatToolCatalogEntry>? _cache;

    public IReadOnlyList<ChatToolCatalogEntry> GetCatalog()
    {
        _cache ??= LoadFromDisk();
        return _cache;
    }

    private IReadOnlyList<ChatToolCatalogEntry> LoadFromDisk()
    {
        var path = FindCatalogFile();
        if (path == null)
        {
            logger.LogWarning(
                "Không tìm thấy {RelativePath} từ {BaseDir} — tool-catalog cho FE sẽ trống.",
                CatalogRelativePath,
                AppContext.BaseDirectory);
            return [];
        }
        var json = File.ReadAllText(path);
        var raw = JsonSerializer.Deserialize<List<RawEntry>>(json) ?? [];
        return raw.Select(r => new ChatToolCatalogEntry(r.Name, r.Path, r.Label, r.Status ?? "active")).ToList();
    }

    private static string? FindCatalogFile()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, SolutionFileName)))
        {
            dir = dir.Parent;
        }
        if (dir is null)
        {
            return null;
        }
        var path = Path.Combine(dir.FullName, CatalogRelativePath);
        return File.Exists(path) ? path : null;
    }

    private record RawEntry(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("path")] string Path,
        [property: JsonPropertyName("label")] string Label,
        [property: JsonPropertyName("status")] string? Status = null);
}
