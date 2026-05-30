using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.MediaFile.MediaFile;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using MediatR;

namespace Application.Features.UserManager.Commands.UploadAvatarForAdmin;

public class UploadAvatarForAdminCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository,
    IFileReadService fileReadService,
    IFileInsertService fileInsertService,
    IFileDeleteService fileDeleteService,
    IMediaFileInsertRepository mediaFileInsertRepository,
    IMediaFileReadRepository mediaFileReadRepository,
    IMediaFileUpdateRepository mediaFileUpdateRepository,
    IUnitOfWork unitOfWork,
    IUserStreamService userStreamService) : IRequestHandler<UploadAvatarForAdminCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UploadAvatarForAdminCommand request, CancellationToken cancellationToken)
    {
        var user = await userReadRepository.FindUserByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return Error.NotFound("User not found.");
        }
        if (user.DeletedAt is not null)
        {
            return Error.Forbidden("User account is deleted.");
        }
        if (string.Compare(user.Status, UserStatus.Banned) == 0)
        {
            return Error.Forbidden("User account is banned.");
        }
        var saveResult = await fileInsertService.SaveFileAsync(request.FileContent, cancellationToken, "avatars")
            .ConfigureAwait(false);
        if (saveResult.IsFailure)
        {
            return Result<string>.Failure(saveResult.Error!);
        }
        var savedFile = saveResult.Value;
        var mediaFile = new MediaFile
        {
            StorageType = "local",
            StoragePath = savedFile.StoragePath,
            OriginalFileName = request.FileName,
            ContentType = "image/webp",
            FileExtension = savedFile.Extension,
            FileSize = savedFile.Size
        };
        mediaFileInsertRepository.Add(mediaFile);
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            var oldPath = ExtractStoragePath(user.AvatarUrl);
            if (!string.IsNullOrEmpty(oldPath))
            {
                var oldMediaFile = await mediaFileReadRepository.GetByStoragePathAsync(oldPath, cancellationToken)
                    .ConfigureAwait(false);
                if (oldMediaFile is not null)
                {
                    oldMediaFile.DeletedAt = DateTimeOffset.UtcNow;
                    mediaFileUpdateRepository.Update(oldMediaFile);
                }
            }
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        user.AvatarUrl = fileReadService.GetPublicUrl(savedFile.StoragePath);
        var (succeeded, _) = await userUpdateRepository.UpdateUserAsync(user, cancellationToken).ConfigureAwait(false);
        if (!succeeded)
        {
            fileDeleteService.DeleteFile(savedFile.StoragePath);
            return Error.Validation("Failed to update user avatar.", "UpdateFailed");
        }
        userStreamService.NotifyUserUpdate(user.Id);
        return user.AvatarUrl;
    }

    private string? ExtractStoragePath(string? publicUrl)
    {
        if (string.IsNullOrEmpty(publicUrl))
            return null;
        var marker = "view-image/";
        var index = publicUrl.IndexOf(marker);
        if (index != -1)
        {
            return publicUrl.Substring(index + marker.Length);
        }
        return null;
    }
}
