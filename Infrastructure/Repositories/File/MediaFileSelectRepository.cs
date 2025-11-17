using Application.Interfaces.Repositories.File;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Repositories.File
{
    public class MediaFileSelectRepository(ApplicationDBContext context) : IMediaFileSelectRepository
    {
        public ValueTask<MediaFile?> GetByStoredFileNameAsync(string storedFileName, CancellationToken cancellationToken, bool includeDeleted = false)
        {
            if (includeDeleted)
            {
                var task = context.All<MediaFile>().FirstOrDefaultAsync(m => string.Compare(m.StoredFileName, storedFileName) == 0, cancellationToken);
                return new ValueTask<MediaFile?>(task);
            }

            var normalTask = context.MediaFiles.FirstOrDefaultAsync(m => string.Compare(m.StoredFileName, storedFileName) == 0, cancellationToken);
            return new ValueTask<MediaFile?>(normalTask);
        }
    }
}
