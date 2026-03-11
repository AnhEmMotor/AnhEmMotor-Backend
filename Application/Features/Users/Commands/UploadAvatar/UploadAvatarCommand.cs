using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Commands.UploadAvatar;

public sealed record UploadAvatarCommand : IRequest<Result<string>>
{
    public string? UserId { get; init; }
    public required Stream FileContent { get; init; }
    public required string FileName { get; init; }
}
