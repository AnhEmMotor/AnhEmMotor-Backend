using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.LocalFile;
using Application.Interfaces.Repositories.MediaFile;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Users.Commands.UploadAvatar;

public class UploadAvatarCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository,
    IFileStorageService fileStorageService,
    IMediaFileInsertRepository mediaFileInsertRepository,
    IMediaFileReadRepository mediaFileReadRepository,
    IMediaFileUpdateRepository mediaFileUpdateRepository,
    IUnitOfWork unitOfWork,
    IUserStreamService userStreamService) : IRequestHandler<UploadAvatarCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            return Error.BadRequest("Invalid user ID.");
        }

        var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }

        if(user.DeletedAt is not null)
        {
            return Error.Forbidden("User account is deleted.");
        }

        if(string.Compare(user.Status, Domain.Constants.UserStatus.Banned) == 0)
        {
            return Error.Forbidden("User account is banned.");
        }

        var saveResult = await fileStorageService.SaveFileAsync(request.FileContent, cancellationToken, "avatars")
            .ConfigureAwait(false);

        if(saveResult.IsFailure)
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

        if(!string.IsNullOrEmpty(user.AvatarUrl))
        {
            var oldPath = ExtractStoragePath(user.AvatarUrl);
            if(!string.IsNullOrEmpty(oldPath))
            {
                var oldMediaFile = await mediaFileReadRepository.GetByStoragePathAsync(oldPath, cancellationToken)
                    .ConfigureAwait(false);

                if(oldMediaFile is not null)
                {
                    oldMediaFile.DeletedAt = DateTimeOffset.UtcNow;
                    mediaFileUpdateRepository.Update(oldMediaFile);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        user.AvatarUrl = fileStorageService.GetPublicUrl(savedFile.StoragePath);

        var (succeeded, _) = await userUpdateRepository.UpdateUserAsync(user, cancellationToken).ConfigureAwait(false);

        if(!succeeded)
        {
            fileStorageService.DeleteFile(savedFile.StoragePath);
            return Error.Validation("Failed to update user avatar.", "UpdateFailed");
        }

        userStreamService.NotifyUserUpdate(user.Id);

        return user.AvatarUrl;
    }

    private string? ExtractStoragePath(string? publicUrl)
    {
        if(string.IsNullOrEmpty(publicUrl))
            return null;

        var marker = "view-image/";
        var index = publicUrl.IndexOf(marker);
        if(index != -1)
        {
            return publicUrl.Substring(index + marker.Length);
        }

        return null;
    }
}
