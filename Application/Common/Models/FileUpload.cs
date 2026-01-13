namespace Application.Common.Models;

public sealed record FileUpload(string StoragePath, string Extension, long Size);