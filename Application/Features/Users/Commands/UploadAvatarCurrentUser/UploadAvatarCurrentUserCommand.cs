using Application.Common.Models;
using MediatR;
using System.IO;

namespace Application.Features.Users.Commands.UploadAvatarCurrentUser;

public sealed record UploadAvatarCurrentUserCommand : IRequest<Result<string>>
{
    public required Stream FileContent { get; init; }

    public required string FileName { get; init; }
}
