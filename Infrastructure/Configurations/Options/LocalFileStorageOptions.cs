namespace Infrastructure.Configurations.Options;

public class LocalFileStorageOptions
{
    public const string SectionName = "LocalFileStorage";
    public string UploadPath { get; set; } = string.Empty;
}
