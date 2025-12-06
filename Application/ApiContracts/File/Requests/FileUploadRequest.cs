namespace Application.ApiContracts.File.Requests
{
    public sealed record FileUploadRequest(Stream FileContent, string FileName);
}
