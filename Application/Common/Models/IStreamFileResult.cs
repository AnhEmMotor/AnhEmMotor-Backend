
namespace Application.Common.Models;

public interface IStreamFileResult
{
    public Stream FileStream { get; }

    public string ContentType { get; }
}
