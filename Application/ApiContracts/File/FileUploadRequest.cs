namespace Application.ApiContracts.File
{
    public sealed record FileUploadRequest(Stream FileContent, string FileName);
}
