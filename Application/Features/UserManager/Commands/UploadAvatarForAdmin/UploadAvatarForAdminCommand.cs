using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.UserManager.Commands.UploadAvatarForAdmin;

public sealed record UploadAvatarForAdminCommand : IRequest<Result<string>>
{
    public required Guid UserId { get; init; }

    public required Stream FileContent { get; init; }

    public required string FileName { get; init; }
}
