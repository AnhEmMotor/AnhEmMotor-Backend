using Application;
using Application.ApiContracts;
using Application.ApiContracts.File;

namespace Application.ApiContracts.File.Requests
{
    public sealed record FileUploadRequest(Stream FileContent, string FileName);
}
