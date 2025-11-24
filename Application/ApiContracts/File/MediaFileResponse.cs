namespace Application.ApiContracts.File;

public class MediaFileResponse
{
    public int? Id { get; set; }
    public string? StorageType { get; set; }
    public string? StoragePath { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public string? FileExtension { get; set; }
    public long? FileSize { get; set; }
    public string? PublicUrl { get; set; }
}
