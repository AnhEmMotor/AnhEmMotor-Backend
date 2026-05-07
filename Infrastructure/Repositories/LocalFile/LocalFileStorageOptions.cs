
namespace Infrastructure.Repositories.LocalFile;

public class LocalFileStorageOptions
{
    public const string SectionName = "LocalFileStorage";

    public string? UploadPath { get; set; }
}
