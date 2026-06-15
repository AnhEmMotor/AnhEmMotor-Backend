using Application.ApiContracts.News.Responses;
using MediatR;

using Microsoft.AspNetCore.Http;

namespace Application.Features.News.Commands.UploadNewsContentImage;

public sealed record UploadNewsContentImageCommand : IRequest<UploadNewsContentImageResponse>
{
    public IFormFile File { get; init; } = null!;
}
