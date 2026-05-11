namespace Application.Interfaces.Repositories.MediaFile.File;

public interface IFileDeleteService
{
    public bool DeleteFile(string storagePath);

    public bool DeleteFile(IEnumerable<string> storagePaths);
}
