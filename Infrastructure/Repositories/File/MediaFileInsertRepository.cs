using Application.Interfaces.Repositories.File;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.File;

public class MediaFileInsertRepository(ApplicationDBContext context) : IMediaFileInsertRepository
{
    public void Add(MediaFile mediaFile)
    {
        context.MediaFiles.Add(mediaFile);
    }
}