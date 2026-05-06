using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Files.Commands.UploadBannerImage
{
    public class UploadBannerImageCommand : IRequest<Result<MediaFileResponse>>
    {
        public Stream FileContent { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
    }
}
