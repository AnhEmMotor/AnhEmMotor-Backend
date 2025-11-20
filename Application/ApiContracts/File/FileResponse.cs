namespace Application.ApiContracts.File;

public class FileResponse
{
    public bool IsSuccess { get; set; }
    public string? FileName { get; set; }
    public string? Url { get; set; }
}
