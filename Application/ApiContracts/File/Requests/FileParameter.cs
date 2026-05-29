
namespace Application.ApiContracts.File.Requests;

public class FileParameter
{
    public Stream Content { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
}
