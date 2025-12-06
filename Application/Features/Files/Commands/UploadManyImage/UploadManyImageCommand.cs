using MediatR;

namespace Application.Features.Files.Commands.UploadManyImage;

public sealed record UploadManyImageCommand : IRequest<List<ApiContracts.File.Responses.MediaFileResponse>>
{
    public List<ApiContracts.File.Requests.FileUploadRequest> Files { get; init; } = [];
}