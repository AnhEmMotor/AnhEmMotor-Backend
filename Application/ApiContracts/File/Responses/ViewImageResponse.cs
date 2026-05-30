using Application.Common.Models;

namespace Application.ApiContracts.File.Responses;

public class ViewImageResponse : IStreamFileResult
{
    public Stream FileStream { get; set; } = null!;

    public string ContentType { get; set; } = string.Empty;
}
